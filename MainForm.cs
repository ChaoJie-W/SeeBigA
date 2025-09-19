using System;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StockViewer
{
    public partial class MainForm : Form
    {
        // Win32 API 声明
        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_LAYERED = 0x80000;
        private const int WS_EX_TRANSPARENT = 0x20;
        private const int LWA_ALPHA = 0x2;
        private const int LWA_COLORKEY = 0x1;
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
        private int _currentStockIndex = 0;

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
            
            // MainForm
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

        private void InitializeUI()
        {
            // 设置窗口透明和高质量文字渲染
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            
            // 创建股票显示标签
            _stockLabel = new Label
            {
                AutoSize = true,
                BackColor = Color.Transparent,
                Font = new Font("微软雅黑", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(5, 5),
                Text = "加载中...",
                UseMnemonic = false
            };

            // 设置高质量文字渲染
            _stockLabel.UseCompatibleTextRendering = false;

            Controls.Add(_stockLabel);

            // 调整窗口大小
            AdjustWindowSize();

            // 绑定事件
            _stockLabel.MouseDown += Label_MouseDown;
            _stockLabel.MouseMove += Label_MouseMove;
            _stockLabel.MouseUp += Label_MouseUp;
            _stockLabel.MouseClick += Label_MouseClick;
            // 取消滚轮事件
            // _stockLabel.MouseWheel += Label_MouseWheel;

            MouseDown += MainForm_MouseDown;
            MouseMove += MainForm_MouseMove;
            MouseUp += MainForm_MouseUp;
            MouseClick += MainForm_MouseClick;
            // 取消滚轮事件
            // MouseWheel += MainForm_MouseWheel;
        }

        private void InitializeServices()
        {
            _stockService = new StockDataService();
            
            // 初始化刷新定时器（固定3秒）
            _refreshTimer = new Timer();
            _refreshTimer.Interval = 3000; // 固定3秒拉取数据
            _refreshTimer.Tick += RefreshTimer_Tick;
            _refreshTimer.Start();

            // 初始化滚动定时器（使用设置的切换间隔）
            _scrollTimer = new Timer();
            _scrollTimer.Interval = _settings.SwitchInterval * 1000;
            _scrollTimer.Tick += ScrollTimer_Tick;
            if (_settings.StockCodes.Count > 1)
            {
                _scrollTimer.Start();
            }

            // 立即刷新一次
            RefreshStockData();
        }

        private void InitializeSettings()
        {
            _settings = AppSettings.Load();
        }

        private void InitializeSystemTray()
        {
            // 创建托盘图标
            _notifyIcon = new NotifyIcon();
            _notifyIcon.Icon = ResourceManager.GetApplicationIcon();
            _notifyIcon.Text = "桌面看股小工具";
            _notifyIcon.Visible = true;

            // 创建托盘菜单 - 包含所有设置功能
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
            // 注册全局快捷键 Ctrl+Alt+S
            RegisterHotKey(this.Handle, HOTKEY_ID, MOD_CONTROL | MOD_ALT, VK_S);
            // 注册全局快捷键 Ctrl+左箭头
            RegisterHotKey(this.Handle, HOTKEY_PREV, MOD_CONTROL, VK_LEFT);
            // 注册全局快捷键 Ctrl+右箭头
            RegisterHotKey(this.Handle, HOTKEY_NEXT, MOD_CONTROL, VK_RIGHT);
        }

        private void LoadSettings()
        {
            // 应用设置
            if (_settings.WindowLocation != Point.Empty)
            {
                this.Location = _settings.WindowLocation;
            }

            _stockLabel.Font = new Font(_settings.FontFamily, _settings.FontSize, _settings.FontStyle);
            _stockLabel.ForeColor = _settings.FontColor;
            // 数据拉取频率固定3秒，不再从设置加载
            // _refreshTimer.Interval 已在 InitializeServices 中设置为 3000

            AdjustWindowSize();
        }

        private void AdjustWindowSize()
        {
            // 根据文本大小调整窗口尺寸
            Size textSize = TextRenderer.MeasureText(_stockLabel.Text, _stockLabel.Font);
            this.Size = new Size(textSize.Width + 20, textSize.Height + 10);
            _stockLabel.Location = new Point(10, 5);
        }

        private async void RefreshStockData()
        {
            if (_settings.StockCodes.Count == 0)
            {
                _stockLabel.Text = "请设置股票代码";
                _stockLabel.ForeColor = Color.Yellow;
                AdjustWindowSize();
                return;
            }

            try
            {
                string stockCode = _settings.StockCodes[_currentStockIndex];
                var stockData = await _stockService.GetStockDataAsync(stockCode);

                if (stockData != null)
                {
                    UpdateStockDisplay(stockData);
                }
                else
                {
                    _stockLabel.Text = "数据获取失败";
                    _stockLabel.ForeColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                _stockLabel.Text = $"错误: {ex.Message}";
                _stockLabel.ForeColor = Color.Red;
            }

            AdjustWindowSize();
        }

        private void UpdateStockDisplay(StockData stockData)
        {
            string displayText;
            
            if (_settings.ShowStockName)
            {
                displayText = $"{stockData.Code} {stockData.Name} {stockData.Price:F2} {stockData.ChangePercent:+0.00;-0.00}%";
            }
            else
            {
                displayText = $"{stockData.Code} {stockData.Price:F2} {stockData.ChangePercent:+0.00;-0.00}%";
            }

            _stockLabel.Text = displayText;

            // 根据涨跌设置颜色 - 修复逻辑
            Color textColor;
            
            switch (_settings.ColorMode)
            {
                case ColorMode.红涨绿跌:
                    textColor = stockData.Change >= 0 ? Color.Red : Color.Green;
                    break;
                case ColorMode.绿涨红跌:
                    textColor = stockData.Change >= 0 ? Color.Green : Color.Red;
                    break;
                case ColorMode.固定颜色:
                default:
                    textColor = _settings.FontColor;
                    break;
            }

            _stockLabel.ForeColor = textColor;
        }

        #region 拖动功能
        private void Label_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = true;
                _lastLocation = e.Location;
            }
        }

        private void Label_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point newLocation = new Point(
                    (this.Location.X - _lastLocation.X) + e.X,
                    (this.Location.Y - _lastLocation.Y) + e.Y);
                
                // 防止窗口被拖到屏幕外或任务栏下面
                Rectangle workingArea = Screen.GetWorkingArea(this);
                
                // 限制窗口位置在可见区域内
                if (newLocation.X < 0) newLocation.X = 0;
                if (newLocation.Y < 0) newLocation.Y = 0;
                if (newLocation.X + this.Width > workingArea.Right)
                    newLocation.X = workingArea.Right - this.Width;
                if (newLocation.Y + this.Height > workingArea.Bottom)
                    newLocation.Y = workingArea.Bottom - this.Height;
                
                this.Location = newLocation;
            }
        }

        private void Label_MouseUp(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                _settings.WindowLocation = this.Location;
                _settings.Save();
            }
        }

        private void Label_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ShowContextMenu(e.Location);
            }
            else if (e.Button == MouseButtons.Left)
            {
                // 左键点击切换股票
                SwitchToNextStock();
            }
        }

        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            Label_MouseDown(sender, e);
        }

        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            Label_MouseMove(sender, e);
        }

        private void MainForm_MouseUp(object sender, MouseEventArgs e)
        {
            Label_MouseUp(sender, e);
        }

        private void MainForm_MouseClick(object sender, MouseEventArgs e)
        {
            Label_MouseClick(sender, e);
        }

        private void SwitchToNextStock()
        {
            if (_settings.StockCodes.Count <= 1) return;

            // 停止自动滚动
            _scrollTimer.Stop();

            // 切换到下一只股票
            _currentStockIndex = (_currentStockIndex + 1) % _settings.StockCodes.Count;

            // 立即刷新显示
            RefreshStockData();

            // 重新启动自动滚动定时器
            if (_settings.StockCodes.Count > 1)
            {
                _scrollTimer.Start();
            }
        }

        private void SwitchToPrevStock()
        {
            if (_settings.StockCodes.Count <= 1) return;

            // 停止自动滚动
            _scrollTimer.Stop();

            // 切换到上一只股票
            _currentStockIndex = (_currentStockIndex - 1 + _settings.StockCodes.Count) % _settings.StockCodes.Count;

            // 立即刷新显示
            RefreshStockData();

            // 重新启动自动滚动定时器
            if (_settings.StockCodes.Count > 1)
            {
                _scrollTimer.Start();
            }
        }

        // 已移除滚轮切换功能，改为点击切换
        #endregion

        #region 定时器事件
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
        #endregion

        #region 右键菜单
        private void ShowContextMenu(Point location)
        {
            // 简化右键菜单，只保留基本功能
            var menu = new ContextMenuStrip();
            
            menu.Items.Add("隐藏窗口", null, (s, e) => this.Visible = false);
            menu.Items.Add("退出", null, Exit_Click);

            Point screenPoint = _stockLabel.PointToScreen(location);
            menu.Show(screenPoint);
        }
        #endregion

        #region 事件处理
        private void StockSettings_Click(object sender, EventArgs e)
        {
            var settingsForm = new StockSettingsForm(_settings);
            if (settingsForm.ShowDialog() == DialogResult.OK)
            {
                _settings.Save();
                
                // 重新启动滚动定时器
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
                _scrollTimer.Interval = _settings.SwitchInterval * 1000; // 更新切换间隔
                RefreshStockData();
            }
        }

        private void Donate_Click(object sender, EventArgs e)
        {
            var donateForm = new DonateForm();
            donateForm.ShowDialog();
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
            this.Visible = !this.Visible;
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            StockSettings_Click(sender, e);
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
            this.Visible = !this.Visible;
        }
        #endregion

        #region 窗口消息处理
        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;
            
            if (m.Msg == WM_HOTKEY)
            {
                int hotkeyId = m.WParam.ToInt32();
                if (hotkeyId == HOTKEY_ID)
                {
                    this.Visible = !this.Visible;
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

        protected override void SetVisibleCore(bool value)
        {
            base.SetVisibleCore(value);
        }
        #endregion

        #region 清理资源
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnregisterHotKey(this.Handle, HOTKEY_ID);
                UnregisterHotKey(this.Handle, HOTKEY_PREV);
                UnregisterHotKey(this.Handle, HOTKEY_NEXT);
                _refreshTimer?.Dispose();
                _scrollTimer?.Dispose();
                _notifyIcon?.Dispose();
                _contextMenu?.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}