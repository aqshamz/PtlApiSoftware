namespace Ptl.Gui.Forms
{
    partial class SettingsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            groupPtl = new GroupBox();
            lblGroupZona = new Label();
            txtGroupZona = new TextBox();

            groupDb = new GroupBox();
            lblHost = new Label();
            txtHost = new TextBox();
            lblPort = new Label();
            txtPort = new TextBox();
            lblDbName = new Label();
            txtDbName = new TextBox();
            lblUsername = new Label();
            txtUsername = new TextBox();
            lblPassword = new Label();
            txtPassword = new TextBox();

            btnTest = new Button();
            btnSave = new Button();
            btnCancel = new Button();

            SuspendLayout();

            // ======================
            // GROUP PTL
            // ======================
            groupPtl.Text = "PTL Settings";
            groupPtl.Location = new Point(20, 20);
            groupPtl.Size = new Size(350, 80);

            lblGroupZona.Text = "Group Zona";
            lblGroupZona.Location = new Point(15, 35);
            lblGroupZona.Size = new Size(100, 20);

            txtGroupZona.Location = new Point(120, 32);
            txtGroupZona.Size = new Size(200, 23);

            groupPtl.Controls.Add(lblGroupZona);
            groupPtl.Controls.Add(txtGroupZona);

            // ======================
            // GROUP DB
            // ======================
            groupDb.Text = "Database";
            groupDb.Location = new Point(20, 110);
            groupDb.Size = new Size(350, 250);

            // Host
            lblHost.Text = "Host";
            lblHost.Location = new Point(15, 30);
            txtHost.Location = new Point(120, 30);
            txtHost.Size = new Size(200, 23);

            // Port
            lblPort.Text = "Port";
            lblPort.Location = new Point(15, 70);
            txtPort.Location = new Point(120, 70);
            txtPort.Size = new Size(200, 23);

            // DB Name
            lblDbName.Text = "DB Name";
            lblDbName.Location = new Point(15, 110);
            txtDbName.Location = new Point(120, 110);
            txtDbName.Size = new Size(200, 23);

            // Username
            lblUsername.Text = "Username";
            lblUsername.Location = new Point(15, 150);
            txtUsername.Location = new Point(120, 150);
            txtUsername.Size = new Size(200, 23);

            // Password
            lblPassword.Text = "Password";
            lblPassword.Location = new Point(15, 190);
            txtPassword.Location = new Point(120, 190);
            txtPassword.Size = new Size(200, 23);
            txtPassword.UseSystemPasswordChar = true;
            txtPassword.TextChanged += txtPassword_TextChanged;

            btnTest.Text = "Test Connection";
            btnTest.Location = new Point(120, 220);
            btnTest.Size = new Size(200, 30);
            btnTest.BackColor = Color.LightSkyBlue;
            btnTest.Click += btnTest_Click;

            groupDb.Controls.Add(btnTest);
            groupDb.Controls.Add(lblHost);
            groupDb.Controls.Add(txtHost);
            groupDb.Controls.Add(lblPort);
            groupDb.Controls.Add(txtPort);
            groupDb.Controls.Add(lblDbName);
            groupDb.Controls.Add(txtDbName);
            groupDb.Controls.Add(lblUsername);
            groupDb.Controls.Add(txtUsername);
            groupDb.Controls.Add(lblPassword);
            groupDb.Controls.Add(txtPassword);

            // ======================
            // BUTTONS
            // ======================
            btnSave.Text = "Save";
            btnSave.Location = new Point(80, 380);
            btnSave.Size = new Size(100, 40);
            btnSave.BackColor = Color.LightGreen;
            btnSave.Click += btnSave_Click;

            btnCancel.Text = "Cancel";
            btnCancel.Location = new Point(200, 380);
            btnCancel.Size = new Size(100, 40);
            btnCancel.BackColor = Color.LightCoral;
            btnCancel.Click += btnCancel_Click;

            // ======================
            // FORM
            // ======================
            ClientSize = new Size(400, 450);
            Controls.Add(groupPtl);
            Controls.Add(groupDb);
            Controls.Add(btnSave);
            Controls.Add(btnCancel);

            Text = "Settings";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            ResumeLayout(false);
        }

        #endregion

        private GroupBox groupPtl;
        private Label lblGroupZona;
        private TextBox txtGroupZona;

        private GroupBox groupDb;
        private Label lblHost;
        private Label lblPort;
        private Label lblDbName;
        private Label lblUsername;
        private Label lblPassword;

        private TextBox txtHost;
        private TextBox txtPort;
        private TextBox txtDbName;
        private TextBox txtUsername;
        private TextBox txtPassword;

        private Button btnSave;
        private Button btnCancel;
        private Button btnTest;
    }
}