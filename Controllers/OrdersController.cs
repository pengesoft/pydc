using Microsoft.AspNetCore.Mvc;
using Pydc.Domain;
using Pydc.Service;
using System.Collections.Generic;

namespace Pydc.Controllers
{
    [Route("api/[controller]")]
    public class OrdersController : Controller
    {
        IOrderManager _orders = null;

        public OrdersController(IOrderManager orderManager)
        {
            _orders = orderManager;
        }

        // GET api/values
        [HttpGet("GetOrders")]
        public Orders GetOrders(string userId, string date, int option)
        {
            return _orders.GetOrders(userId, date, option);
        }

        // GET api/values/5
        [HttpGet("Get")]
        public Order Get(string userId, string date)
        {
            return _orders.Get(userId, date);
        }

        [HttpGet("GetLast")]
        public Order GetLast(string userId)
        {
            return _orders.GetLast(userId);
        }

        // POST api/values
        [HttpGet("PlaceOrder")]
        public int PlaceOrder(string userId, string product, int count, string memo)
        {
            return _orders.PlaceOrder(userId, product, count, memo);
        }

        // DELETE api/values/5
        [HttpGet("Delete")]
        public void Delete(string userId)
        {
            _orders.Delete(userId);
        }

        // GET api/values/5
        [HttpGet("GetSysParams")]
        public SysParams GetSysParams(string userId)
        {
            return _orders.GetSysParams(userId);
        }

        // POST api/values
        [HttpGet("UpdateSysParams")]
        public int UpdateSysParams(string userId, SysParams paras, int option)
        {
            return _orders.UpdateSysParams(userId, paras, option);
        }
    }
}
