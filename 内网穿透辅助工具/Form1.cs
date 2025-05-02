using FRPAutoCheckService;
using System.Data;



namespace 内网穿透辅助工具
{
    public delegate void SetTunnelNode(int tunnelId);
    public partial class Form1 : Form
    {
        /// <summary>
        /// 逻辑执行类
        /// </summary>
        public ToolService toolService = new ToolService();
        /// <summary>
        /// 服务类
        /// </summary>
        public CommonService commonService = new CommonService();
        /// <summary>
        /// 窗体控件自适应
        /// </summary>
        AutoSizeFormClass asc = new AutoSizeFormClass();

        /// <summary>
        /// 打开隧道配置节点按钮的事件
        /// </summary>
        public event SetTunnelNode? OnSetTunnelNode;

        /// <summary>
        /// 隧道列表数据
        /// </summary>
        DataTable dataTable = new DataTable();

        /// <summary>
        /// 用户信息类
        /// </summary>
        User user;

        public Form1()
        {
            InitializeComponent();
            InitDatatable();
            //添加数据变动事件
            toolService.OnChangeTunnelData += ChangeTunnelData;
            user = new User();
        }

        
        private void button2_Click(object sender, EventArgs e)
        {
            toolService.InstallService();
        }
        private void button3_Click(object sender, EventArgs e)
        {
            toolService.StartService();
        }
        private void button4_Click(object sender, EventArgs e)
        {
            toolService.StopService();
        }
        private void button5_Click(object sender, EventArgs e)
        {
            toolService.UninstallService();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //取消关闭窗口
            e.Cancel = true;
            HideForm();
        }
        /// <summary>
        /// 隐藏窗口
        /// </summary>
        private void HideForm()
        {
            //最小化窗口
            this.WindowState = FormWindowState.Minimized;
            //取消任务栏图标
            this.ShowInTaskbar = false;
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Dispose();
            this.Close();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrEmpty(this.mysqlAccount_textbox.Text) || string.IsNullOrEmpty(this.mysqlPassword_textbox.Text) || string.IsNullOrEmpty(this.mysqlIp_textbox.Text) || string.IsNullOrEmpty(this.mysqlPort_textbox.Text) || string.IsNullOrEmpty(this.mysqlDatabase_textbox.Text) || string.IsNullOrEmpty(this.mysqlTable_textbox.Text) || string.IsNullOrEmpty(this.mysqlField_textbox.Text))
            {
                return;
            }
            //保存数据库配置
            string ip = this.mysqlIp_textbox.Text.Trim();
            int port = Convert.ToInt32(this.mysqlPort_textbox.Text.Trim());
            string account = this.mysqlAccount_textbox.Text.Trim();
            string password = this.mysqlPassword_textbox.Text.Trim();
            string database = this.mysqlDatabase_textbox.Text.Trim();
            string table = this.mysqlTable_textbox.Text.Trim();
            string field = this.mysqlField_textbox.Text.Trim();
            Tunnel tunnel = this.mysqlTunnel_combox.SelectedItem as Tunnel;
            if (tunnel == null)
            {
                return;
            }
            int id = (this.mysqlTunnel_combox.SelectedItem as Tunnel).id;
            
