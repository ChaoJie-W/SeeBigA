using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace StockViewer
{
    public static class ResourceManager
    {
        private static readonly Assembly CurrentAssembly = Assembly.GetExecutingAssembly();
        private static readonly string ResourcesPath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "Resources");

        /// <summary>
        /// 从嵌入资源中获取Stream
        /// </summary>
        /// <param name="resourceName">资源名称</param>
        /// <returns></returns>
        private static Stream GetEmbeddedResourceStream(string resourceName)
        {
            string fullResourceName = $"StockViewer.Resources.{resourceName}";
            return CurrentAssembly.GetManifestResourceStream(fullResourceName);
        }

        /// <summary>
        /// 获取应用程序图标
        /// </summary>
        /// <returns></returns>
        public static Icon GetApplicationIcon()
        {
            try
            {
                // 优先从嵌入资源加载
                using (var stream = GetEmbeddedResourceStream("app_icon.ico"))
                {
                    if (stream != null)
                    {
                        return new Icon(stream);
                    }
                }
                
                // 如果嵌入资源不存在，尝试从文件加载
                string iconPath = Path.Combine(ResourcesPath, "app_icon.ico");
                if (File.Exists(iconPath))
                {
                    return new Icon(iconPath);
                }
                
                // 都不存在时返回系统默认图标
                return SystemIcons.Application;
            }
            catch
            {
                return SystemIcons.Application;
            }
        }

        /// <summary>
        /// 获取二维码图片
        /// </summary>
        /// <returns></returns>
        public static Image GetQRCodeImage()
        {
            try
            {
                // 优先从嵌入资源加载
                using (var stream = GetEmbeddedResourceStream("qrcode.png"))
                {
                    if (stream != null)
                    {
                        return Image.FromStream(stream);
                    }
                }
                
                // 如果嵌入资源不存在，尝试从文件加载
                string qrCodePath = Path.Combine(ResourcesPath, "qrcode.png");
                if (File.Exists(qrCodePath))
                {
                    return Image.FromFile(qrCodePath);
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 保存二维码图片
        /// </summary>
        /// <param name="image">图片</param>
        /// <returns>是否保存成功</returns>
        public static bool SaveQRCodeImage(Image image)
        {
            try
            {
                string qrCodePath = Path.Combine(ResourcesPath, "qrcode.png");
                image.Save(qrCodePath, System.Drawing.Imaging.ImageFormat.Png);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存二维码图片失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// 保存应用图标
        /// </summary>
        /// <param name="icon">图标</param>
        /// <returns>是否保存成功</returns>
        public static bool SaveApplicationIcon(Icon icon)
        {
            try
            {
                string iconPath = Path.Combine(ResourcesPath, "app_icon.ico");
                using (var stream = new FileStream(iconPath, FileMode.Create))
                {
                    icon.Save(stream);
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存应用图标失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// 从文件保存图标
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>是否保存成功</returns>
        public static bool SaveApplicationIconFromFile(string filePath)
        {
            try
            {
                string iconPath = Path.Combine(ResourcesPath, "app_icon.ico");
                
                // 如果源文件是ico格式，直接复制
                if (Path.GetExtension(filePath).ToLower() == ".ico")
                {
                    File.Copy(filePath, iconPath, true);
                }
                else
                {
                    // 如果是其他格式，转换为ico
                    using (var image = Image.FromFile(filePath))
                    {
                        using (var bitmap = new Bitmap(image, 32, 32))
                        {
                            var icon = Icon.FromHandle(bitmap.GetHicon());
                            SaveApplicationIcon(icon);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存应用图标失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// 从文件保存二维码
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>是否保存成功</returns>
        public static bool SaveQRCodeImageFromFile(string filePath)
        {
            try
            {
                string qrCodePath = Path.Combine(ResourcesPath, "qrcode.png");
                
                using (var image = Image.FromFile(filePath))
                {
                    image.Save(qrCodePath, System.Drawing.Imaging.ImageFormat.Png);
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存二维码图片失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// 获取资源目录路径
        /// </summary>
        /// <returns></returns>
        public static string GetResourcesPath()
        {
            return ResourcesPath;
        }

        /// <summary>
        /// 获取所有嵌入资源名称（调试用）
        /// </summary>
        /// <returns></returns>
        public static string[] GetEmbeddedResourceNames()
        {
            return CurrentAssembly.GetManifestResourceNames();
        }
    }
}