
/**
 *
 * 点餐订单的摘要说明。
 *
 * @class Order *
 */
function Order(){
    /**
     * 用户Id  
     * @property {UserId} 用户Id 
     */
    this.UserId     = null;
    /**
     * 姓名  
     * @property {Name} 姓名 
     */
    this.Name       = null;
    /**
     * 部门Id  
     * @property {DeptId} 部门Id 
     */
    this.DeptId     = null;
    /**
     * 数量  
     * @property {Count} 数量 
     */
    this.Count      = 0;
    /**
     * 产品  
     * @property {Product} 产品 
     */
    this.Product    = null;
    /**
     * 下单时间  
     * @property {CheckTime} 下单时间 
     */
    this.CheckTime  = new Date();
    /**
     * 备注  
     * @property {Memo} 备注 
     */
    this.Memo       = null;
}


/**
 *
 * 点餐系统参数的摘要说明。
 *
 * @class SysParams *
 */
function SysParams(){
    /**
     * 服务商联系人手机  
     * @property {ContactMob} 服务商联系人手机 
     */
    this.ContactMob   = null;
    /**
     * 管理员钉钉UserId  
     * @property {AdminUserId} 管理员钉钉UserId 
     */
    this.AdminUserId  = null;
    /**
     * 订餐截止时间 从0点计算的当天分钟数 
     * @property {Deadline} 订餐截止时间 
     */
    this.Deadline     = 0;
    /**
     * 可选产品  
     * @property {Products} 可选产品 
     */
    this.Products     = new Array();
}
