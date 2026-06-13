using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StockViewer
{
    public partial class StockSettingsForm : Form
    {
        private sealed class StockListItem
        {
            public string Code { get; set; }
            public string Name { get; set; }
        }

        private readonly AppSettings _settings;
        private readonly StockDataService _stockDataService = new StockDataService();
        private ListView _stockListView;
        private TextBox _stockCodeTextBox;
        private Button _addButton;
        private Button _removeButton;
        private Button _okButton;
        private Button _cancelButton;
        private Label _statusLabel;

        public StockSettingsForm(AppSettings settings)
        {
            _settings = settings;
            InitializeComponent();
            Shown += async (s, e) => await LoadStockCodesAsync();
        }

        private void InitializeComponent()
        {
            Text = "股票设置";
            ClientSize = new Size(560, 380);
            MinimumSize = new Size(520, 360);
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            AutoScaleMode = AutoScaleMode.Dpi;

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                ColumnCount = 2,
                RowCount = 3
            };
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62F));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            Controls.Add(mainPanel);

            var lblStocks = new Label
            {
                Text = "已关注股票",
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 8)
            };
            mainPanel.Controls.Add(lblStocks, 0, 0);

            _stockListView = new ListView
            {
                Dock = DockStyle.Fill,
                FullRowSelect = true,
                GridLines = false,
                HideSelection = false,
                MultiSelect = false,
                View = View.Details
            };
            _stockListView.Columns.Add("代码", 130);
            _stockListView.Columns.Add("名称", 180);
            mainPanel.Controls.Add(_stockListView, 0, 1);

            var sidePanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Margin = new Padding(16, 0, 0, 0)
            };
            sidePanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            sidePanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            sidePanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            sidePanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            sidePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainPanel.Controls.Add(sidePanel, 1, 1);

            var lblAdd = new Label
            {
                Text = "添加股票代码",
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 8)
            };
            sidePanel.Controls.Add(lblAdd, 0, 0);

            _stockCodeTextBox = new TextBox
            {
                Dock = DockStyle.Top,
                Margin = new Padding(0, 0, 0, 8)
            };
            var tooltip = new ToolTip();
            tooltip.SetToolTip(_stockCodeTextBox, "如：000001、600519、sh000001、sz399001");
            sidePanel.Controls.Add(_stockCodeTextBox, 0, 1);

            var buttonRow = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                Margin = new Padding(0, 0, 0, 12)
            };
            sidePanel.Controls.Add(buttonRow, 0, 2);

            _addButton = new Button
            {
                Text = "添加",
                Size = new Size(72, 30)
            };
            _addButton.Click += AddButton_Click;
            buttonRow.Controls.Add(_addButton);

            _removeButton = new Button
            {
                Text = "删除",
                Size = new Size(72, 30)
            };
            _removeButton.Click += RemoveButton_Click;
            buttonRow.Controls.Add(_removeButton);

            var lblHelp = new Label
            {
                AutoSize = true,
                MaximumSize = new Size(180, 0),
                Text = "支持格式：\r\n• 000001（深市）\r\n• 600000（沪市）\r\n• sh000001（上证指数）\r\n• sz399001（深证成指）"
            };
            sidePanel.Controls.Add(lblHelp, 0, 3);

            _statusLabel = new Label
            {
                AutoSize = true,
                MaximumSize = new Size(180, 0),
                ForeColor = Color.DimGray,
                Margin = new Padding(0, 12, 0, 0),
                Text = "打开窗口后会自动补全股票名称。"
            };
            sidePanel.Controls.Add(_statusLabel, 0, 4);

            var footer = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                Margin = new Padding(0, 12, 0, 0)
            };
            mainPanel.SetColumnSpan(footer, 2);
            mainPanel.Controls.Add(footer, 0, 2);

            _cancelButton = new Button
            {
                Text = "取消",
                Size = new Size(80, 32),
                DialogResult = DialogResult.Cancel
            };
            footer.Controls.Add(_cancelButton);

            _okButton = new Button
            {
                Text = "确定",
                Size = new Size(80, 32),
                DialogResult = DialogResult.OK
            };
            _okButton.Click += OkButton_Click;
            footer.Controls.Add(_okButton);

            AcceptButton = _okButton;
            CancelButton = _cancelButton;
        }

        private async Task LoadStockCodesAsync()
        {
            _stockListView.Items.Clear();
            _statusLabel.Text = "正在加载股票名称...";

            foreach (var code in _settings.StockCodes)
            {
                await AddStockItemAsync(code);
            }

            _statusLabel.Text = _stockListView.Items.Count == 0
                ? "至少保留一只股票用于桌面显示。"
                : "这里会显示代码对应的股票名称。";
        }

        private async void AddButton_Click(object sender, EventArgs e)
        {
            string stockCode = _stockCodeTextBox.Text.Trim();
            if (string.IsNullOrEmpty(stockCode))
            {
                MessageBox.Show("请输入股票代码", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string normalizedCode = _stockDataService.NormalizeStockCode(stockCode);
            if (FindItemByCode(normalizedCode) != null)
            {
                MessageBox.Show("股票代码已存在", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            await AddStockItemAsync(stockCode);
            _stockCodeTextBox.Clear();
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            if (_stockListView.SelectedItems.Count > 0)
            {
                _stockListView.Items.Remove(_stockListView.SelectedItems[0]);
            }
            else
            {
                MessageBox.Show("请选择要删除的股票代码", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            if (_stockListView.Items.Count == 0)
            {
                MessageBox.Show("至少需要添加一个股票代码", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _settings.StockCodes.Clear();
            foreach (ListViewItem item in _stockListView.Items)
            {
                if (item.Tag is StockListItem stockItem)
                {
                    _settings.StockCodes.Add(stockItem.Code);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _stockDataService?.Dispose();
            }

            base.Dispose(disposing);
        }

        private async Task AddStockItemAsync(string stockCode)
        {
            string normalizedCode = _stockDataService.NormalizeStockCode(stockCode);
            string stockName = "加载中...";

            var listItem = new ListViewItem(new[] { normalizedCode, stockName })
            {
                Tag = new StockListItem
                {
                    Code = normalizedCode,
                    Name = stockName
                }
            };

            _stockListView.Items.Add(listItem);

            var stockData = await _stockDataService.GetStockDataAsync(normalizedCode);
            string resolvedName = string.IsNullOrWhiteSpace(stockData?.Name) ? "未识别名称" : stockData.Name;

            listItem.SubItems[1].Text = resolvedName;
            ((StockListItem)listItem.Tag).Name = resolvedName;
        }

        private ListViewItem FindItemByCode(string normalizedCode)
        {
            foreach (ListViewItem item in _stockListView.Items)
            {
                if (item.Tag is StockListItem stockItem &&
                    string.Equals(stockItem.Code, normalizedCode, StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }
            }

            return null;
        }
    }
}
