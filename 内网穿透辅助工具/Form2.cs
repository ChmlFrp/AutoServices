using FRPAutoCheckService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 内网穿透辅助工具
{
    public partial class Form2 : Form
    {
        DataTable dt;
        int tunnelId;
        CommonService commonService = new CommonService();
        public Form2()
        {
            InitializeComponent();
            SetComboxData();
            dt = new DataTable();
            dt.Columns.Add("name", typeof(string));
            dt.Columns.Add("level", typeof(int));
            dataGridView1.DataSource = dt;
            //添加接收隧道ID的事件
            Program.MainForm.OnSetTunnelNode += SetTunnelId;
        }
        private void SetTunnelId(int tunnelId)
        {
            this.tunnelId = tunnelId;
            //加载隧道自动切换的节点列表
            LoadTunnelNode();
            //设置启用按钮
            this.checkBox1.Checked = CommonData.config.tunnels[tunnelId].isAutoConnect;
        }
        private void SetComboxData()
        {
            this.node_combox.DisplayMember = "name";
            this.node_combox.ValueMember = "id";
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            //加载服务器节点列表
            LoadNodeList();
        }
        /// <summary>
        /// 加载服务器节点列表
        /// </summary>
        private void LoadNodeList()
        {
            CommonData.LoadConfig();
            //加载服务器节点列表
            List<Node> nodes = CommonData.config.nodes.Values.ToList();
            nodes.Sort();
            this.node_combox.DataSource = nodes;
        }
        /// <summary>
        /// 加载切换节点列表
        /// </summary>
        private void LoadTunnelNode()
        {
            dt.Clear();
            Dictionary<string, Node> nodes;
            if (CommonData.config.tunnelNodes.ContainsKey(tunnelId))
            {
                nodes = CommonData.config.tunnelNodes[tunnelId];
            }
            else
            {
                nodes = new Dictionary<string, Node>();
                CommonData.config.tunnelNodes[tunnelId] = nodes;
            }
            List<Node> tempList = nodes.Values.ToList();
            tempList.Sort();
            foreach (var node in tempList)
            {
                dt.Rows.Add(node.name, node.level);
            }

        }
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void add_button_Click(object sender, EventArgs e)
        {
            Dictionary<string, Node> nodes = CommonData.config.tunnelNodes[tunnelId];
            int id = Convert.ToInt32(node_combox.SelectedValue);
            string? nodeName = (node_combox.SelectedItem as Node).name;
            if (nodes.ContainsKey(nodeName!))
            {
                return;
            }
            int level = Convert.ToInt32(this.level_textbox.Text.Trim());
            Node node = new Node();
            node.id = id;
            node.name = nodeName;
            node.level = level;
            nodes.Add(nodeName!, node);
            dt.Rows.Add(node.name, node.level);
            CommonData.SaveConfig();
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void delete_button_Click(object sender, EventArgs e)
        {
            Dictionary<string, Node> nodes = CommonData.config.tunnelNodes[tunnelId];
            int index = dataGridView1.SelectedRows[0].Index;
            string nodeName = dataGridView1.SelectedRows[0].Cells[0].Value.ToString();
            nodes.Remove(nodeName!);
            dataGridView1.Rows.RemoveAt(index);
            CommonData.SaveConfig();
        }
        /// <summary>
        /// 启用隧道
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            CommonData.config.tunnels[tunnelId].isAutoConnect= checkBox1.Checked;
            CommonData.SaveConfig();
        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            //添加接收隧道ID的事件
            Program.MainForm.OnSetTunnelNode -= SetTunnelId;
        }
        /// <summary>
        /// 刷新节点列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            commonService.LoadNodeList();
            LoadNodeList();
            LoadTunnelNode();
        }
    }
}
