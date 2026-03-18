using System.Diagnostics;
using System.Net.Http.Json;
using Ptl.Contracts.Dtos.Hardware;

namespace Ptl.Gui
{
    public partial class Form1 : Form
    {
        private ProcessLauncher _launcher = new();
        private ApiHealthChecker _healthChecker = new();
        private readonly HttpClient _client = new HttpClient();

        private Process? _apiProcess;
        private Process? _hardwareProcess;

        private bool _isRefreshing = false;

        private System.Windows.Forms.Timer _statusTimer = new();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _statusTimer.Interval = 2000; // every 2 sec
            _statusTimer.Tick += async (s, ev) => await RefreshStatus();
        }

        private void AppendLog(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(AppendLog), message);
                return;
            }

            txtLogs.AppendText(
                $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}"
            );

            txtLogs.SelectionStart = txtLogs.Text.Length;
            txtLogs.ScrollToCaret();
        }

        private async void btnStartSystem_Click(object sender, EventArgs e)
        {
            AppendLog("Starting PTL System...");

            UpdateApiStatus(false);
            UpdateHardwareStatus(false);
            UpdateDbStatus(false);

            string apiPath = @"..\..\..\..\WebApplication1\bin\Debug\net8.0\Ptl.Api.exe";
            string hwPath = @"..\..\..\..\Ptl.Hardware\bin\Debug\net8.0-windows\Ptl.Hardware.exe";

            _apiProcess = _launcher.Start(apiPath, msg => AppendLog("[API] " + msg));

            AppendLog("API started");
            AppendLog("Checking API readiness...");

            bool ready = await _healthChecker.WaitUntilReady(
                "http://127.0.0.1:5000/health",
                AppendLog
            );

            if (!ready)
            {
                AppendLog("API failed to start");
                return;
            }

            AppendLog("API is ready");
            UpdateApiStatus(true);
            await LoadGateways();

            _hardwareProcess = _launcher.Start(hwPath, msg => AppendLog("[HW] " + msg));

            AppendLog("Hardware started");
            UpdateHardwareStatus(true);

            AppendLog("System started");

            btnStartSystem.Enabled = false;
            btnStopSystem.Enabled = true;

            _statusTimer.Start();
        }

        private void btnStopSystem_Click(object sender, EventArgs e)
        {
            AppendLog("Stopping system...");
            _statusTimer.Stop();
            try
            {
                if (_apiProcess != null && !_apiProcess.HasExited)
                    _apiProcess.Kill();

                if (_hardwareProcess != null && !_hardwareProcess.HasExited)
                    _hardwareProcess.Kill();
            }
            catch { }

            AppendLog("System stopped");

            btnStartSystem.Enabled = true;
            btnStopSystem.Enabled = false;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                _hardwareProcess?.Kill();
                _apiProcess?.Kill();
            }
            catch { }

            base.OnFormClosing(e);
        }

        void UpdateApiStatus(bool isUp)
        {
            lblApiStatus.Text = $"API: {(isUp ? "🟢 Running" : "🔴 Stopped")}";
            lblApiStatus.ForeColor = isUp ? Color.Green : Color.Red;
        }

        void UpdateHardwareStatus(bool isUp)
        {
            lblHardwareStatus.Text = $"Hardware: {(isUp ? "🟢 Running" : "🔴 Stopped")}";
            lblHardwareStatus.ForeColor = isUp ? Color.Green : Color.Red;
        }

        void UpdateDbStatus(bool isUp)
        {
            lblDbStatus.Text = $"DB: {(isUp ? "🟢 Connected" : "🔴 Disconnected")}";
            lblDbStatus.ForeColor = isUp ? Color.Green : Color.Red;
        }

        async Task LoadGateways()
        {
            try
            {
                var res = await _client.GetAsync("http://127.0.0.1:5000/ptl/hardware/gateways");

                if (!res.IsSuccessStatusCode)
                {
                    PtlLog.Error($"LoadGateways failed: {res.StatusCode}");
                    return;
                }

                var gateways = await res.Content.ReadFromJsonAsync<List<PtlGatewayConfig>>();

                if (gateways == null)
                {
                    PtlLog.Error("Gateway response is null");
                    return;
                }

                dgvGateways.Rows.Clear();

                foreach (var g in gateways)
                {
                    dgvGateways.Rows.Add(
                        g.GatewayId,
                        g.IpAddress,
                        "🟡 Unknown"
                    );
                }
            }
            catch (Exception ex)
            {
                PtlLog.Error($"Failed to load gateways: {ex.Message}");
            }
        }

        async Task RefreshStatus()
        {
            if (_isRefreshing) return;

            _isRefreshing = true;

            try
            {
                // 🔹 Gateway status
                var res = await _client.GetAsync("http://127.0.0.1:5000/ptl/hardware/status");

                if (!res.IsSuccessStatusCode)
                    throw new Exception($"Status API error: {res.StatusCode}");

                var connected = await res.Content.ReadFromJsonAsync<List<GatewayRuntimeInfo>>()
                                ?? new List<GatewayRuntimeInfo>();

                UpdateApiStatus(true);

                // 🔹 DB status
                bool dbOk = false;

                try
                {
                    var dbRes = await _client.GetAsync("http://127.0.0.1:5000/database/db-status");

                    if (dbRes.IsSuccessStatusCode)
                    {
                        dbOk = await dbRes.Content.ReadFromJsonAsync<bool>();
                    }
                }
                catch
                {
                    dbOk = false;
                }

                UpdateDbStatus(dbOk);

                foreach (DataGridViewRow row in dgvGateways.Rows)
                {
                    int id = Convert.ToInt32(row.Cells[0].Value);

                    var isConnected = connected.Any(x => x.GatewayId == id);

                    row.Cells[2].Value = isConnected
                        ? "🟢 Connected"
                        : "🔴 Disconnected";

                    row.DefaultCellStyle.BackColor = isConnected
                        ? Color.LightGreen
                        : Color.LightCoral;
                }
            }
            catch (Exception ex)
            {
                UpdateApiStatus(false);
                UpdateDbStatus(false);

                PtlLog.Error($"Refresh failed: {ex.Message}");

                foreach (DataGridViewRow row in dgvGateways.Rows)
                {
                    row.Cells[2].Value = "⚫ Unknown";
                    row.DefaultCellStyle.BackColor = Color.LightGray;
                }
            }
            finally
            {
                _isRefreshing = false;
            }
        }

        private void dgvGateways_CLick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void lblApiStatus_Click(object sender, EventArgs e)
        {

        }

        private void lblHardwareStatus_Click(object sender, EventArgs e)
        {

        }

        private void lblDbStatus_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }
    }
}