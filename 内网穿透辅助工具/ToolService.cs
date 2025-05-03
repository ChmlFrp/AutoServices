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

        public ToolService()
        {
            OnChangeTunnelData += () =>{};
            CommonData.LoadConfig();
            CommonData.LoadLogText();
        }
        
        /// <summary>
        /// 根据域名获取IP地址
        /// </summary>
        /// <param name="domainName"></param>
        /// <returns></returns>
        private string GetIP(string domainName)
        {
            domainName = domainName.Replace("http://", "").Replace("https://", "");
            IPHostEntry entry = Dns.GetHostEntry(domainName);
            IPEndPoint point = new IPEndPoint(entry.AddressList[0], 0);
            return point.Address.ToString();
        }
        /// <summary>
        /// 检测隧道是否连通
        /// </summary>
        /// <param name="tunnel"></param>
        /// <returns></returns>
        public bool CheckVisit(string type,string ip,string dorp)
        {
            if (type == "http" || type == "https")
            {
                string url = "";
                if (type == "http")
                {
                    url = "http://" + dorp;
                }
                else
                {
                    url = "https://" + dorp;
                }
                if (CheckVisit(url))
                {
                    return true;
                }

            }
            else if (type == "tcp" || type == "udp")
            {
                if (CheckVisit(GetIP(ip!), Convert.ToInt32(dorp!)))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 检测ip:端口是否连通
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        private bool CheckVisit(string ip, int port)
        {
            TcpClient tcp = null;
            try
            {
                var ipa = IPAddress.Parse(ip);
                var point = new IPEndPoint(ipa, port);
                tcp = new TcpClient();
                tcp.Connect(point);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (tcp != null)
                {
                    tcp.Close();
                }
            }
        }
        /// <summary>
        /// 检测网址是否连通
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private bool CheckVisit(string url)
        {
            try
            {
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback += (a, b, c, d) => true;
                handler.SslProtocols = System.Security.Authentication.SslProtocols.None;
                HttpClient req = new HttpClient(handler);
                req.Timeout = TimeSpan.FromSeconds(3);
                var resp = req.GetAsync(url).Result;
                if (resp.StatusCode != HttpStatusCode.NotFound)
                {
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
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
