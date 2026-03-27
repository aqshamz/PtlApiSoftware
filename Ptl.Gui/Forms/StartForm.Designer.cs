namespace Ptl.Gui
{
    partial class StartForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            txtLogs = new RichTextBox();
            btnStartSystem = new Button();
            btnStopSystem = new Button();
            lblApiStatus = new Label();
            lblDbStatus = new Label();
            lblHardwareStatus = new Label();
            dgvGateways = new DataGridView();
            GateawayId = new DataGridViewTextBoxColumn();
            IP = new DataGridViewTextBoxColumn();
            Status = new DataGridViewTextBoxColumn();
            groupBox1 = new GroupBox();
            btnSettings = new Button();
            groupControls = new GroupBox();
            lblSeparator = new Label();
            ((System.ComponentModel.ISupportInitialize)dgvGateways).BeginInit();
            groupBox1.SuspendLayout();
            groupControls.SuspendLayout();
            SuspendLayout();
            // 
            // txtLogs
            // 
            txtLogs.Dock = DockStyle.Bottom;
            txtLogs.Font = new Font("Consolas", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtLogs.Location = new Point(0, 270);
            txtLogs.Name = "txtLogs";
            txtLogs.ReadOnly = true;
            txtLogs.Size = new Size(800, 180);
            txtLogs.TabIndex = 4;
            txtLogs.Text = "";
            // 
            // btnStartSystem
            // 
            btnStartSystem.BackColor = Color.LightGreen;
            btnStartSystem.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnStartSystem.Location = new Point(150, 30);
            btnStartSystem.Name = "btnStartSystem";
            btnStartSystem.Size = new Size(140, 40);
            btnStartSystem.TabIndex = 0;
            btnStartSystem.Text = "▶ Start";
            btnStartSystem.UseVisualStyleBackColor = false;
            btnStartSystem.Click += btnStartSystem_Click;
            // 
            // btnStopSystem
            // 
            btnStopSystem.BackColor = Color.LightCoral;
            btnStopSystem.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnStopSystem.Location = new Point(310, 30);
            btnStopSystem.Name = "btnStopSystem";
            btnStopSystem.Size = new Size(140, 40);
            btnStopSystem.TabIndex = 1;
            btnStopSystem.Text = "⏹ Stop";
            btnStopSystem.UseVisualStyleBackColor = false;
            btnStopSystem.Click += btnStopSystem_Click;
            // 
            // lblApiStatus
            // 
            lblApiStatus.AutoSize = true;
            lblApiStatus.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblApiStatus.Location = new Point(15, 30);
            lblApiStatus.Name = "lblApiStatus";
            lblApiStatus.Size = new Size(76, 19);
            lblApiStatus.TabIndex = 7;
            lblApiStatus.Text = "Api Status";
            lblApiStatus.Click += lblApiStatus_Click;
            // 
            // lblDbStatus
            // 
            lblDbStatus.AutoSize = true;
            lblDbStatus.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblDbStatus.Location = new Point(15, 65);
            lblDbStatus.Name = "lblDbStatus";
            lblDbStatus.Size = new Size(115, 19);
            lblDbStatus.TabIndex = 8;
            lblDbStatus.Text = "Database Status";
            lblDbStatus.Click += lblDbStatus_Click;
            // 
            // lblHardwareStatus
            // 
            lblHardwareStatus.AutoSize = true;
            lblHardwareStatus.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblHardwareStatus.Location = new Point(15, 100);
            lblHardwareStatus.Name = "lblHardwareStatus";
            lblHardwareStatus.Size = new Size(120, 19);
            lblHardwareStatus.TabIndex = 9;
            lblHardwareStatus.Text = "Hardware Status";
            lblHardwareStatus.Click += lblHardwareStatus_Click;
            // 
            // dgvGateways
            // 
            dgvGateways.AllowUserToAddRows = false;
            dgvGateways.AllowUserToResizeRows = false;
            dgvGateways.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvGateways.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvGateways.Columns.AddRange(new DataGridViewColumn[] { GateawayId, IP, Status });
            dgvGateways.Location = new Point(297, 3);
            dgvGateways.Name = "dgvGateways";
            dgvGateways.RowHeadersVisible = false;
            dgvGateways.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvGateways.Size = new Size(470, 171);
            dgvGateways.TabIndex = 10;
            dgvGateways.CellContentClick += dgvGateways_CLick;
            // 
            // GateawayId
            // 
            GateawayId.HeaderText = "Gateaway";
            GateawayId.Name = "GateawayId";
            // 
            // IP
            // 
            IP.HeaderText = "IP";
            IP.Name = "IP";
            // 
            // Status
            // 
            Status.HeaderText = "Status";
            Status.Name = "Status";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(lblApiStatus);
            groupBox1.Controls.Add(lblHardwareStatus);
            groupBox1.Controls.Add(lblDbStatus);
            groupBox1.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            groupBox1.Location = new Point(20, 20);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(250, 150);
            groupBox1.TabIndex = 11;
            groupBox1.TabStop = false;
            groupBox1.Text = "System Status";
            groupBox1.Enter += groupBox1_Enter;
            // 
            // btnSettings
            // 
            btnSettings.BackColor = Color.LightGray;
            btnSettings.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnSettings.Location = new Point(500, 30);
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new Size(140, 40);
            btnSettings.TabIndex = 3;
            btnSettings.Text = "⚙ Settings";
            btnSettings.UseVisualStyleBackColor = false;
            btnSettings.Click += btnSettings_Click;
            // 
            // groupControls
            // 
            groupControls.Controls.Add(btnStartSystem);
            groupControls.Controls.Add(btnStopSystem);
            groupControls.Controls.Add(lblSeparator);
            groupControls.Controls.Add(btnSettings);
            groupControls.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            groupControls.Location = new Point(20, 180);
            groupControls.Name = "groupControls";
            groupControls.Size = new Size(747, 84);
            groupControls.TabIndex = 12;
            groupControls.TabStop = false;
            groupControls.Text = "Controls";
            // 
            // lblSeparator
            // 
            lblSeparator.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblSeparator.Location = new Point(470, 32);
            lblSeparator.Name = "lblSeparator";
            lblSeparator.Size = new Size(20, 30);
            lblSeparator.TabIndex = 2;
            lblSeparator.Text = "|";
            // 
            // StartForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(groupBox1);
            Controls.Add(dgvGateways);
            Controls.Add(groupControls);
            Controls.Add(txtLogs);
            Name = "StartForm";
            Text = "PTL Control Panel";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)dgvGateways).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupControls.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private RichTextBox txtLogs;
        private Button btnStartSystem;
        private Button btnStopSystem;
        private Label lblApiStatus;
        private Label lblDbStatus;
        private Label lblHardwareStatus;
        private DataGridView dgvGateways;
        private DataGridViewTextBoxColumn GateawayId;
        private DataGridViewTextBoxColumn IP;
        private DataGridViewTextBoxColumn Status;
        private GroupBox groupBox1;
        private Button btnSettings;
        private GroupBox groupControls;
        private Label lblSeparator;
    }
}
