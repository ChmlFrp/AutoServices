using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAutoCheckService
{
    public class MyConfig
    {
        
        //用户id
        public int userId;
        
        
        //阿里云ddns
        public AliyunDdns aliyunDdns;
        //smtp邮箱配置
        public SmtpMail smtpMail;
        //隧道列表
        public Dictionary<int,Tunnel> tunnels;
        //隧道下的自动切换节点优先级配置
        public Dictionary<int, Dictionary<string,Node>> tunnelNodes;
        //节点列表
        public Dictionary<int,Node> nodes;
        //数据库配置
        public Dictionary<int,MysqlConnectInfo> tunnelMysqlInfos;
        public MyConfig() 
        {
            tunnels = new ();
            tunnelNodes = new ();
            nodes = new ();
            aliyunDdns = new ();
            smtpMail = new();
            tunnelMysqlInfos = new();
        }
    }
    public class MysqlConnectInfo
    {
        public string ip;
        public int port;
        public string account;
        public string password;
        public string database;
        public string table;
        public string field;
        public string where;
        public string tunnelIp;
        public bool isAuto;

    }
    public class SmtpMail
    {
        //smtp邮箱服务器地址
        public string mailSmtpServer;
        //邮箱账号
        public string mailAccount;
        //邮箱密码或授权码
        public string mailPassword;
        //接收信息的邮箱
        public string acceptMail;
        //是否启用smtp邮箱
        public bool isAuto;
    }
    public class AliyunDdns
    {
        //阿里云ddns keyid
        public string AccessKeyId;
        //阿里云ddns keysecret
        public string AccessKeySecret;
        //是否启用阿里云ddns
        public bool isAuto;
    }
    public class Tunnel
    {
        public int id { get; set; }
        public string name { get; set; }
        public string localip;
        public int nport;
        public string type;
        public string dorp;
        public string node;
        public string nodestate;
        public string ip;
        public int pid;
        public bool isAutoConnect;
        
    }
    public class Node:IComparable<Node>
    {
        public int id { get; set; }
        public string name { get; set; }
        public int level;

        public int CompareTo(Node other)
        {
            return other.level-level;
        }
    }
}
