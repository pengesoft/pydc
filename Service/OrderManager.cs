using PengeSoft.Cache;
using PengeSoft.Client;
using PengeSoft.Common;
using PengeSoft.DingTalk;
using PengeSoft.Enterprise.Application;
using PengeSoft.Service;
using Pydc.Domain;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore.Internal;

namespace Pydc.Service
{
    internal class SendStatus
    {
        public bool Placed = false;
        public bool Finished = false;
    }

    /// <summary>
    /// 订单管理
    /// </summary>
    [PublishName("OrderManager")]
    public class OrderManager : ApplicationBase, IOrderManager
    {
        // redis config
        private RedisClient _redis = null;
        private IAuther auther = null;
        private ICache _db = null;
        private const string Key_Sys_Params = "Sys_Params";
        private const string PreKey_Order = "Order_";
        private const string Key_LastOrder = "LastOrder";
        private Dictionary<string, SendStatus> _sendNotify;
        private Dictionary<int, string> _deptName;

        /// <summary>
        /// 构造函数
        /// </summary>
        public OrderManager()
            : base()
        {
            _redis = new RedisClient("127.0.0.1", 6379, "", 2);
            _db = new SSRedisCache(_redis, "dcsj");
            auther = ServiceManager.GetService<IAuther>("DingTalk.Auther", true);
            _sendNotify = new Dictionary<string, SendStatus>();

            _deptName = new Dictionary<int, string>();

            Initial();
        }

        private void Initial()
        {
            if (_deptName.Count > 0)
                return;

            try
            {
                var svr = RometeClientManager.GetClient<IDepartmentAPI>("https://oapi.dingtalk.com/department");
                var tempDepts = svr.list(auther.GetAccessToken(), 1, true);
                Dictionary<int, Department> depts = new Dictionary<int, Department>();
                foreach (Department dept in tempDepts.department)
                    depts.Add(dept.id, dept);
                foreach (Department dept in tempDepts.department)
                {
                    Department dp = dept;
                    while (dp.parentid != 1)
                        dp = depts[dp.parentid];
                    _deptName.Add(dept.id, dp.name);
                }
            }
            catch
            {
                _deptName.Clear();
            }
        }

        /// <summary>
        /// 取订单汇总
        /// </summary>
        /// <param name="userId">用户标识</param>
        /// <param name="date">日期，格式yyyyMMdd，空表示当天</param>
        /// <param name="option">汇总选项，0x01: 用户标识，0x02: 部门标识，0x04: 产品, 0x08: 备注</param>
        /// <returns>订单表</returns>
        [PublishMethod]
        public Orders GetOrders(string userId, string date, int option)
        {
            if (string.IsNullOrWhiteSpace(date))
                date = GetCurrentDate();
            string ordersKey = GetOrdersKey(date);
            Dictionary<string, Order> total = new Dictionary<string, Order>();
            var values = _db.HGetAll(ordersKey);
            if (values != null)
            {
                foreach (KeyValuePair<string, object> item in values)
                {
                    Order ord = (Order)item.Value;
                    if (ord != null)
                    {
                        string key = GetTotalKey(ord, option);
                        Order value = null;
                        if (!total.TryGetValue(key, out value))
                        {
                            value = ord;
                            total.Add(key, value);
                        }
                        else
                            value.Count += ord.Count;
                    }
                }
            }

            return new Orders(total.Values);
        }

        private string GetOrdersKey(string date)
        {
            return PreKey_Order + date;
        }

        private string GetTotalKey(Order ord, int option)
        {
            StringBuilder key = new StringBuilder();
            key.Append("key");
            if ((option & 0x01) != 0)
            {
                key.Append('|');
                key.Append(ord.UserId);
            }
            if ((option & 0x02) != 0)
            {
                key.Append('|');
                key.Append(ord.DeptId);
            }
            if ((option & 0x04) != 0)
            {
                key.Append('|');
                key.Append(ord.Product);
            }
            if ((option & 0x08) != 0)
            {
                key.Append('|');
                key.Append(ord.Memo);
            }

            return key.ToString();
        }

