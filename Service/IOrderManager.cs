using System.Collections.Generic;
using PengeSoft.Enterprise.Appication;
using PengeSoft.Service;
using Pydc.Domain;

namespace Pydc.Service
{
    /// <summary>
    /// 订单管理
    /// </summary>
    [PublishName("OrderManager")]
    public interface IOrderManager : IApplication
    {
        /// <summary>
        /// 取订单汇总
        /// </summary>
        /// <param name="userId">用户标识</param>
        /// <param name="date">日期，格式yyyyMMdd，空表示当天</param>
        /// <param name="option">汇总选项，0x01: 用户标识，0x02: 部门标识，0x04: 产品, 0x08: 备注</param>
        /// <returns>订单表</returns>
        [PublishMethod]
        Orders GetOrders(string userId, string date, int option);

        /// <summary>
        /// 删除订单
        /// </summary>
        /// <param name="userId">用户标识</param>
        [PublishMethod]
        void Delete(string userId);
        /// <summary>
        /// 取订单信息
        /// </summary>
        /// <param name="userId">用户标识</param>
        /// <param name="date">日期，格式yyyyMMdd，空表示当天</param>
        /// <returns>订单，不存在返回空对象</returns>
        [PublishMethod]
        Order Get(string userId, string date);

        /// <summary>
        /// 取订最后一次订单信息
        /// </summary>
        /// <param name="userId">用户标识</param>
        /// <returns>订单，不存在返回空对象</returns>
        [PublishMethod]
        Order GetLast(string userId);

        /// <summary>
        /// 下单
        /// </summary>
        /// <param name="userId">用户标识</param>
        /// <param name="product">产品</param>
        /// <param name="count">数量</param>
        /// <param name="memo">备注</param>
        /// <returns>出错码，0：成功, -1: 失败  -2: 不在订餐时间</returns>
        [PublishMethod]
        int PlaceOrder(string userId, string product, int count, string memo);

        /// <summary>
        /// 完成订单订单
        /// </summary>
        /// <param name="userId">用户标识</param>
        /// <returns>出错码, -2001: 订餐时间不能结束, -2002: 已发送过, -2003: 没有订单</returns>
        [PublishMethod]
        int FinishedOrder(string userId);

        /// <summary>
        /// 取系统参数
        /// </summary>
        /// <param name="userId">用户标识</param>
        /// <returns>系统参数</returns>
        [PublishMethod]
        SysParams GetSysParams(string userId);

        /// <summary>
        /// 更新系统参数
        /// </summary>
        /// <param name="userId">用户标识</param>
        /// <param name="paras">系统参数</param>
        /// <param name="option">选项, 1: 重置  2: 更新管理员信息</param>
        /// <returns>出错码, 0: 成功, -1: 未指定参数  -2: 没有权限</returns>
        [PublishMethod]
        int UpdateSysParams(string userId, SysParams paras, int option);

        /// <summary>
        /// 发送订单通知
        /// </summary>
        /// <param name="userId">用户标识</param>
        /// <param name="mobs">手机号码表，‘，’号分格，不输入使用系统参数</param>
        /// <param name="option">0x01：同时通知系统管理员, 0x02: 同时使用系统联系人手机</param>
        /// <returns>出错码，0:成功， -1：发送失败，-1001：不在时间范围，-1002：无订单，-1003：无联系人</returns>
        [PublishMethod]
        int SendNotify(string userId, string mobs, int option);
    }
}