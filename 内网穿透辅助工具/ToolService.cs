using System.Configuration.Install;
using System.ServiceProcess;
using System.Collections;
using FRPAutoCheckService;
using System.Net.Sockets;
using System.Net;
using System.Net.Http;


namespace 内网穿透辅助工具
{
    public delegate void ChangeTunnelData();
    public class ToolService
    {
        public event ChangeTunnelData OnChangeTunnelData;
        string serviceFilePath = Path.Combine(Application.StartupPath, "FRPAutoCheckService.exe");
        string serviceName = "内网穿透辅助服务";
        private CommonService commonService = new CommonService();
        public ToolService()
        {
            OnChangeTunnelData += () =>{};
            CommonData.LoadConfig();
            CommonData.LoadLogText();
        }
        /// <summary>
        /// 检测隧道是否连通
        /// </summary>
        /// <param name="tunnel"></param>
        /// <returns></returns>
        public bool CheckVisit(string type,string ip,string dorp)
        {
            return commonService.CheckVisit(new Tunnel { type=type,ip=ip,dorp=dorp});
        }
       
        /// <summary>
        /// 安装服务
        /// </summary>
        public void InstallService()
        {
            if(IsServiceExisted(serviceName))
            {
                UninstallService(serviceFilePath);
            }
            InstallService(serviceFilePath);
        }
        /// <summary>
        /// 启动服务
        /// </summary>
        public void StartService()
        {
            if (IsServiceExisted(serviceName))
            {
                ServiceStart(serviceName);
            }
        }
        /// <summary>
        /// 停止服务
        /// </summary>
        public void StopService()
        {
            if (IsServiceExisted(serviceName))
            {
                ServiceStop(serviceName);
            }
        }
        /// <summary>
        /// 卸载服务
        /// </summary>
        public void UninstallService()
        {
            if (IsServiceExisted(serviceName))
            {
                ServiceStop(serviceName);
                UninstallService(serviceFilePath);
            }
        }
        //安装服务
        private void InstallService(string serviceFilePath)
        {
            using (AssemblyInstaller installer = new AssemblyInstaller())
            {
                installer.UseNewContext = true;
                installer.Path = serviceFilePath;
                IDictionary savedState = new Hashtable();
                installer.Install(savedState);
                installer.Commit(savedState);
            }
        }
        //卸载服务
        private void UninstallService(string serviceFilePath)
        {
            using (AssemblyInstaller installer = new AssemblyInstaller())
            {
                installer.UseNewContext = true;
                installer.Path = serviceFilePath;
                installer.Uninstall(null);
            }
        }
        //启动服务
        private void ServiceStart(string serviceName)
        {
            using (ServiceController control = new ServiceController(serviceName))
            {
                if (control.Status == ServiceControllerStatus.Stopped)
                {
                    control.Start();
                }
            }
        }

        //停止服务
        private void ServiceStop(string serviceName)
        {
            using (ServiceController control = new ServiceController(serviceName))
            {
                if (control.Status == ServiceControllerStatus.Running)
                {
                    control.Stop();
                }
            }
        }
        //判断服务是否存在
        private bool IsServiceExisted(string serviceName)
        {
            ServiceController[] services = ServiceController.GetServices();
            foreach (ServiceController sc in services)
            {
                if (sc.ServiceName.ToLower() == serviceName.ToLower())
                {
                    return true;
                }
            }
            return false;
        }
    }
}
