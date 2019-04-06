using PengeSoft.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pydc.Domain
{
    /// <summary>
    /// SysParams 点餐系统参数的摘要说明。
    /// </summary>
    public partial class SysParams : DataPacket
    {
        #region 私有字段

        private string _ContactMob;      // 服务商联系人手机
        private string _AdminUserId;      // 管理员钉钉UserId
        private int _Deadline;      // 订餐截止时间
        private string[] _Products;      // 可选产品

        #endregion

        #region 属性定义

        /// <summary>
        /// 服务商联系人手机 
        /// </summary>
        public string ContactMob { get { return _ContactMob; } set { _ContactMob = value; } }
        /// <summary>
        /// 管理员钉钉UserId 
        /// </summary>
        public string AdminUserId { get { return _AdminUserId; } set { _AdminUserId = value; } }
        /// <summary>
        /// 订餐截止时间 
        /// 从0点计算的当天分钟数 
        /// </summary>
        public int Deadline { get { return _Deadline; } set { _Deadline = value; } }
        /// <summary>
        /// 可选产品 
        /// </summary>
        public string[] Products { get { return _Products; } set { _Products = value; } }

        #endregion

        #region 重载公共函数

        /// <summary>
        /// 清除所有数据。
        /// </summary>
        public override void Clear()
        {
            base.Clear();

            _ContactMob = null;
            _AdminUserId = null;
            _Deadline = 0;
            _Products = new string[0];
        }

#if SILVERLIGHT
#else
        /// <summary>
        /// 用指定节点序列化整个数据对象。
        /// </summary>
        /// <param name="node">用于序列化的 XmlNode 节点。</param>
        public override void XMLEncode(System.Xml.XmlNode node)
        {
            base.XMLEncode(node);

            WriteXMLValue(node, "ContactMob", _ContactMob);
            WriteXMLValue(node, "AdminUserId", _AdminUserId);
            WriteXMLValue(node, "Deadline", _Deadline);
            WriteXMLValue(node, "Products", _Products);
        }

        /// <summary>
        /// 用指定节点反序列化整个数据对象。
        /// </summary>
        /// <param name="node">用于反序列化的 XmlNode 节点。</param>
        public override void XMLDecode(System.Xml.XmlNode node)
        {
            base.XMLDecode(node);

            ReadXMLValue(node, "ContactMob", ref _ContactMob);
            ReadXMLValue(node, "AdminUserId", ref _AdminUserId);
            ReadXMLValue(node, "Deadline", ref _Deadline);
            ReadXMLValue(node, "Products", ref _Products);
        }
#endif

        /// <summary>
        /// 复制数据对象
        /// </summary>
        /// <param name="sou">源对象,需从DataPacket继承</param>
        public override void AssignFrom(DataPacket sou)
        {
            base.AssignFrom(sou);

            SysParams s = sou as SysParams;
            if (s != null)
            {
                _ContactMob = s._ContactMob;
                _AdminUserId = s._AdminUserId;
                _Deadline = s._Deadline;
                _Products = (string[])s._Products.Clone();
            }
        }

        #endregion
    }
}
