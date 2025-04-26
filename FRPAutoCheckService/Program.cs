using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace FRPAutoCheckService
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main()
        {
            
            if(Environment.UserInteractive)
            {
                AutoCheckService autoCheckService = new AutoCheckService();
                autoCheckService.DebugStart();
                while (Console.ReadKey().Key!=ConsoleKey.F1)
                {
                    continue;
                }
                autoCheckService.DebugStop();
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                new AutoCheckService()
                };
                ServiceBase.Run(ServicesToRun);
            }
            
        }
    }
}