            if (ToolData.config.tunnelMysqlInfos.ContainsKey(id))
            {
                var t = ToolData.config.tunnelMysqlInfos[id];
                t.account = account;
                t.password = password;
                t.database = database;
                t.table = table;
                t.field = field;
                t.ip= ip;
                t.port = port;
                t.tunnelIp= (this.mysqlTunnel_combox.SelectedItem as Tunnel).ip;
                t.where = this.mysqlWhere_textbox.Text.Trim();
                t.isAuto=this.isAutoMysql_checkbox.Checked;
            }
            else
            {
                var t = new MysqlConnectInfo();
                t.account = account;
                t.password = password;
                t.database = database;
                t.table = table;
                t.field = field;
                t.ip = ip;
                t.port = port;
                t.tunnelIp = (this.mysqlTunnel_combox.SelectedItem as Tunnel).ip;
                t.where = this.mysqlWhere_textbox.Text.Trim();
                t.isAuto= this.isAutoMysql_checkbox.Checked;
                ToolData.config.tunnelMysqlInfos.Add(id, t);
            }
            ToolData.SaveConfig();
        }
        /// <summary>
        /// 加载隧道列表提供数据库配置
        /// </summary>
        private void LoadTunnelListForDatabase()
        {
            this.mysqlTunnel_combox.DisplayMember = "name";
            this.mysqlTunnel_combox.ValueMember = "id";
            List<Tunnel> tunnels = ToolData.config.tunnels.Values.ToList();
            this.mysqlTunnel_combox.DataSource = tunnels;
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e is MouseEventArgs)
            {
                MouseEventArgs mouse_e = e as MouseEventArgs;
                if (mouse_e.Button == MouseButtons.Right)
                {
                    return;
                }
                if (this.WindowState == FormWindowState.Minimized)
                {
                    //还原窗体
                    this.WindowState = FormWindowState.Normal;
                    //任务栏显示
                    this.ShowInTaskbar = true;
                }
                //激活窗体
                this.Activate();
            }
        }
        
        private void InitDatatable()
        {
            //设置数据表格式
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataTable.Columns.Add("ID", typeof(int));
            dataTable.Columns.Add("启用", typeof(string));
            dataTable.Columns.Add("状态", typeof(string));
            dataTable.Columns.Add("类型", typeof(string));
            dataTable.Columns.Add("隧道名称", typeof(string));
            dataTable.Columns.Add("域名/端口", typeof(string));
            dataTable.Columns.Add("服务器", typeof(string));
            dataTable.Columns.Add("服务器ip", typeof(string));
        }

        /// <summary>
        /// 更新隧道列表
        /// </summary>
        private void ChangeTunnelData()
        {
            this.pictureBox1.Visible = true;
            this.dataGridView1.DataSource = null;
            dataTable.Rows.Clear();
            foreach (var tunnel in ToolData.config.tunnels.Values)
            {
                dataTable.Rows.Add(tunnel.id, tunnel.isAutoConnect ? "启用" : "未启用", "加载中",tunnel.type, tunnel.name, tunnel.dorp, tunnel.node, tunnel.ip);
            }
            dataGridView1.DataSource = dataTable;
            dataGridView1.Columns[0].Visible = false;
            for (int i = 0; i < 8; i++)
            {
                dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
            this.pictureBox1.Visible = false;

            //更新日志内容
            this.textBox1.Text = ToolData.logText;
            this.textBox1.ScrollToCaret();
            //更新隧道状态
            Task.Run(() => 
            {
                DataTable dataTable = (DataTable)dataGridView1.DataSource;
                foreach (DataRow row in dataTable.Rows)
                {
                    bool isConnected =  toolService.CheckVisit(row[3].ToString(), row[7].ToString(), row[5].ToString());
                    if (isConnected)
                    {
                        row[2] = "在线";
                    }
                    else
                    {
                        row[2] = "离线";
                    }
                }
                
            });
        }


        /// <summary>
        /// 保存账号密码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            string? username = this.username_textbox.Text?.Trim();
            string? password = this.password_textbox.Text?.Trim();
            user.Username = username;
            user.Password = password;
            commonService.Login(username, password);
            ToolData.LoadConfig();
            ChangeTunnelData();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            this.pictureBox1.Parent = dataGridView1;
            this.pictureBox1.SendToBack();
            //更新配置数据到表格和日志
            ChangeTunnelData();
            //窗体控件自适应
            asc.controllInitializeSize(this);


            //加载账号密码
            if (!string.IsNullOrEmpty(user.Username) && !string.IsNullOrEmpty(user.Password))
            {
                this.username_textbox.Text = user.Username;
                this.password_textbox.Text = user.Password;
            }

            //加载阿里云ddns配置内容
            if (!string.IsNullOrEmpty(ToolData.config.aliyunDdns.AccessKeyId))
            {
                this.aliyunAccessKeyId_textbox.Text = ToolData.config.aliyunDdns.AccessKeyId;
                this.aliyunAccessKeySecret_textbox.Text = ToolData.config.aliyunDdns.AccessKeySecret;
                this.isAutoAliyunDDNS_checkbox.Checked = ToolData.config.aliyunDdns.isAuto;
            }

            //加载邮箱配置内容
            if (!string.IsNullOrEmpty(ToolData.config.smtpMail.mailSmtpServer))
            {
                this.mailSmtpServer_textbox.Text = ToolData.config.smtpMail.mailSmtpServer;
                this.mailAccount_textbox.Text = ToolData.config.smtpMail.mailAccount;
                this.mailPassword_textbox.Text = ToolData.config.smtpMail.mailPassword;
                this.isAutoMail_checkbox.Checked = ToolData.config.smtpMail.isAuto;
                this.acceptMail_textbox.Text = ToolData.config.smtpMail.acceptMail;
            }

            //加载数据库内容
            LoadTunnelListForDatabase();
            if (!string.IsNullOrEmpty(this.mysqlTunnel_combox.SelectedValue?.ToString()))
            {
                int id =Convert.ToInt32(this.mysqlTunnel_combox.SelectedValue);
                if (ToolData.config.tunnelMysqlInfos.ContainsKey(id))
                {
                    var info = ToolData.config.tunnelMysqlInfos[id];
                    this.mysqlIp_textbox.Text = info.ip;
                    this.mysqlPort_textbox.Text = info.port.ToString();
                    this.mysqlAccount_textbox.Text = info.account;
                    this.mysqlPassword_textbox.Text = info.password;
                    this.mysqlDatabase_textbox.Text = info.database;
                    this.mysqlTable_textbox.Text = info.table;
                    this.mysqlField_textbox.Text = info.field;
                    this.mysqlWhere_textbox.Text = info.where;
                    this.isAutoMysql_checkbox.Checked = info.isAuto;
                }
            }

        }
        private void button9_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.aliyunAccessKeyId_textbox.Text) || string.IsNullOrEmpty(this.aliyunAccessKeySecret_textbox.Text)|| string.IsNullOrEmpty(this.isAutoAliyunDDNS_checkbox.Text))
            {
                return;
            }
            ToolData.config.aliyunDdns.AccessKeyId = this.aliyunAccessKeyId_textbox.Text.Trim();
            ToolData.config.aliyunDdns.AccessKeySecret = this.aliyunAccessKeySecret_textbox.Text.Trim();
            ToolData.config.aliyunDdns.isAuto = this.isAutoAliyunDDNS_checkbox.Checked;
            ToolData.SaveConfig();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrEmpty(this.mailSmtpServer_textbox.Text) || string.IsNullOrEmpty(this.mailAccount_textbox.Text) || string.IsNullOrEmpty(this.mailPassword_textbox.Text) || string.IsNullOrEmpty(this.acceptMail_textbox.Text))
            {
                return;
            }
            ToolData.config.smtpMail.mailSmtpServer = this.mailSmtpServer_textbox.Text.Trim();
            ToolData.config.smtpMail.mailAccount = this.mailAccount_textbox.Text.Trim();
            ToolData.config.smtpMail.mailPassword = this.mailPassword_textbox.Text.Trim();
            ToolData.config.smtpMail.acceptMail = this.acceptMail_textbox.Text.Trim();
            ToolData.config.smtpMail.isAuto = this.isAutoMail_checkbox.Checked;
            ToolData.SaveConfig();
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            //控件自适应大小
            asc.controlAutoSize(this);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (dataGridView1.DataSource == null || dataGridView1.Rows.Count==0)
            {
                MessageBox.Show("未选中隧道");
                return;
            }
            new Form2().Show();
            int id = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[0].Value);
            OnSetTunnelNode(id);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            ToolData.LoadConfig();
            ChangeTunnelData();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            ToolData.LoadLogText();
            //更新日志内容
            this.textBox1.Text = ToolData.logText;
            this.textBox1.ScrollToCaret();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            LoadTunnelListForDatabase();
        }

        private void mysqlTunnel_combox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.mysqlTunnel_combox.SelectedValue?.ToString()))
            {
                int id = Convert.ToInt32(this.mysqlTunnel_combox.SelectedValue);
                if (ToolData.config.tunnelMysqlInfos.ContainsKey(id))
                {
                    var info = ToolData.config.tunnelMysqlInfos[id];
                    this.mysqlIp_textbox.Text = info.ip;
                    this.mysqlPort_textbox.Text = info.port.ToString();
                    this.mysqlAccount_textbox.Text = info.account;
                    this.mysqlPassword_textbox.Text = info.password;
                    this.mysqlDatabase_textbox.Text = info.database;
                    this.mysqlTable_textbox.Text = info.table;
                    this.mysqlField_textbox.Text = info.field;
                    this.mysqlWhere_textbox.Text = info.where;
                    this.isAutoMysql_checkbox.Checked = info.isAuto;
                }
                else
                {
                    this.mysqlIp_textbox.Text = "";
                    this.mysqlPort_textbox.Text = "";
                    this.mysqlAccount_textbox.Text = "";
                    this.mysqlPassword_textbox.Text = "";
                    this.mysqlDatabase_textbox.Text = "";
                    this.mysqlTable_textbox.Text = "";
                    this.mysqlField_textbox.Text = "";
                    this.mysqlWhere_textbox.Text = "";
                    this.isAutoMysql_checkbox.Checked = false;
                }
            }
        }
    }
}
