using System;
using System.Drawing;
using System.Windows.Forms;

namespace StockViewer
{
    public partial class DisplaySettingsForm : Form
    {
        private AppSettings _settings;
        private CheckBox _showNameCheckBox;
        private NumericUpDown _switchIntervalNumeric;
        private Button _okButton;
        private Button _cancelButton;

        public DisplaySettingsForm(AppSettings settings)
        {
            _settings = settings;
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            // Form设置
            this.Text = "显示设置";
            this.Size = new Size(420, 350);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            // 显示股票名称
            _showNameCheckBox = new CheckBox();
            _showNameCheckBox.Text = "显示股票名称";
            _showNameCheckBox.Location = new Point(20, 20);
            _showNameCheckBox.Size = new Size(120, 24);
            this.Controls.Add(_showNameCheckBox);

            // 切换间隔
            var lblSwitch = new Label();
            lblSwitch.Text = "切换间隔（秒）：";
            lblSwitch.Location = new Point(20, 55);
            lblSwitch.Size = new Size(100, 20);
            this.Controls.Add(lblSwitch);

            _switchIntervalNumeric = new NumericUpDown();
            _switchIntervalNumeric.Location = new Point(130, 53);
            _switchIntervalNumeric.Size = new Size(80, 25);
            _switchIntervalNumeric.Minimum = 1;
            _switchIntervalNumeric.Maximum = 60;
            _switchIntervalNumeric.Value = 3;
            this.Controls.Add(_switchIntervalNumeric);

            // 说明
            var lblHelp = new Label();
            lblHelp.Text = "注意：\n• 数据拉取频率固定3秒\n• 此设置为多股票切换时间\n• 设置会立即生效";
            lblHelp.Location = new Point(20, 85);
            lblHelp.Size = new Size(300, 60);
            this.Controls.Add(lblHelp);

            // 按钮 - 位置调整到窗口底部并预留足够空间
            _okButton = new Button();
            _okButton.Text = "确定";
            _okButton.Location = new Point(215, 280);
            _okButton.Size = new Size(75, 30);
            _okButton.DialogResult = DialogResult.OK;
            _okButton.Click += OkButton_Click;
            this.Controls.Add(_okButton);

            _cancelButton = new Button();
            _cancelButton.Text = "取消";
            _cancelButton.Location = new Point(300, 280);
            _cancelButton.Size = new Size(75, 30);
            _cancelButton.DialogResult = DialogResult.Cancel;
            this.Controls.Add(_cancelButton);

            this.AcceptButton = _okButton;
            this.CancelButton = _cancelButton;
        }

        private void LoadSettings()
        {
            _showNameCheckBox.Checked = _settings.ShowStockName;
            _switchIntervalNumeric.Value = _settings.SwitchInterval;
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            _settings.ShowStockName = _showNameCheckBox.Checked;
            _settings.SwitchInterval = (int)_switchIntervalNumeric.Value;
        }
    }
}