using log4net.Config;
using Microsoft.Owin.Hosting;
using PigRunService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PigApi
{
    class Program
    {
        static void Main(string[] args)
        {
            // 注册日志
            XmlConfigurator.Configure(new FileInfo("log4net.config"));

            // 启用监听
            StartOptions options = new StartOptions();
            options.Urls.Add("http://localhost:6666");
            options.Urls.Add("http://127.0.0.1:6666");
            options.Urls.Add(string.Format("http://{0}:6666", Environment.MachineName));
            options.Urls.Add("http://+:6666");
            options.Urls.Add("http://localhost:90");
            options.Urls.Add("http://127.0.0.1:90");
            options.Urls.Add(string.Format("http://{0}:90", Environment.MachineName));
            options.Urls.Add("http://+:90");
            WebApp.Start<Startup>(options);

            // 初始化k线
            KlineUtils.Begin();

            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}
