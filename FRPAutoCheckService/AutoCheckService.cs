using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace FRPAutoCheckService
{
    public partial class AutoCheckService : ServiceBase
    {
        /// <summary>
        /// 逻辑执行类
        /// </summary>
        CommonService commService;
        public AutoCheckService()
        {
            InitializeComponent();
            commService = new CommonService();
        }

        protected override void OnStart(string[] args)
        {
            CommonData.ClearLog();
            CommonData.PrintLog("frp自动检测服务已启动!,检测间隔为1分钟,请耐心等待.");
            commService.timer.Start();
        }
        public void DebugStart()
        {
            CommonData.ClearLog();
            CommonData.PrintLog("frp自动检测服务已启动!,检测间隔为1分钟,请耐心等待.");
            commService.timer.Start();
        }
        protected override void OnStop()
        {
            CommonData.PrintLog("frp自动检测服务已停止!");
            commService.timer.Stop();
            commService.CloseAllFrp();
        }
        public void DebugStop()
        {
            CommonData.PrintLog("frp自动检测服务已停止!");
            commService.timer.Stop();
            commService.CloseAllFrp();
        }
    }
}
