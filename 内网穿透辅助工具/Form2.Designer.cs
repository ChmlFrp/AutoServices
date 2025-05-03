namespace 内网穿透辅助工具
{
    partial class Form2
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            this.node_combox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.add_button = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.level = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.delete_button = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.level_textbox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // node_combox
            // 
            this.node_combox.FormattingEnabled = true;
            this.node_combox.Location = new System.Drawing.Point(65, 31);
            this.node_combox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.node_combox.Name = "node_combox";
            this.node_combox.Size = new System.Drawing.Size(144, 20);
            this.node_combox.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(30, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "节点:";
            // 
            // add_button
            // 
            this.add_button.Location = new System.Drawing.Point(65, 96);
            this.add_button.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.add_button.Name = "add_button";
            this.add_button.Size = new System.Drawing.Size(64, 31);
            this.add_button.TabIndex = 2;
            this.add_button.Text = "添加";
            this.add_button.UseVisualStyleBackColor = true;
            this.add_button.Click += new System.EventHandler(this.add_button_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.name,
            this.level});
            this.dataGridView1.Location = new System.Drawing.Point(10, 135);
            this.dataGridView1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowTemplate.Height = 25;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.Size = new System.Drawing.Size(306, 139);
            this.dataGridView1.TabIndex = 3;
            // 
            // name
            // 
            this.name.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.name.DataPropertyName = "name";
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.name.DefaultCellStyle = dataGridViewCellStyle7;
            this.name.FillWeight = 60F;
            this.name.HeaderText = "节点名称";
            this.name.Name = "name";
            this.name.ReadOnly = true;
            // 
            // level
            // 
            this.level.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.level.DataPropertyName = "level";
            this.level.FillWeight = 40F;
            this.level.HeaderText = "优先级";
            this.level.Name = "level";
            this.level.ReadOnly = true;
            // 
            // delete_button
            // 
            this.delete_button.Location = new System.Drawing.Point(145, 96);
            this.delete_button.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.delete_button.Name = "delete_button";
            this.delete_button.Size = new System.Drawing.Size(64, 31);
            this.delete_button.TabIndex = 4;
            this.delete_button.Text = "删除";
            this.delete_button.UseVisualStyleBackColor = true;
            this.delete_button.Click += new System.EventHandler(this.delete_button_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(20, 63);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 12);
            this.label2.TabIndex = 5;
            this.label2.Text = "优先级:";
            // 
            // level_textbox
            // 
            this.level_textbox.Location = new System.Drawing.Point(65, 61);
            this.level_textbox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.level_textbox.Name = "level_textbox";
            this.level_textbox.Size = new System.Drawing.Size(86, 21);
            this.level_textbox.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.Blue;
            this.label3.Location = new System.Drawing.Point(161, 63);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(95, 12);
            this.label3.TabIndex = 7;
            this.label3.Text = "整数,越大越靠前";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(127, 8);
            this.checkBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(48, 16);
            this.checkBox1.TabIndex = 8;
            this.checkBox1.Text = "启用";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(216, 29);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(40, 23);
            this.button1.TabIndex = 10;
            this.button1.Text = "刷新";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(327, 294);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.level_textbox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.delete_button);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.add_button);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.node_combox);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form2";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "隧道配置";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form2_FormClosed);
            this.Load += new System.EventHandler(this.Form2_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ComboBox node_combox;
        private Label label1;
        private Button add_button;
        private DataGridView dataGridView1;
        private Button delete_button;
        private Label label2;
        private TextBox level_textbox;
        private Label label3;
        private DataGridViewTextBoxColumn name;
        private DataGridViewTextBoxColumn level;
        private CheckBox checkBox1;
        private Button button1;
    }
}