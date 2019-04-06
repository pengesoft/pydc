using PengeSoft.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pydc.Domain
{
    /// <summary>
    /// Order 点餐订单的摘要说明。
    /// </summary>
    public partial class Order : DataPacket
    {
        #region 私有字段

        private string _UserId;      // 用户Id
        private string _Name;      // 姓名
        private string _DeptId;      // 部门Id
        private int _Count;      // 数量
        private string _Product;      // 产品
        private DateTime _CheckTime;      // 下单时间
        private string _Memo;      // 备注

        #endregion

        #region 属性定义

        /// <summary>
        /// 用户Id 
        /// </summary>
        public string UserId { get { return _UserId; } set { _UserId = value; } }
        /// <summary>
        /// 姓名 
        /// </summary>
        public string Name { get { return _Name; } set { _Name = value; } }
        /// <summary>
        /// 部门Id 
        /// </summary>
        public string DeptId { get { return _DeptId; } set { _DeptId = value; } }
        /// <summary>
        /// 数量 
        /// </summary>
        public int Count { get { return _Count; } set { _Count = value; } }
        /// <summary>
        /// 产品 
        /// </summary>
        public string Product { get { return _Product; } set { _Product = value; } }
        /// <summary>
        /// 下单时间 
        /// </summary>
        public DateTime CheckTime { get { return _CheckTime; } set { _CheckTime = value; } }
        /// <summary>
        /// 备注 
        /// </summary>
        public string Memo { get { return _Memo; } set { _Memo = value; } }

        #endregion

        #region 重载公共函数

        /// <summary>
        /// 清除所有数据。
        /// </summary>
        public override void Clear()
        {
            base.Clear();

            _UserId = null;
            _Name = null;
            _DeptId = null;
            _Count = 0;
            _Product = null;
            _CheckTime = DateTime.MinValue;
            _Memo = null;
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

            WriteXMLValue(node, "UserId", _UserId);
            WriteXMLValue(node, "Name", _Name);
            WriteXMLValue(node, "DeptId", _DeptId);
            WriteXMLValue(node, "Count", _Count);
            WriteXMLValue(node, "Product", _Product);
            WriteXMLValue(node, "CheckTime", _CheckTime);
            WriteXMLValue(node, "Memo", _Memo);
        }

        /// <summary>
        /// 用指定节点反序列化整个数据对象。
        /// </summary>
        /// <param name="node">用于反序列化的 XmlNode 节点。</param>
        public override void XMLDecode(System.Xml.XmlNode node)
        {
            base.XMLDecode(node);

            ReadXMLValue(node, "UserId", ref _UserId);
            ReadXMLValue(node, "Name", ref _Name);
            ReadXMLValue(node, "DeptId", ref _DeptId);
            ReadXMLValue(node, "Count", ref _Count);
            ReadXMLValue(node, "Product", ref _Product);
            ReadXMLValue(node, "CheckTime", ref _CheckTime);
            ReadXMLValue(node, "Memo", ref _Memo);
        }
#endif

        /// <summary>
        /// 复制数据对象
        /// </summary>
        /// <param name="sou">源对象,需从DataPacket继承</param>
        public override void AssignFrom(DataPacket sou)
        {
            base.AssignFrom(sou);

            Order s = sou as Order;
            if (s != null)
            {
                _UserId = s._UserId;
                _Name = s._Name;
                _DeptId = s._DeptId;
                _Count = s._Count;
                _Product = s._Product;
                _CheckTime = s._CheckTime;
                _Memo = s._Memo;
            }
        }

        #endregion
    }

    /// <summary>
    /// OrderList 点餐订单表的摘要说明。
    /// </summary>
    public partial class Orders : NorDataList<Order>
    {
        #region 构造函数

        /// <summary>
        /// 初始化新实例 
        /// </summary>
        public Orders()
            : base()
        {
        }

        /// <summary>
        /// 初始化新实例，该实例包含从指定集合复制的元素并且具有与所复制的元素数相同的初始容量。
        /// </summary>
        /// <param name="c"></param>
        public Orders(ICollection c)
            : base(c)
        {
        }

        #endregion
    }

}
