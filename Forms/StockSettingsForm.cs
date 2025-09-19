using System;
using System.Linq;
using System.Windows.Forms;

namespace StockViewer
{
    public partial class StockSettingsForm : Form
    {
        private AppSettings _settings;
        private ListBox _stockListBox;
        private TextBox _stockCodeTextBox;
        private Button _addButton;
        private Button _removeButton;
        private Button _okButton;
        private Button _cancelButton;

        public StockSettingsForm(AppSettings settings)
        {
            _settings = settings;
            InitializeComponent();
            LoadStockCodes();
        }

        private void InitializeComponent()
        {
            // Form设置
            this.Text = "股票设置";
            this.Size = new System.Drawing.Size(420, 350);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            // 股票列表
            var lblStocks = new Label();
            lblStocks.Text = "股票代码列表：";
            lblStocks.Location = new System.Drawing.Point(12, 15);
            lblStocks.Size = new System.Drawing.Size(100, 20);
            this.Controls.Add(lblStocks);

            _stockListBox = new ListBox();
            _stockListBox.Location = new System.Drawing.Point(12, 40);
            _stockListBox.Size = new System.Drawing.Size(200, 180);
            this.Controls.Add(_stockListBox);

            // 添加股票代码
            var lblAdd = new Label();
            lblAdd.Text = "添加股票代码：";
            lblAdd.Location = new System.Drawing.Point(230, 40);
            lblAdd.Size = new System.Drawing.Size(100, 20);
            this.Controls.Add(lblAdd);

            _stockCodeTextBox = new TextBox();
            _stockCodeTextBox.Location = new System.Drawing.Point(230, 65);
            _stockCodeTextBox.Size = new System.Drawing.Size(140, 25);
            // 无法使用PlaceholderText，使用Tooltip代替
            var tooltip = new ToolTip();
            tooltip.SetToolTip(_stockCodeTextBox, "如：000001 或 sh000001");
            this.Controls.Add(_stockCodeTextBox);

            _addButton = new Button();
            _addButton.Text = "添加";
            _addButton.Location = new System.Drawing.Point(230, 95);
            _addButton.Size = new System.Drawing.Size(65, 30);
            _addButton.Click += AddButton_Click;
            this.Controls.Add(_addButton);

            _removeButton = new Button();
            _removeButton.Text = "删除";
            _removeButton.Location = new System.Drawing.Point(305, 95);
            _removeButton.Size = new System.Drawing.Size(65, 30);
            _removeButton.Click += RemoveButton_Click;
            this.Controls.Add(_removeButton);

            // 说明文字
            var lblHelp = new Label();
            lblHelp.Text = "支持格式：\n• 000001 (深圳)\n• 600000 (上海)\n• sh000001 (上海指数)\n• sz399001 (深圳指数)";
            lblHelp.Location = new System.Drawing.Point(230, 140);
            lblHelp.Size = new System.Drawing.Size(160, 80);
            this.Controls.Add(lblHelp);

            // 按钮 - 位置调整到窗口底部并留出足够空间
            _okButton = new Button();
            _okButton.Text = "确定";
            _okButton.Location = new System.Drawing.Point(215, 280);
            _okButton.Size = new System.Drawing.Size(75, 30);
            _okButton.DialogResult = DialogResult.OK;
            _okButton.Click += OkButton_Click;
            this.Controls.Add(_okButton);

            _cancelButton = new Button();
            _cancelButton.Text = "取消";
            _cancelButton.Location = new System.Drawing.Point(300, 280);
            _cancelButton.Size = new System.Drawing.Size(75, 30);
            _cancelButton.DialogResult = DialogResult.Cancel;
            this.Controls.Add(_cancelButton);

            this.AcceptButton = _okButton;
            this.CancelButton = _cancelButton;
        }

        private void LoadStockCodes()
        {
            _stockListBox.Items.Clear();
            foreach (var code in _settings.StockCodes)
            {
                _stockListBox.Items.Add(code);
            }
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            string stockCode = _stockCodeTextBox.Text.Trim();
            if (string.IsNullOrEmpty(stockCode))
            {
                MessageBox.Show("请输入股票代码", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_stockListBox.Items.Contains(stockCode))
            {
                MessageBox.Show("股票代码已存在", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _stockListBox.Items.Add(stockCode);
            _stockCodeTextBox.Clear();
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            if (_stockListBox.SelectedItem != null)
            {
                _stockListBox.Items.Remove(_stockListBox.SelectedItem);
            }
            else
            {
                MessageBox.Show("请选择要删除的股票代码", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            if (_stockListBox.Items.Count == 0)
            {
                MessageBox.Show("至少需要添加一个股票代码", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _settings.StockCodes.Clear();
            foreach (string item in _stockListBox.Items)
            {
                _settings.StockCodes.Add(item);
            }
        }
    }
}