        /// <summary>
        /// 取订单信息
        /// </summary>
        /// <param name="userId">用户标识</param>
        /// <param name="date">日期，格式yyyyMMdd，空表示当天</param>
        /// <returns>订单，不存在返回空对象</returns>
        [PublishMethod]
        public Order Get(string userId, string date)
        {
            if (string.IsNullOrWhiteSpace(date))
                date = GetCurrentDate();
            string ordersKey = GetOrdersKey(date);
            Order value = null; 
            _db.HGet<Order>(ordersKey, userId, out value);
            return value;
        }

        /// <summary>
        /// 取订最后一次订单信息
        /// </summary>
        /// <param name="userId">用户标识</param>
        /// <returns>订单，不存在返回空对象</returns>
        [PublishMethod]
        public Order GetLast(string userId)
        {
            Order value = null;
            _db.HGet<Order>(Key_LastOrder, userId, out value);
            return value;
        }

        /// <summary>
        /// 下单
        /// </summary>
        /// <param name="userId">用户标识</param>
        /// <param name="product">产品</param>
        /// <param name="count">数量</param>
        /// <param name="memo">备注</param>
        /// <returns>出错码，0：成功, -1: 失败  -2: 不在订餐时间</returns>
        [PublishMethod]
        public int PlaceOrder(string userId, string product, int count, string memo)
        {
            var userinfo = auther.GetUserinfo(userId);
            if (userinfo == null)
                return -1;

            Initial();
            string deptName = "";
            if (userinfo.department != null && userinfo.department.Length > 0)
                _deptName.TryGetValue(userinfo.department[0], out deptName);

            SysParams sysPara = GetSysParams("");

#if !DEBUG
            DateTime dt = DateTime.Now;
            int dt0 = dt.Hour * 60 + dt.Minute;
            if ((dt0 > sysPara.Deadline) || (dt0 + 120 < sysPara.Deadline))
                //throw new Exception("不在订餐时间");
                return -2;
#endif

            Order value = new Order();
            value.UserId = userId;
            value.Name = userinfo.name;
            value.DeptId = deptName;
            if (count <= 0)
                count = 1;
            value.Count = count;
            value.Product = product;
            value.Memo = memo;
            value.CheckTime = DateTime.Now;
            string ordersKey = GetOrdersKey(GetCurrentDate());
            _db.HPut(ordersKey, userId, value);
            _db.HPut(Key_LastOrder, userId, value);

            return 0;
        }

        /// <summary>
        /// 删除订单
        /// </summary>
        /// <param name="userId">用户标识</param>
        [PublishMethod]
        public void Delete(string userId)
        {
            string ordersKey = GetOrdersKey(GetCurrentDate());
            _db.HDel(ordersKey, userId);
        }

        private static string GetCurrentDate()
        {
            return DateTime.Now.ToString("yyyyMMdd");
        }

        /// <summary>
        /// 完成订单订单
        /// </summary>
        /// <param name="userId">用户标识</param>
        /// <returns>出错码, -2001: 订餐时间不能结束, -2002: 已发送过, -2003: 没有订单</returns>
        [PublishMethod]
        public int FinishedOrder(string userId)
        {
            SysParams sysPara = GetSysParams("");
            DateTime dt = DateTime.Now;
            int dt0 = dt.Hour * 60 + dt.Minute;
            if (dt0 < sysPara.Deadline)
                return -2001;

            string key = GetCurrentDate();
            if (!_sendNotify.TryGetValue(key, out SendStatus status))
            {
                status = new SendStatus();
                _sendNotify.Add(key, status);
            }
            if (!status.Finished)
            {
                var orders = GetOrders(userId, "", 1).ToList();
                var productType = orders.First(p => p.UserId == userId).Product.Split('-')[0];
                var theSameTypeOrder = orders.Where(p => p.Product.StartsWith(productType));
                if (theSameTypeOrder.Any())
                {
                    var ids = theSameTypeOrder.Select(p => p.UserId).Join(",");
                    string msg = $"小伙伴，{productType}到了，快来取哦！";
                    Utils.SendNotifyTextMsgEx(auther.GetAccessToken(), auther.AgentId, ids, msg);

                    return 0;
                }
                //if (orders.Count > 0)
                //{
                //    StringBuilder ids = new StringBuilder();
                //    foreach (Order order in orders)
                //    {
                //        if (userId != order.UserId)
                //        {
                //            ids.Append(order.UserId);
                //            ids.Append(',');
                //        }
                //    }
                //    ids.Length = ids.Length - 1;
                //    string msg = "小伙伴，餐到了，快来取哦！";
                //    Utils.SendNotifyTextMsgEx(auther.GetAccessToken(), auther.AgentId, ids.ToString(), msg);

                //    return 0;
                //}
                else
                    return -2003;
            }
            else
                return -2002;
        }

