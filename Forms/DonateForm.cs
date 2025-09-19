using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace StockViewer
{
    public partial class DonateForm : Form
    {
        private PictureBox _qrCodePictureBox;
        private Label _titleLabel;
        private Label _descriptionLabel;
        private Button _closeButton;

        public DonateForm()
        {
            InitializeComponent();
            LoadQRCode();
        }

        private void InitializeComponent()
        {
            // Form设置
            this.Text = "请我喝咖啡";
            this.Size = new Size(350, 450);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            // 标题
            _titleLabel = new Label();
            _titleLabel.Text = "感谢您的支持！";
            _titleLabel.Font = new Font("微软雅黑", 14F, FontStyle.Bold);
            _titleLabel.ForeColor = Color.FromArgb(51, 51, 51);
            _titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            _titleLabel.Location = new Point(20, 20);
            _titleLabel.Size = new Size(290, 30);
            this.Controls.Add(_titleLabel);

            // 描述
            _descriptionLabel = new Label();
            _descriptionLabel.Text = "如果这个小工具对您有帮助，\n欢迎扫码请我喝杯咖啡☕";
            _descriptionLabel.Font = new Font("微软雅黑", 10F);
            _descriptionLabel.ForeColor = Color.FromArgb(102, 102, 102);
            _descriptionLabel.TextAlign = ContentAlignment.MiddleCenter;
            _descriptionLabel.Location = new Point(20, 60);
            _descriptionLabel.Size = new Size(290, 40);
            this.Controls.Add(_descriptionLabel);

            // 二维码
            _qrCodePictureBox = new PictureBox();
            _qrCodePictureBox.Location = new Point(75, 120);
            _qrCodePictureBox.Size = new Size(200, 200);
            _qrCodePictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            _qrCodePictureBox.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(_qrCodePictureBox);

            // 感谢文字
            var thanksLabel = new Label();
            thanksLabel.Text = "您的支持是我持续改进的动力！";
            thanksLabel.Font = new Font("微软雅黑", 9F);
            thanksLabel.ForeColor = Color.FromArgb(102, 102, 102);
            thanksLabel.TextAlign = ContentAlignment.MiddleCenter;
            thanksLabel.Location = new Point(20, 340);
            thanksLabel.Size = new Size(290, 20);
            this.Controls.Add(thanksLabel);

            // 关闭按钮
            _closeButton = new Button();
            _closeButton.Text = "关闭";
            _closeButton.Location = new Point(137, 375);
            _closeButton.Size = new Size(75, 30);
            _closeButton.DialogResult = DialogResult.OK;
            this.Controls.Add(_closeButton);

            this.AcceptButton = _closeButton;
        }

        private void LoadQRCode()
        {
            try
            {
                var qrImage = ResourceManager.GetQRCodeImage();
                
                if (qrImage != null)
                {
                    _qrCodePictureBox.Image = qrImage;
                }
                else
                {
                    // 如果没有找到图片，显示默认文字
                    _qrCodePictureBox.Hide();
                    
                    var noImageLabel = new Label();
                    noImageLabel.Text = "二维码图片未找到\n请将二维码图片放置在\nResources/qrcode.png";
                    noImageLabel.Font = new Font("微软雅黑", 10F);
                    noImageLabel.ForeColor = Color.Gray;
                    noImageLabel.TextAlign = ContentAlignment.MiddleCenter;
                    noImageLabel.Location = new Point(75, 150);
                    noImageLabel.Size = new Size(200, 100);
                    this.Controls.Add(noImageLabel);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载二维码失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}