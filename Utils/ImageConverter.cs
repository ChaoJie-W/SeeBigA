using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace StockViewer
{
    public static class ImageConverter
    {
        /// <summary>
        /// 将图片转换为ICO格式
        /// </summary>
        /// <param name="sourcePath">源图片路径</param>
        /// <param name="targetPath">目标ICO路径</param>
        /// <param name="size">图标尺寸</param>
        /// <returns>转换是否成功</returns>
        public static bool ConvertToIco(string sourcePath, string targetPath, int size = 32)
        {
            try
            {
                using (var originalImage = Image.FromFile(sourcePath))
                {
                    using (var resizedImage = new Bitmap(originalImage, size, size))
                    {
                        IntPtr hIcon = resizedImage.GetHicon();
                        using (var icon = Icon.FromHandle(hIcon))
                        {
                            using (var fileStream = new FileStream(targetPath, FileMode.Create))
                            {
                                icon.Save(fileStream);
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"转换ICO失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 调整图片尺寸
        /// </summary>
        /// <param name="sourcePath">源图片路径</param>
        /// <param name="targetPath">目标图片路径</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="format">图片格式</param>
        /// <returns>是否成功</returns>
        public static bool ResizeImage(string sourcePath, string targetPath, int width, int height, ImageFormat format = null)
        {
            try
            {
                if (format == null)
                    format = ImageFormat.Png;

                using (var originalImage = Image.FromFile(sourcePath))
                {
                    using (var resizedImage = new Bitmap(originalImage, width, height))
                    {
                        resizedImage.Save(targetPath, format);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"调整图片尺寸失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 检查图片文件是否有效
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>是否有效</returns>
        public static bool IsValidImage(string filePath)
        {
            try
            {
                using (var image = Image.FromFile(filePath))
                {
                    return image.Width > 0 && image.Height > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取图片尺寸信息
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>尺寸信息</returns>
        public static Size GetImageSize(string filePath)
        {
            try
            {
                using (var image = Image.FromFile(filePath))
                {
                    return new Size(image.Width, image.Height);
                }
            }
            catch
            {
                return Size.Empty;
            }
        }
    }
}