using System;
using System.Drawing;
using System.Windows.Forms;

namespace StockViewer
{
    public partial class ColorSettingsForm : Form
    {
        private AppSettings _settings;
        private RadioButton _fixedColorRadio;
        private RadioButton _redUpGreenDownRadio;
        private RadioButton _greenUpRedDownRadio;
        private Button _colorButton;
        private Panel _colorPanel;
        private Button _okButton;
        private Button _cancelButton;

        public ColorSettingsForm(AppSettings settings)
        {
            _settings = settings;
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            // Form设置
            this.Text = "颜色设置";
            this.Size = new Size(420, 350);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            // 颜色模式组
            var groupBox = new GroupBox();
            groupBox.Text = "颜色模式";
            groupBox.Location = new Point(12, 12);
            groupBox.Size = new Size(310, 120);
            this.Controls.Add(groupBox);

            _fixedColorRadio = new RadioButton();
            _fixedColorRadio.Text = "固定颜色";
            _fixedColorRadio.Location = new Point(15, 25);
            _fixedColorRadio.Size = new Size(80, 20);
            _fixedColorRadio.CheckedChanged += ColorModeRadio_CheckedChanged;
            groupBox.Controls.Add(_fixedColorRadio);

            _redUpGreenDownRadio = new RadioButton();
            _redUpGreenDownRadio.Text = "红涨绿跌";
            _redUpGreenDownRadio.Location = new Point(15, 50);
            _redUpGreenDownRadio.Size = new Size(80, 20);
            _redUpGreenDownRadio.CheckedChanged += ColorModeRadio_CheckedChanged;
            groupBox.Controls.Add(_redUpGreenDownRadio);

            _greenUpRedDownRadio = new RadioButton();
            _greenUpRedDownRadio.Text = "绿涨红跌";
            _greenUpRedDownRadio.Location = new Point(15, 75);
            _greenUpRedDownRadio.Size = new Size(80, 20);
            _greenUpRedDownRadio.CheckedChanged += ColorModeRadio_CheckedChanged;
            groupBox.Controls.Add(_greenUpRedDownRadio);

            // 颜色选择
            var lblColor = new Label();
            lblColor.Text = "固定颜色：";
            lblColor.Location = new Point(120, 25);
            lblColor.Size = new Size(70, 20);
            groupBox.Controls.Add(lblColor);

            _colorPanel = new Panel();
            _colorPanel.Location = new Point(195, 23);
            _colorPanel.Size = new Size(30, 24);
            _colorPanel.BorderStyle = BorderStyle.FixedSingle;
            _colorPanel.BackColor = Color.White;
            groupBox.Controls.Add(_colorPanel);

            _colorButton = new Button();
            _colorButton.Text = "选择";
            _colorButton.Location = new Point(235, 23);
            _colorButton.Size = new Size(60, 24);
            _colorButton.Click += ColorButton_Click;
            groupBox.Controls.Add(_colorButton);

            // 按钮 - 位置调整到窗口底部
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
            switch (_settings.ColorMode)
            {
                case ColorMode.固定颜色:
                    _fixedColorRadio.Checked = true;
                    break;
                case ColorMode.红涨绿跌:
                    _redUpGreenDownRadio.Checked = true;
                    break;
                case ColorMode.绿涨红跌:
                    _greenUpRedDownRadio.Checked = true;
                    break;
            }

            _colorPanel.BackColor = _settings.FontColor;
            UpdateColorControlsEnabled();
        }

        private void ColorModeRadio_CheckedChanged(object sender, EventArgs e)
        {
            UpdateColorControlsEnabled();
        }

        private void UpdateColorControlsEnabled()
        {
            bool enableColorSelection = _fixedColorRadio.Checked;
            _colorButton.Enabled = enableColorSelection;
            _colorPanel.Enabled = enableColorSelection;
        }

        private void ColorButton_Click(object sender, EventArgs e)
        {
            var colorDialog = new ColorDialog();
            colorDialog.Color = _colorPanel.BackColor;
            
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                _colorPanel.BackColor = colorDialog.Color;
            }
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            if (_fixedColorRadio.Checked)
            {
                _settings.ColorMode = ColorMode.固定颜色;
                _settings.FontColor = _colorPanel.BackColor;
            }
            else if (_redUpGreenDownRadio.Checked)
            {
                _settings.ColorMode = ColorMode.红涨绿跌;
            }
            else if (_greenUpRedDownRadio.Checked)
            {
                _settings.ColorMode = ColorMode.绿涨红跌;
            }
        }
    }
}