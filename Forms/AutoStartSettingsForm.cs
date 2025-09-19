using System;
using System.Drawing;
using System.Windows.Forms;

namespace StockViewer
{
    public partial class AutoStartSettingsForm : Form
    {
        private AppSettings _settings;
        private CheckBox _autoStartCheckBox;
        private Label _statusLabel;
        private Button _okButton;
        private Button _cancelButton;

        public AutoStartSettingsForm(AppSettings settings)
        {
            _settings = settings;
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            // Form设置
            this.Text = "启动设置";
            this.Size = new Size(420, 350);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            // 自启动设置
            _autoStartCheckBox = new CheckBox();
            _autoStartCheckBox.Text = "开机自动启动";
            _autoStartCheckBox.Location = new Point(20, 20);
            _autoStartCheckBox.Size = new Size(120, 24);
            _autoStartCheckBox.CheckedChanged += AutoStartCheckBox_CheckedChanged;
            this.Controls.Add(_autoStartCheckBox);

            // 状态标签
            _statusLabel = new Label();
            _statusLabel.Location = new Point(20, 50);
            _statusLabel.Size = new Size(300, 30);
            _statusLabel.ForeColor = Color.Gray;
            this.Controls.Add(_statusLabel);

            // 说明
            var helpLabel = new Label();
            helpLabel.Text = "注意：启用自启动将在Windows注册表中添加启动项";
            helpLabel.Location = new Point(20, 85);
            helpLabel.Size = new Size(300, 20);
            helpLabel.ForeColor = Color.Gray;
            this.Controls.Add(helpLabel);

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
            bool isAutoStartEnabled = AutoStartHelper.IsAutoStartEnabled();
            _autoStartCheckBox.Checked = isAutoStartEnabled;
            _settings.AutoStart = isAutoStartEnabled;
            
            UpdateStatusLabel();
        }

        private void AutoStartCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateStatusLabel();
        }

        private void UpdateStatusLabel()
        {
            if (_autoStartCheckBox.Checked)
            {
                _statusLabel.Text = "程序将在Windows启动时自动运行";
                _statusLabel.ForeColor = Color.Green;
            }
            else
            {
                _statusLabel.Text = "程序不会自动启动";
                _statusLabel.ForeColor = Color.Gray;
            }
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            bool success = AutoStartHelper.SetAutoStart(_autoStartCheckBox.Checked);
            
            if (success)
            {
                _settings.AutoStart = _autoStartCheckBox.Checked;
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("设置自启动失败，请检查权限或稍后重试", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.None;
            }
        }
    }
}