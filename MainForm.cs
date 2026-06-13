using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace StockViewer
{
    public partial class MainForm : Form
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_ALT = 0x0001;
        private const uint VK_S = 0x53;
        private const uint VK_LEFT = 0x25;
        private const uint VK_RIGHT = 0x27;
        private const int HOTKEY_ID = 9000;
        private const int HOTKEY_PREV = 9001;
        private const int HOTKEY_NEXT = 9002;

        private StockDataService _stockService;
        private AppSettings _settings;
        private NotifyIcon _notifyIcon;
        private ContextMenuStrip _contextMenu;
        private Timer _refreshTimer;
        private Timer _scrollTimer;
        private Label _stockLabel;
        private Point _lastLocation;
        private bool _isDragging;
        private bool _hasMovedWhileDragging;
        private bool _isRefreshing;
        private int _currentStockIndex;

        public MainForm()
        {
            InitializeComponent();
            InitializeSettings();
            InitializeUI();
            InitializeServices();
            InitializeSystemTray();
            InitializeHotkey();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            AutoScaleDimensions = new SizeF(6F, 12F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(300, 30);
            ControlBox = false;
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "MainForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            Text = "桌面看股小工具";
            TopMost = true;
            TransparencyKey = Color.Black;

            ResumeLayout(false);
        }

        private void InitializeSettings()
        {
            _settings = AppSettings.Load();
        }

        private void InitializeUI()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);

            _stockLabel = new Label
            {
                AutoSize = true,
                BackColor = Color.Transparent,
                Font = new Font("微软雅黑", 12F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = Color.White,
                Location = new Point(6, 4),
                Text = "加载中..."
            };

            _stockLabel.UseCompatibleTextRendering = false;
            Controls.Add(_stockLabel);

            _stockLabel.MouseDown += Label_MouseDown;
            _stockLabel.MouseMove += Label_MouseMove;
            _stockLabel.MouseUp += Label_MouseUp;
            _stockLabel.MouseClick += Label_MouseClick;

            MouseDown += MainForm_MouseDown;
            MouseMove += MainForm_MouseMove;
            MouseUp += MainForm_MouseUp;
            MouseClick += MainForm_MouseClick;

            AdjustWindowSize();
        }

        private void InitializeServices()
        {
            _stockService = new StockDataService();

            _refreshTimer = new Timer { Interval = 3000 };
            _refreshTimer.Tick += RefreshTimer_Tick;
            _refreshTimer.Start();

            _scrollTimer = new Timer { Interval = _settings.SwitchInterval * 1000 };
            _scrollTimer.Tick += ScrollTimer_Tick;
            if (_settings.StockCodes.Count > 1)
            {
                _scrollTimer.Start();
            }

            RefreshStockData();
        }

        private void InitializeSystemTray()
        {
            _notifyIcon = new NotifyIcon();
            _notifyIcon.Icon = ResourceManager.GetApplicationIcon();
            _notifyIcon.Text = "桌面看股小工具";
            _notifyIcon.Visible = true;

            _contextMenu = new ContextMenuStrip();
            _contextMenu.Items.Add("显示/隐藏", null, ShowHide_Click);
            _contextMenu.Items.Add("-");
            _contextMenu.Items.Add("股票设置", null, StockSettings_Click);
            _contextMenu.Items.Add("颜色设置", null, ColorSettings_Click);
            _contextMenu.Items.Add("字体设置", null, FontSettings_Click);
            _contextMenu.Items.Add("显示设置", null, DisplaySettings_Click);
            _contextMenu.Items.Add("启动设置", null, AutoStartSettings_Click);
            _contextMenu.Items.Add("-");
            _contextMenu.Items.Add("请我喝咖啡", null, Donate_Click);
            _contextMenu.Items.Add("关于", null, About_Click);
            _contextMenu.Items.Add("-");
            _contextMenu.Items.Add("退出", null, Exit_Click);

            _notifyIcon.ContextMenuStrip = _contextMenu;
            _notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
        }

        private void InitializeHotkey()
        {
            RegisterHotKey(Handle, HOTKEY_ID, MOD_CONTROL | MOD_ALT, VK_S);
            RegisterHotKey(Handle, HOTKEY_PREV, MOD_CONTROL, VK_LEFT);
            RegisterHotKey(Handle, HOTKEY_NEXT, MOD_CONTROL, VK_RIGHT);
        }

        private void LoadSettings()
        {
            if (_settings.WindowLocation != Point.Empty)
            {
                Location = _settings.WindowLocation;
            }

            _stockLabel.Font = new Font("微软雅黑", _settings.FontSize, _settings.FontStyle, GraphicsUnit.Point);
            _stockLabel.ForeColor = _settings.FontColor;
            AdjustWindowSize();
        }

        private void AdjustWindowSize()
        {
            Size textSize = TextRenderer.MeasureText(
                _stockLabel.Text,
                _stockLabel.Font,
                new Size(int.MaxValue, int.MaxValue),
                TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);

            Size = new Size(Math.Max(120, textSize.Width + 16), Math.Max(24, textSize.Height + 8));
            _stockLabel.Location = new Point(6, 4);
        }

        private async void RefreshStockData()
        {
            if (_isRefreshing)
            {
                return;
            }

            if (_settings.StockCodes.Count == 0)
            {
                _stockLabel.Text = "请先设置股票代码";
                _stockLabel.ForeColor = Color.Yellow;
                AdjustWindowSize();
                return;
            }

            try
            {
                _isRefreshing = true;
                string stockCode = _settings.StockCodes[_currentStockIndex];
                var stockData = await _stockService.GetStockDataAsync(stockCode);

                if (stockData != null)
                {
                    UpdateStockDisplay(stockData);
                }
                else
                {
                    _stockLabel.Text = "行情获取失败";
                    _stockLabel.ForeColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                _stockLabel.Text = $"错误: {ex.Message}";
                _stockLabel.ForeColor = Color.Red;
            }
            finally
            {
                _isRefreshing = false;
            }

            AdjustWindowSize();
        }

        private void UpdateStockDisplay(StockData stockData)
        {
            _stockLabel.Text = _settings.ShowStockName
                ? $"{stockData.Code} {stockData.Name} {stockData.Price:F2} {stockData.ChangePercent:+0.00;-0.00}%"
                : $"{stockData.Code} {stockData.Price:F2} {stockData.ChangePercent:+0.00;-0.00}%";

            switch (_settings.ColorMode)
            {
                case ColorMode.红涨绿跌:
                    _stockLabel.ForeColor = stockData.Change >= 0 ? Color.Red : Color.Green;
                    break;
                case ColorMode.绿涨红跌:
                    _stockLabel.ForeColor = stockData.Change >= 0 ? Color.Green : Color.Red;
                    break;
                case ColorMode.固定颜色:
                default:
                    _stockLabel.ForeColor = _settings.FontColor;
                    break;
            }
        }

        private void Label_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = true;
                _hasMovedWhileDragging = false;
                _lastLocation = e.Location;
            }
        }

        private void Label_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                _hasMovedWhileDragging = true;
                Point newLocation = new Point(
                    (Location.X - _lastLocation.X) + e.X,
                    (Location.Y - _lastLocation.Y) + e.Y);

                Rectangle workingArea = Screen.GetWorkingArea(this);
                if (newLocation.X < 0) newLocation.X = 0;
                if (newLocation.Y < 0) newLocation.Y = 0;
                if (newLocation.X + Width > workingArea.Right) newLocation.X = workingArea.Right - Width;
                if (newLocation.Y + Height > workingArea.Bottom) newLocation.Y = workingArea.Bottom - Height;

                Location = newLocation;
            }
        }

        private void Label_MouseUp(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                _settings.WindowLocation = Location;
                _settings.Save();
            }
        }

        private void Label_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                _contextMenu.Show(PointToScreen(e.Location));
            }
            else if (e.Button == MouseButtons.Left && !_hasMovedWhileDragging)
            {
                SwitchToNextStock();
            }
        }

        private void MainForm_MouseDown(object sender, MouseEventArgs e) => Label_MouseDown(sender, e);
        private void MainForm_MouseMove(object sender, MouseEventArgs e) => Label_MouseMove(sender, e);
        private void MainForm_MouseUp(object sender, MouseEventArgs e) => Label_MouseUp(sender, e);
        private void MainForm_MouseClick(object sender, MouseEventArgs e) => Label_MouseClick(sender, e);

        private void SwitchToNextStock()
        {
            if (_settings.StockCodes.Count <= 1) return;

            _scrollTimer.Stop();
            _currentStockIndex = (_currentStockIndex + 1) % _settings.StockCodes.Count;
            RefreshStockData();

            if (_settings.StockCodes.Count > 1)
            {
                _scrollTimer.Start();
            }
        }

        private void SwitchToPrevStock()
        {
            if (_settings.StockCodes.Count <= 1) return;

            _scrollTimer.Stop();
            _currentStockIndex = (_currentStockIndex - 1 + _settings.StockCodes.Count) % _settings.StockCodes.Count;
            RefreshStockData();

            if (_settings.StockCodes.Count > 1)
            {
                _scrollTimer.Start();
            }
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            RefreshStockData();
        }

        private void ScrollTimer_Tick(object sender, EventArgs e)
        {
            if (_settings.StockCodes.Count > 1)
            {
                _currentStockIndex = (_currentStockIndex + 1) % _settings.StockCodes.Count;
                RefreshStockData();
            }
        }

        private void StockSettings_Click(object sender, EventArgs e)
        {
            var settingsForm = new StockSettingsForm(_settings);
            if (settingsForm.ShowDialog() == DialogResult.OK)
            {
                _settings.Save();
                _scrollTimer.Stop();
                _currentStockIndex = 0;
                if (_settings.StockCodes.Count > 1)
                {
                    _scrollTimer.Start();
                }

                RefreshStockData();
            }
        }

        private void ColorSettings_Click(object sender, EventArgs e)
        {
            var colorForm = new ColorSettingsForm(_settings);
            if (colorForm.ShowDialog() == DialogResult.OK)
            {
                _settings.Save();
                RefreshStockData();
            }
        }

        private void FontSettings_Click(object sender, EventArgs e)
        {
            var fontForm = new SimpleFontSettingsForm(_settings);
            if (fontForm.ShowDialog() == DialogResult.OK)
            {
                _settings.Save();
                LoadSettings();
                RefreshStockData();
            }
        }

        private void DisplaySettings_Click(object sender, EventArgs e)
        {
            var displayForm = new DisplaySettingsForm(_settings);
            if (displayForm.ShowDialog() == DialogResult.OK)
            {
                _settings.Save();
                _scrollTimer.Interval = _settings.SwitchInterval * 1000;
                RefreshStockData();
            }
        }

        private void Donate_Click(object sender, EventArgs e)
        {
            using (var donateForm = new DonateForm())
            {
                donateForm.ShowDialog();
            }
        }

        private void About_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "桌面看股小工具 v1.1\n\n" +
                "操作说明：\n" +
                "• 拖动窗口可调整位置\n" +
                "• 左键点击切换股票\n" +
                "• Ctrl+左右箭头切换股票\n" +
                "• 右键托盘图标进入设置\n" +
                "• Ctrl+Alt+S 快速显隐\n\n" +
                "开发者：CJ",
                "关于",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void ShowHide_Click(object sender, EventArgs e)
        {
            Visible = !Visible;
        }

        private void AutoStartSettings_Click(object sender, EventArgs e)
        {
            var autoStartForm = new AutoStartSettingsForm(_settings);
            if (autoStartForm.ShowDialog() == DialogResult.OK)
            {
                _settings.Save();
            }
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            Visible = !Visible;
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;

            if (m.Msg == WM_HOTKEY)
            {
                int hotkeyId = m.WParam.ToInt32();
                if (hotkeyId == HOTKEY_ID)
                {
                    Visible = !Visible;
                }
                else if (hotkeyId == HOTKEY_PREV)
                {
                    SwitchToPrevStock();
                }
                else if (hotkeyId == HOTKEY_NEXT)
                {
                    SwitchToNextStock();
                }
            }

            base.WndProc(ref m);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnregisterHotKey(Handle, HOTKEY_ID);
                UnregisterHotKey(Handle, HOTKEY_PREV);
                UnregisterHotKey(Handle, HOTKEY_NEXT);
                _refreshTimer?.Dispose();
                _scrollTimer?.Dispose();
                _notifyIcon?.Dispose();
                _contextMenu?.Dispose();
                _stockService?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
