using MailKit.Security;
using MimeKit.Text;
using MimeKit;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using System.Diagnostics;
using AlibabaCloud.SDK.Alidns20150109.Models;
using Tea;
using System.Net;
using System.Net.Sockets;
using System.Net.Http;
using System.IO;
using MySql.Data.MySqlClient;
using System.ComponentModel;
using System.Management;
using System.Threading;

namespace FRPAutoCheckService
{
    public class CommonService
    {
        public  System.Timers.Timer timer;
        private int tunnelStartingNum=0;

        private User user;

        public CommonService()
        {
            user=new User();
            CommonData.LoadConfig();
            CloseAllFrp();//服务启动时先杀死所有frp进程
            timer = new System.Timers.Timer();
            timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimerTriggle);
            timer.Interval = 1000;//1秒后启动,再在方法中设置为1分钟
            timer.AutoReset = true;//一直循环
            timer.Enabled = false;//先不开启
        }

        /// <summary>
        /// 定时执行任务,检测隧道是否在线,如果不在线则检测服务器节点状态,如果节点离线则自动切换.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimerTriggle(object sender, System.Timers.ElapsedEventArgs e)
        {
            //CommonData.PrintLog("定时器触发,开始检测隧道状态!");
            if (timer.Interval <= 1000)
            {
                timer.Interval = 1000 * 60;//每分钟检测一次
                //每次启动服务强制重新登录,刷新隧道列表
                if (!string.IsNullOrEmpty(user.Username) && !string.IsNullOrEmpty(user.Password))
                {
                    Login(user.Username, user.Password);
                }
            }
            CommonData.LoadConfig();//加载配置文件
            if (tunnelStartingNum!=0)//frp隧道过多会拖慢启动速度,如果frp隧道还没有完全启动完毕则等待启动完成
            {
                CommonData.PrintLog("frp隧道正在启动中,跳过检测!启动中的隧道数量:"+tunnelStartingNum);
                tunnelStartingNum--;//每隔一分钟就减一,防止死循环
                return;
            }
            if (string.IsNullOrEmpty(user.Usertoken))
            {
                //登录
                if (!string.IsNullOrEmpty(user.Username) && !string.IsNullOrEmpty(user.Password))
                {
                    Login(user.Username, user.Password);
                }
            }
            else
            {
                //自动连接隧道
                foreach (var tunnel in CommonData.config.tunnels.Values)
                {
                    if (!tunnel.isAutoConnect)//如果不自动连接
                    {
                        //CommonData.PrintLog("隧道 " + tunnel.name + " 不自动连接!");
                        continue;
                    }
                    if (CheckVisit(tunnel))//如果可以正常访问
                    {
                        //CommonData.PrintLog("隧道 " + tunnel.name + " 正常访问!");
                        continue;
                    }
                    CommonData.PrintLog($"隧道 {tunnel.name} 离线,开始重新连接!");
                    //string state = GetNodeStatus(tunnel.node);
                    string state = CheckVisit(GetIP(tunnel.ip!), 7000, "tcp") ? "online" : "offline";//检测隧道节点是否在线
                    if (state != null)
                    {
                        if(state == "offline")//2次检测确保隧道是真的离线了
                        {
                            //如果节点离线则,线程阻塞3秒钟,再检测一次
                            Thread.Sleep(3000);
                            state = GetNodeStatus(tunnel.node);//检测隧道节点是否在线
                        }
                        if (state == "online")//如果服务器节点在线
                        {
                            //如果隧道节点在线,则直接启动
                            CommonData.PrintLog("隧道 " + tunnel.name +":"+ tunnel.node+" 节点在线,开始启动!");
                            GetTunnelConfig(tunnel.name, tunnel.node);
                            ExecuteFrp(tunnel.id);
                        }
                        else if (state == "offline")
                        {
                            //如果隧道节点离线,则自动切换节点
                            CommonData.PrintLog("隧道 " + tunnel.name+":"+tunnel.node + " 节点离线,开始切换节点!");
                            if (CommonData.config.tunnelNodes.Count==0)
                            {
                                continue;
                            }
                            //按照优先级修改隧道节点
                            List<Node> nodes = CommonData.config.tunnelNodes?[tunnel.id]?.Values?.ToList();
                            if (nodes != null)
                            {
                                nodes.Sort();
                                foreach (var node in nodes)
                                {
                                    string state1 = GetNodeStatus(node.name);
                                    if (state1 == "offline")
                                    {
                                        continue;
                                    }
                                    //如果节点在线,则修改隧道节点
                                    CommonData.PrintLog("隧道 " + tunnel.name + ":" + tunnel.node + " 节点切换到 " + node.name+",开始启动!");
                                    UpdateTunnel(tunnel, node.name!);
                                    GetTunnelConfig(tunnel.name, node.name!);
                                    ExecuteFrp(tunnel.id);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            CommonData.PrintLog("隧道 " + tunnel.name + ":" + tunnel.node + " 节点状态未知,跳过检测!");
                            continue;
                        }
                        if (tunnel.type == "http" || tunnel.type == "https")
                        {
                            if (CommonData.config.aliyunDdns.isAuto)
                            {
                                //如果是http或https类型的隧道,则更新阿里云ddns解析
                                //CommonData.PrintLog("隧道 " + tunnel.name + ":" + tunnel.node + " 节点更新阿里云ddns解析!");
                                UpdateAliyunDdns(tunnel.dorp!, tunnel.ip!);
                            }
                        }

                    }

                }
                //自动更新数据库
                foreach (var mysqlInfo in CommonData.config.tunnelMysqlInfos.Values)
                {
                    if (mysqlInfo.isAuto)
                    {
                        //CommonData.PrintLog("隧道 " + mysqlInfo.tunnelIp + " 节点更新数据库!");
                        UpdateMysqlByIp(mysqlInfo);
                    }
                    
                }
                
            }
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
                if (CheckVisit(GetIP(tunnel.ip!), Convert.ToInt32(tunnel.dorp!),tunnel.type))
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
        private bool CheckVisit(string ip, int port,string type)
        {
            try
            {
                var ipa = IPAddress.Parse(ip);
                var point = new IPEndPoint(ipa, port);
                var timeout = 1000;
                var time = PingAsync(ipa, port,type,timeout).Result;
                
                if (time.TotalMilliseconds>=timeout || time == TimeSpan.Zero)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                //CommonData.PrintLog($"测试{ip}:{port} 连接失败!,异常信息:{ex.Message}:{ex.StackTrace}");
                return false;
            }
        }
        /// <summary>
        /// 检测指定地址和端口的连通性
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="protocolType"></param>
        /// <param name="timeout"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="TimeoutException"></exception>
        public static async Task<TimeSpan> PingAsync(IPAddress address, int port, string protocolType = "tcp", int timeout = 2000,
            CancellationToken token = default)
        {
            using (var socket = new Socket(SocketType.Stream, protocolType=="tcp"?ProtocolType.Tcp:ProtocolType.Udp) { Blocking = true })
            {
                // initialize timing
                var sw = new Stopwatch();
                var timedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
                timedTokenSource.CancelAfter(timeout);
                sw.Start();

                // try connecting to the server, with time limit
                try
                {
                    var connectTask = socket.ConnectAsync(address, port);
                    await connectTask.WaitAsync(timedTokenSource.Token);
                    // no TaskCanceledException thrown, not cancelled, so job done
                    return sw.Elapsed;
                }
                catch (TaskCanceledException)
                {
                    // not cancelled by user -> timed out
                    if (!token.IsCancellationRequested)
                        throw new TimeoutException("Timed out waiting for response");

                    // do nothing: cancelled by user
                    return TimeSpan.Zero;
                }
                catch (AggregateException ex)
                {
                    throw ex.Unwrap();
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
                var status = resp.StatusCode;
                if (status == HttpStatusCode.OK || status==HttpStatusCode.Unauthorized || status == HttpStatusCode.Found)
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
        /// 修改隧道节点
        /// </summary>
        /// <param name="oldT"></param>
        /// <param name="newT"></param>
        private void UpdateTunnel(Tunnel oldT,string nodeName)
        {
            //修改隧道节点
            
            var options = new RestClientOptions("https://cf-v1.uapis.cn/api")
            {
                ConfigureMessageHandler = (handler) =>
                new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = (a, b, c, d) => true,
                    SslProtocols = System.Security.Authentication.SslProtocols.None
                }
            };
            var client = new RestClient(options);
            var request = new RestRequest("cztunnel.php");
            

            JObject json = new JObject();
            json["usertoken"] = user.Usertoken;
            json["tunnelid"] = oldT.id;
            json["name"] = oldT.name;
            json["node"] = nodeName;
            json["localip"] = oldT.localip;
            json["type"] = oldT.type;
            json["nport"] = oldT.nport;
            json["dorp"] = oldT.dorp;
            json["ap"] = "";
            json["encryption"] = true;
            json["compression"] = true;
            request.AddParameter("application/json", json.ToString(Formatting.None), ParameterType.RequestBody);

            request.Method = Method.Post;
            var response = client.Post(request);
            JObject respJson = JObject.Parse(response.Content ?? "{}");
            string state = respJson["state"]?.ToString();
            if (CommonData.config.smtpMail.isAuto)
            {
                string content = $"隧道 {oldT.name} 的原节点 {oldT.node} 已切换到新节点 {nodeName} ";
                SendEmail(CommonData.config.smtpMail.acceptMail!, "隧道节点修改通知", content);
            }
            //更新隧道信息
            LoadTunnelList();
        }

        /// <summary>
        /// 获取隧道配置信息,写入文件中
        /// </summary>
        /// <param name="tunnelName"></param>
        /// <param name="nodeName"></param>
        private void GetTunnelConfig(string tunnelName,string nodeName)
        {
            var options = new RestClientOptions("http://cf-v2.uapis.cn");
            var client = new RestClient(options);
            var request = new RestRequest("tunnel_config");
            request.AddParameter("token", user.Usertoken);
            request.AddParameter("node", nodeName);
            request.AddParameter("tunnel_names", tunnelName);
            request.Method = Method.Get;
            var response = client.Get(request);
            JObject respJson = JObject.Parse(response.Content ?? "{}");
            string state = respJson["state"]?.ToString();
            if (state == "success")
            {
                string tunnelConfig = respJson["data"].ToString();
                string fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ChmlFrp", tunnelName + ".ini");
                using (StreamWriter sw = new StreamWriter(fileName,false,Encoding.UTF8))
                {
                    sw.Write(tunnelConfig);
                    sw.Flush();
                }
            }
            
        }
        
        /// <summary>
        /// 查找启动的frpc.exe程序,然后杀掉这些程序
        /// </summary>
        private void KillAllProcess()
        {
            string fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ChmlFrp", "frpc.exe");
            Process[] ps = Process.GetProcesses();
            foreach (var p in ps)
            {
                if (p.ProcessName != "frpc")
                    continue;
                if (p.MainModule.FileName != fileName)
                    continue;
                p.Kill();
            }
        }

        /// <summary>
        /// 关闭所有frp进程
        /// </summary>
        public void CloseAllFrp()
        {
            KillAllProcess();
            CommonData.SaveConfig();
        }
        /// <summary>
        /// 通过进程ID,杀死frp进程
        /// </summary>
        public void CloseFrp(int processId)
        {
            Process[] myproc = Process.GetProcesses();
            foreach (Process proc in myproc)
            {
                if (proc.Id == processId)
                {
                    proc.Kill();
                }
            }
        }
        /// <summary>
        /// 通过启动参数关闭frp进程
        /// </summary>
        /// <param name="processArgs"></param>
        public void CloseFrp(string processArgs)
        {
            Process[] myproc = Process.GetProcesses();
            foreach (Process proc in myproc)
            {
                if (proc.GetCommandLineArgs() == processArgs)
                {
                    proc.Kill();
                }
            }
        }

        /// <summary>
        /// 启动隧道
        /// </summary>
        private void ExecuteFrp(int tunnelId)
        {
            this.tunnelStartingNum++;//启动隧道前
            Task.Run(() =>
            {
                Tunnel tunnel = CommonData.config.tunnels[tunnelId];
                if (tunnel.pid != 0)
                {
                    CloseFrp(tunnel.pid);
                }
                string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ChmlFrp");
                string exeName = "frpc.exe";
                string fileName = Path.Combine(exePath, exeName);
                Process myProcess = new Process();
                myProcess.StartInfo.FileName = fileName;
                myProcess.StartInfo.UseShellExecute = false;
                myProcess.StartInfo.RedirectStandardOutput = true;
                myProcess.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                myProcess.StartInfo.CreateNoWindow = true;
                myProcess.StartInfo.WorkingDirectory = exePath;
                myProcess.StartInfo.Arguments = "-c " + tunnel.name + ".ini";
                string frpArgs = $"\"{myProcess.StartInfo.FileName}\" {myProcess.StartInfo.Arguments}";
                myProcess.Start();
                tunnel.pid = myProcess.Id;
                CommonData.SaveConfig();//保存配置文件
                //this.Invoke(new Action(() =>
                //{
                //    this.textBox1.Text += "进程ID" + myProcess.Id + "\r\n"; ;
                //}));
                while (!myProcess.StandardOutput.EndOfStream)
                {
                    string line = myProcess.StandardOutput.ReadLine();
                    line = line.Replace(user.Usertoken, "usertoken");
                    CommonData.PrintLog(line);

                    //启动成功或者启动失败都计数减一
                    if (line.Contains("映射启动成功"))
                    {
                        if(this.tunnelStartingNum>0)this.tunnelStartingNum--;
                    }
                    else if (line.Contains("already exists") || line.Contains("[W]"))
                    {
                        if (this.tunnelStartingNum > 0) this.tunnelStartingNum--;
                        CloseFrp(frpArgs);
                        ExecuteFrp(tunnelId);
                        break;
                    }
                }
                
            });
        }
        /// <summary>
        /// 获取节点状态,是否在线
        /// </summary>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        private string GetNodeStatus(string nodeName)
        {
            var options = new RestClientOptions("http://cf-v2.uapis.cn");
            var client = new RestClient(options);
            var request = new RestRequest("nodeinfo");
            request.AddParameter("token", user.Usertoken);
            request.AddParameter("node", nodeName);
            request.Method = Method.Get;
            var response = client.Get(request);
            JObject respJson = JObject.Parse(response.Content ?? "{}");
            string state = respJson["state"]?.ToString();
            if (state == "success")
            {
                JObject nodeData = respJson["data"] as JObject;
                string nodeState = nodeData["state"]?.ToString();
                if (nodeState == "offline")
                {
                    CommonData.PrintLog($"节点 {nodeName } 离线!,接口返回信息:{nodeData.ToString()}");
                }
                return nodeState;
            }
            return null;
        }
        

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public (bool,string) Login(string username, string password)
        {
            var options = new RestClientOptions("http://cf-v2.uapis.cn");
            var client = new RestClient(options);
            var request = new RestRequest("login");
            request.AddParameter("username", username);
            request.AddParameter("password", password);
            request.Method = Method.Get;
            var response = client.Get(request);
            JObject respJson = JObject.Parse(response.Content ?? "{}");
            if (!respJson.ContainsKey("state"))
            {
                return (false,"登录失败,请求v2接口失败!");
            }
            string state = respJson["state"]?.ToString();
            if (state !=null && state == "success")
            {
                string token = respJson["data"]["usertoken"].ToString();
                int userId = Convert.ToInt32(respJson["data"]["id"]);
                CommonData.config.userId= userId;
                user.Usertoken = token;
                LoadTunnelList();
                LoadNodeList();
            }
            CommonData.SaveConfig();
            CommonData.PrintLog("登录成功");
            return (true, "登录成功,用户ID:" + CommonData.config.userId + ",用户Token:" + user.Usertoken);
        }
        /// <summary>
        /// 加载服务器节点
        /// </summary>
        public void LoadNodeList()
        {
            var options = new RestClientOptions("http://cf-v2.uapis.cn");
            var client = new RestClient(options);
            var request = new RestRequest("node");
            request.Method = Method.Get;
            var response = client.Get(request);
            JObject respJson = JObject.Parse(response.Content ?? "{}");
            string state = respJson["state"]?.ToString();
            if (state == "success")
            {
                JArray jArray = respJson["data"] as JArray;
                var tempNodes = CommonData.config.nodes.ToDictionary(k => k.Key, v => v.Value);
                CommonData.config.nodes.Clear();
                foreach (var item in jArray)
                {
                    Node node = new Node();
                    node.id = Convert.ToInt32(item["id"].ToString());
                    node.name = item["name"].ToString();
                    string nodeGroup = item["nodegroup"].ToString();
                    if (nodeGroup== "vip")
                    {
                        node.name += $"({nodeGroup})";
                    }

                    if (tempNodes.ContainsKey(node.id))
                    {
                        Node temp = tempNodes[node.id];
                        node.level = temp.level;
                        CommonData.config.nodes[node.id] = node;
                    }
                    else
                    {
                        CommonData.config.nodes.Add(node.id, node);
                    }

                }
                //保存配置文件
                CommonData.SaveConfig();
                CommonData.PrintLog("服务器节点加载成功!");
            }
        }
        /// <summary>
        /// 加载隧道列表
        /// </summary>
        public void LoadTunnelList()
        {
            var options = new RestClientOptions("http://cf-v2.uapis.cn");
            var client = new RestClient(options);
            var request = new RestRequest("tunnel");
            request.AddParameter("token", user.Usertoken);
            request.Method = Method.Get;
            var response = client.Get(request);
            JObject respJson = JObject.Parse(response.Content ?? "{}");
            string state = respJson["state"]?.ToString();
            if (state == "success")
            {
                JArray jArray = respJson["data"] as JArray;
                //保存临时隧道列表
                var tempTunnels = CommonData.config.tunnels.ToDictionary(k=>k.Key,v=>v.Value);
                //清空隧道列表
                CommonData.config.tunnels.Clear();
                foreach (var item in jArray)
                {
                    Tunnel tunnel = new Tunnel();
                    tunnel.id = Convert.ToInt32(item["id"].ToString());
                    tunnel.name = item["name"].ToString();
                    tunnel.localip = item["localip"].ToString();
                    tunnel.type = item["type"].ToString();
                    tunnel.nport = Convert.ToInt32(item["nport"].ToString());
                    tunnel.dorp = item["dorp"].ToString();
                    tunnel.node = item["node"].ToString();
                    tunnel.nodestate = item["nodestate"].ToString();
                    tunnel.ip = item["ip"].ToString();

                    if (tempTunnels.ContainsKey(tunnel.id))
                    {
                        Tunnel temp = tempTunnels[tunnel.id];
                        //添加本地自定义属性
                        tunnel.pid = temp.pid;
                        tunnel.isAutoConnect = temp.isAutoConnect;
                    }
                    CommonData.config.tunnels.Add(tunnel.id, tunnel);
                    
                }
                //保存配置文件
                CommonData.SaveConfig();
                CommonData.PrintLog("隧道列表加载成功!");
            }
        }

        /// <summary>
        /// 创建阿里云链接客户端
        /// </summary>
        /// <returns></returns>
        public  AlibabaCloud.SDK.Alidns20150109.Client CreateClient()
        {
            // 工程代码泄露可能会导致 AccessKey 泄露，并威胁账号下所有资源的安全性。以下代码示例仅供参考。
            // 建议使用更安全的 STS 方式，更多鉴权访问方式请参见：https://help.aliyun.com/document_detail/378671.html。
            AlibabaCloud.OpenApiClient.Models.Config alibabaConfig = new AlibabaCloud.OpenApiClient.Models.Config
            {
                // 必填，请确保代码运行环境设置了环境变量 ALIBABA_CLOUD_ACCESS_KEY_ID。
                
                AccessKeyId =CommonData.config.aliyunDdns.AccessKeyId,
                // 必填，请确保代码运行环境设置了环境变量 ALIBABA_CLOUD_ACCESS_KEY_SECRET。
                
                AccessKeySecret = CommonData.config.aliyunDdns.AccessKeySecret,
            };
            // Endpoint 请参考 https://api.aliyun.com/product/Alidns
            //config.Endpoint = "alidns.cn-hangzhou.aliyuncs.com";
            alibabaConfig.RegionId = "cn-chengdu-a";
            return new AlibabaCloud.SDK.Alidns20150109.Client(alibabaConfig);
        }
        /// <summary>
        /// 更新阿里云解析记录
        /// </summary>
        /// <param name="tunnelDomain"></param>
        /// <param name="cname"></param>
        private void UpdateAliyunDdns(string tunnelDomain,string newCname)
        {
            var client = CreateClient();
            var req = new DescribeDomainsRequest();
            var resp = client.DescribeDomains(req);
            JObject respJson = JObject.Parse(AlibabaCloud.TeaUtil.Common.ToJSONString(resp.ToMap()));

            if (Convert.ToInt32(respJson["statusCode"]) == 200)
            {
                JArray domains = respJson["body"]["Domains"]["Domain"] as JArray;
                foreach (var item in domains)
                {
                    string domainName = item["DomainName"].ToString();
                    var req1 = new DescribeDomainRecordsRequest();
                    req1.DomainName = domainName;
                    var resp1 = client.DescribeDomainRecords(req1);
                    JObject respJson1 = JObject.Parse(AlibabaCloud.TeaUtil.Common.ToJSONString(resp1.ToMap()));
                    JArray records = respJson1["body"]["DomainRecords"]["Record"] as JArray;
                    foreach(var record in records)
                    {
                        string rr = record["RR"].ToString();
                        string str = rr + "." + domainName;
                        string status = record["Status"].ToString();

                        if (status == "ENABLE" && str == tunnelDomain)
                        {
                            string oldCname = record["Value"].ToString();
                            if (!oldCname.Equals(newCname))//每次建立链接都会检测新老解析是否相同,如果不同则更新ddns
                            {
                                var req2 = new UpdateDomainRecordRequest();
                                req2.RecordId = record["RecordId"].ToString();
                                req2.RR = rr;
                                req2.Type = record["Type"].ToString();
                                req2.Value = newCname;
                                var resp2 = client.UpdateDomainRecord(req2);
                                JObject respJson2 = JObject.Parse(AlibabaCloud.TeaUtil.Common.ToJSONString(resp2.ToMap()));
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 发送邮件方法
        /// </summary>
        /// <param name="mailTo">接收人邮件</param>
        /// <param name="mailTitle">发送邮件标题</param>
        /// <param name="mailContent">发送邮件内容</param>
        /// <returns></returns>
        public bool SendEmail(string mailTo, string mailTitle, string mailContent)
        {
            //设置发送方邮件信息，例如：qq邮箱
            
            string smtpServer = CommonData.config.smtpMail.mailSmtpServer;
            string mailAccount = CommonData.config.smtpMail.mailAccount;
            string pwd = CommonData.config.smtpMail.mailPassword;

            var messageToSend = new MimeMessage();
            //messageToSend.Sender = new MailboxAddress("唐鹏程", mailAccount);//发送人
            messageToSend.From.Add(new MailboxAddress("内网穿透辅助工具", mailAccount));//发送人
            messageToSend.Subject = mailTitle;//标题
            messageToSend.Body = new TextPart(MimeKit.Text.TextFormat.Plain) { Text = mailContent };//内容
            messageToSend.To.Add(new MailboxAddress("", mailTo));//接收人
            using (var smtp = new SmtpClient())
            {
                smtp.MessageSent += (sender, args) =>
                {
                    //打印服务器响应内容
                    //Console.WriteLine(args.Response);
                };
                smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;//服务器证书响应一律返回true
                smtp.Connect(smtpServer, 587, SecureSocketOptions.StartTls);//使用tls建立链接
                smtp.Authenticate(mailAccount, pwd);//登录
                smtp.Send(messageToSend);//发送
                smtp.Disconnect(true);//关闭
            }
            return true;
        }
        /// <summary>
        /// 更新数据库
        /// </summary>
        /// <param name="info"></param>
        /// <param name="tunnel"></param>
        private void UpdateMysqlByIp(MysqlConnectInfo info)
        {
            using (MySqlConnection conn = new MySqlConnection(GetMysqlConnectionStr(info)))
            {
                conn.Open();
                string sql = $"select {info.field} from {info.table} where {info.where ?? "1=1"}";
                MySqlCommand command = new MySqlCommand(sql, conn);
                string ip = command.ExecuteScalar()?.ToString();
                string tunnelIp = GetIP(info.tunnelIp);
                if (ip != tunnelIp)
                {
                    string sql2 = $"update {info.table} set {info.field}=\"{tunnelIp}\" where {info.where ?? "1=1"}";
                    var command2 = new MySqlCommand(sql2, conn);
                    command2.ExecuteNonQuery();
                }
            } 
        }

        private string GetMysqlConnectionStr(MysqlConnectInfo info)
        {
            MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder();
            builder.Database = info.database;
            builder.UserID = info.account;
            builder.Password = info.password;
            builder.Port =Convert.ToUInt32(info.port);
            builder.Server=info.ip;
            builder.CharacterSet = "utf8";
            builder.Pooling = true;
            return builder.ConnectionString;
        }
        
    }
    /// <summary>
    /// 为 <see cref="Process"/> 类型提供扩展方法。
    /// </summary>
    public static class ProcessExtensions
    {
        /// <summary>
        /// 获取一个正在运行的进程的命令行参数。
        /// 与 <see cref="Environment.GetCommandLineArgs"/> 一样，使用此方法获取的参数是包含应用程序路径的。
        /// 关于 <see cref="Environment.GetCommandLineArgs"/> 可参见：
        /// .NET 命令行参数包含应用程序路径吗？https://blog.walterlv.com/post/when-will-the-command-line-args-contain-the-executable-path.html
        /// </summary>
        /// <param name="process">一个正在运行的进程。</param>
        /// <returns>表示应用程序运行命令行参数的字符串。</returns>
        public static string GetCommandLineArgs(this Process process)
        {
            if (process is null) throw new ArgumentNullException(nameof(process));

            try
            {
                return GetCommandLineArgsCore();
            }
            catch (Win32Exception ex) when ((uint)ex.ErrorCode == 0x80004005)
            {
                // 没有对该进程的安全访问权限。
                return string.Empty;
            }
            catch (InvalidOperationException)
            {
                // 进程已退出。
                return string.Empty;
            }

            string GetCommandLineArgsCore()
            {
                using (var searcher = new ManagementObjectSearcher(
                    "SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id))
                using (var objects = searcher.Get())
                {
                    var @object = objects.Cast<ManagementBaseObject>().SingleOrDefault();
                    return @object?["CommandLine"]?.ToString() ?? "";
                }
            }
        }
    }
}
