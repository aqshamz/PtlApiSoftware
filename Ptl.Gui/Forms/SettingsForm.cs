using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ptl.Gui.Forms
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
        }

        public int GroupZona => int.TryParse(txtGroupZona.Text, out var z) ? z : 0;
        public string Host => txtHost.Text;
        public string Port => txtPort.Text;
        public string DbName => txtDbName.Text;
        public string Username => txtUsername.Text;
        public string Password => _passwordChanged ? txtPassword.Text : _originalPassword;
        private bool _passwordChanged = false;
        private string _originalPassword = "";

        public void LoadSettings(int groupZona, string host, string port, string db, string user, string pass)
        {
            txtGroupZona.Text = groupZona.ToString();
            txtHost.Text = host;
            txtPort.Text = port;
            txtDbName.Text = db;
            txtUsername.Text = user;

            _originalPassword = pass;
            txtPassword.Text = "*****"; // mask
            _passwordChanged = false;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (GroupZona <= 0)
            {
                MessageBox.Show("Group Zona must be > 0");
                return;
            }

            if (string.IsNullOrWhiteSpace(Host) ||
                string.IsNullOrWhiteSpace(Port) ||
                string.IsNullOrWhiteSpace(DbName) ||
                string.IsNullOrWhiteSpace(Username))
            {
                MessageBox.Show("All database fields are required");
                return;
            }

            // if password masked but not changed → allow
            if (_passwordChanged && string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Password cannot be empty");
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private async void btnTest_Click(object sender, EventArgs e)
        {
            try
            {
                var password = _passwordChanged ? txtPassword.Text : _originalPassword;
                var cs = $"Host={Host};Port={Port};Database={DbName};Username={Username};Password={password}";

                using var conn = new Npgsql.NpgsqlConnection(cs);
                await conn.OpenAsync();

                MessageBox.Show("✅ Connection successful!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Connection failed:\n{ex.Message}");
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {
            _passwordChanged = true;
        }
    }
}
