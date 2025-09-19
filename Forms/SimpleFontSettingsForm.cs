using System;
using System.Drawing;
using System.Windows.Forms;

namespace StockViewer
{
    public partial class SimpleFontSettingsForm : Form
    {
        private AppSettings _settings;
        private ComboBox _fontSizeComboBox;
        private Button _colorButton;
        private Panel _colorPanel;
        private Button _okButton;
        private Button _cancelButton;

        public SimpleFontSettingsForm(AppSettings settings)
        {
            _settings = settings;
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            // Form设置
            this.Text = "字体设置";
            this.Size = new Size(420, 350);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            // 字体大小
            var lblSize = new Label();
            lblSize.Text = "字体大小：";
            lblSize.Location = new Point(20, 25);
            lblSize.Size = new Size(70, 20);
            this.Controls.Add(lblSize);

            _fontSizeComboBox = new ComboBox();
            _fontSizeComboBox.Location = new Point(100, 23);
            _fontSizeComboBox.Size = new Size(80, 25);
            _fontSizeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _fontSizeComboBox.Items.AddRange(new object[] { "6", "8", "10", "12", "14", "16", "18", "20", "24", "28" });
            this.Controls.Add(_fontSizeComboBox);

            // 字体颜色
            var lblColor = new Label();
            lblColor.Text = "字体颜色：";
            lblColor.Location = new Point(20, 60);
            lblColor.Size = new Size(70, 20);
            this.Controls.Add(lblColor);

            _colorPanel = new Panel();
            _colorPanel.Location = new Point(100, 58);
            _colorPanel.Size = new Size(30, 24);
            _colorPanel.BorderStyle = BorderStyle.FixedSingle;
            _colorPanel.BackColor = Color.White;
            this.Controls.Add(_colorPanel);

            _colorButton = new Button();
            _colorButton.Text = "选择";
            _colorButton.Location = new Point(140, 58);
            _colorButton.Size = new Size(60, 24);
            _colorButton.Click += ColorButton_Click;
            this.Controls.Add(_colorButton);

            // 说明
            var lblNote = new Label();
            lblNote.Text = "字体：微软雅黑（固定）\n样式：半粗体（优化显示效果）";
            lblNote.Location = new Point(20, 95);
            lblNote.Size = new Size(200, 35);
            lblNote.ForeColor = Color.Gray;
            this.Controls.Add(lblNote);

            // 按钮 - 位置调整到窗口底部并预留足够空间
            _okButton = new Button();
            _okButton.Text = "确定";
            _okButton.Location = new Point(215, 280);
            _okButton.Size = new Size(65, 30);
            _okButton.DialogResult = DialogResult.OK;
            _okButton.Click += OkButton_Click;
            this.Controls.Add(_okButton);

            _cancelButton = new Button();
            _cancelButton.Text = "取消";
            _cancelButton.Location = new Point(300, 280);
            _cancelButton.Size = new Size(65, 30);
            _cancelButton.DialogResult = DialogResult.Cancel;
            this.Controls.Add(_cancelButton);

            this.AcceptButton = _okButton;
            this.CancelButton = _cancelButton;
        }

        private void LoadSettings()
        {
            _fontSizeComboBox.Text = _settings.FontSize.ToString();
            _colorPanel.BackColor = _settings.FontColor;
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
            if (float.TryParse(_fontSizeComboBox.Text, out float fontSize))
            {
                _settings.FontFamily = "微软雅黑";
                _settings.FontSize = fontSize;
                _settings.FontStyle = FontStyle.Bold; // 使用Bold来模拟半粗体效果
                _settings.FontColor = _colorPanel.BackColor;
            }
            else
            {
                MessageBox.Show("请选择有效的字体大小", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }
    }
}