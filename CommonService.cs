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
    public class CommonService
    {

        public event ChangeTunnelData OnChangeTunnelData;
        public  System.Timers.Timer? timer;

        string serviceFilePath = Path.Combine(Application.StartupPath, "FRPAutoCheckService.exe");
        string serviceName = "内网穿透辅助服务";

        public CommonService()
        {
            OnChangeTunnelData += () =>{};
            CommonData.LoadConfig();
            CommonData.LoadLogText();
            timer = new System.Timers.Timer();
            timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimerTriggle);
            timer.Interval = 1000*60*10;//每10分钟自动检测一次;
            timer.AutoReset = true;//一直循环
            timer.Enabled = true;//开启
        }
       
        /// <summary>
        /// 定时执行任务,检测隧道是否在线,如果不在线则检测服务器节点状态,如果节点离线则自动切换.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimerTriggle(object? sender, System.Timers.ElapsedEventArgs e)
        {
            //更新日志
            CommonData.LoadLogText();
            //更新配置文件
            CommonData.LoadConfig();
            //每分钟更新隧道列表和日志数据
            OnChangeTunnelData.Invoke();
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
        public bool CheckVisit(Tunnel tunnel)
        {
            if (tunnel.type == "http" || tunnel.type == "https")
            {
                string url = "";
                if (tunnel.type == "http")
                {
                    url = "http://" + tunnel.dorp;
                }
                else
                {
                    url = "https://" + tunnel.dorp;
                }
                if (CheckVisit(url))
                {
                    return true;
                }

            }
            else if (tunnel.type == "tcp" || tunnel.type == "udp")
            {
                if (CheckVisit(GetIP(tunnel.ip!), Convert.ToInt16(tunnel.dorp!)))
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
