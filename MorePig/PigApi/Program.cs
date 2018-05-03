using log4net.Config;
using Microsoft.Owin.Hosting;
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
            XmlConfigurator.Configure(new FileInfo("log4net.config"));

            StartOptions options = new StartOptions();
            options.Urls.Add("http://localhost:4004");
            options.Urls.Add("http://127.0.0.1:4004");
            options.Urls.Add(string.Format("http://{0}:4004", Environment.MachineName));
            options.Urls.Add("http://+:4004");
            options.Urls.Add("http://localhost:80");
            options.Urls.Add("http://127.0.0.1:80");
            options.Urls.Add(string.Format("http://{0}:80", Environment.MachineName));
            options.Urls.Add("http://+:80");
            WebApp.Start<Startup>(options);

            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}
