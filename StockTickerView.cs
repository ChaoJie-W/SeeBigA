using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace StockViewer
{
    public class StockTickerView
    {
        private const int HorizontalPadding = 8;
        private const int VerticalPadding = 4;
        private string _displayText = "加载中...";
        private Color _textColor = Color.White;

        public StockTickerView()
        {
            Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold, GraphicsUnit.Point);
        }

        public string DisplayText
        {
            get => _displayText;
            set
            {
                _displayText = string.IsNullOrWhiteSpace(value) ? " " : value;
            }
        }

        public Color TextColor
        {
            get => _textColor;
            set
            {
                _textColor = value;
            }
        }

        public Font Font { get; set; }

        public Size MeasureDisplay()
        {
            using (var bitmap = new Bitmap(1, 1))
            using (var g = Graphics.FromImage(bitmap))
            {
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                var size = g.MeasureString(_displayText, Font, PointF.Empty, StringFormat.GenericTypographic);
                return new Size(
                    (int)Math.Ceiling(size.Width) + HorizontalPadding * 2,
                    (int)Math.Ceiling(size.Height) + VerticalPadding * 2);
            }
        }

        public void Draw(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            using (var shadowBrush = new SolidBrush(Color.FromArgb(60, 0, 0, 0)))
            using (var textBrush = new SolidBrush(_textColor))
            {
                var textPoint = new PointF(HorizontalPadding, VerticalPadding);
                var shadowPoint = new PointF(textPoint.X, textPoint.Y + 1);

                g.DrawString(_displayText, Font, shadowBrush, shadowPoint, StringFormat.GenericTypographic);
                g.DrawString(_displayText, Font, textBrush, textPoint, StringFormat.GenericTypographic);
            }
        }
    }
}