        /// <summary>
        /// 取系统参数
        /// </summary>
        /// <param name="userId">用户标识</param>
        /// <returns>系统参数</returns>
        [PublishMethod]
        public SysParams GetSysParams(string userId)
        {
            SysParams sysPara = null;
            if (!_db.Get<SysParams>(Key_Sys_Params, out sysPara))
            {
                sysPara = ConfigurationManager.GetSection<SysParams>("SysParams");
                if (sysPara == null)
                {
                    sysPara = new SysParams();
                    sysPara.ContactMob = "";
                    sysPara.AdminUserId = "";
                    sysPara.Deadline = 17 * 60 + 30;
                    sysPara.Title = "";
                    sysPara.Description = "";
                }
            }

            return sysPara;
        }

        /// <summary>
        /// 更新系统参数
        /// </summary>
        /// <param name="userId">用户标识</param>
        /// <param name="paras">系统参数</param>
        /// <param name="option">选项, 1: 重置  2: 更新管理员信息</param>
        /// <returns>出错码, 0: 成功, -1: 未指定参数  -2: 没有权限</returns>
        [PublishMethod]
        public int UpdateSysParams(string userId, SysParams paras, int option)
        {
            var sysParas = GetSysParams(userId);
            if ((option != 2) && (userId != sysParas.AdminUserId))
                return -2;

            if ((option == 1) && (paras == null))
            {
                paras = sysParas;
            }
            if (paras != null)
            {
                if (option != 2)
                    paras.AdminUserId = sysParas.AdminUserId;

                _db.Put(Key_Sys_Params, paras);
                return 0;
            }
            else
                return -1;
        }

        /// <summary>
        /// 发送订单通知
        /// </summary>
        /// <param name="userId">用户标识</param>
        /// <param name="mobs">手机号码表，‘，’号分格，不输入使用系统参数</param>
        /// <param name="option">0x01: 检查发送时间，0x02：同时通知系统管理员, 0x04: 同时使用系统联系人手机</param>
        /// <returns>出错码，0:成功， -1：发送失败，-1001：不在时间范围，-1002：无订单，-1003：无联系人</returns>
        [PublishMethod]
        public int SendNotify(string userId, string mobs, int option)
        {
            DateTime dt = DateTime.Now;
            int dt0 = dt.Hour * 60 + dt.Minute;
            SysParams sysPars = GetSysParams(userId);
            int errcode = -1;
            if (((option & 0x01) == 0) || (dt0 > sysPars.Deadline))
            {
                var orders = GetOrders(userId, "", 12);
                if (orders == null || orders.Count == 0)
                {
                    return -1002;
                }
                else
                {
                    StringBuilder msg = new StringBuilder();
                    foreach (var order in orders)
                    {
                        msg.Append($"品种:{order.Product}-{order.Memo} 数量;{order.Count}\n");
                    }
                    string msgText = msg.ToString();
                    if (!string.IsNullOrWhiteSpace(sysPars.AdminUserId) && (option & 0x02) != 0)
                    {
                        var rst = Utils.SendNotifyTextMsg(auther.GetAccessToken(), auther.AgentId, sysPars.AdminUserId, msgText);
                        if (rst.errcode == 0)
                            errcode = 0;
                    }
                    if ((string.IsNullOrEmpty(mobs) || (option & 0x04) != 0) && (!string.IsNullOrWhiteSpace(sysPars.ContactMob)))
                        mobs = mobs + "," + sysPars.ContactMob;
                    if (!string.IsNullOrWhiteSpace(mobs))
                    {
                        var svr = RometeClientManager.GetClient<INotifyMsgService>("https://oa.p9i.cn/Service/MsgNotifyJson.assx");
                        var ans = svr.SendMsgToMobs("", "", mobs, msgText);
                        if (ans.Success)
                            errcode = 0;
                    }
                    else
                        errcode = -1003;
                }
            }
            else
                errcode = -1001;

            return errcode;
        }
    }
}
