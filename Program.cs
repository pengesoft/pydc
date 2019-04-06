using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PengeSoft.Client;
using PengeSoft.DingTalk;
using PengeSoft.Service;
using Pydc.Domain;
using Pydc.Service;

namespace Pydc
{
    public class Program
    {
        private static bool isStop = false;
        private static IOrderManager _orderMan;
        private static bool _notified = false;

        public static void Main(string[] args)
        {
            _orderMan = ServiceManager.GetService<IOrderManager>("OrderManager", true);
            Thread thread = new Thread(new ThreadStart(CheckDateThread));
            thread.Start();

            var config = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("hosting.json", optional: true)
                   .Build();

            BuildWebHost(args, config).Run();

            isStop = true;
        }

        private static void CheckDateThread()
        {
            while (!isStop)
            {
                Thread.Sleep(5000);
                try
                {
                    DoCheckOrder();
                }catch
                {

                }
            }
        }

        private static void DoCheckOrder()
        {
            DateTime dt = DateTime.Now;
            int dt0 = dt.Hour * 60 + dt.Minute + 1;
            if ((dt0 > 11 * 60) && (dt0 < 19 * 60))
            {
                if (!_notified)
                {
                    if (_orderMan.SendNotify("", "", 3) == 0)
                        _notified = true;
                }
            }
            else
                _notified = false;
        }

        public static IWebHost BuildWebHost(string[] args, IConfiguration config) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel()
                //.UseUrls("http://*:8088")
                .UseConfiguration(config)
                .UseStartup<Startup>()
                .Build();
    }
}
