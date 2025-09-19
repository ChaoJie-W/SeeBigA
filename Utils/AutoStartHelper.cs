using Microsoft.Win32;
using System;
using System.IO;
using System.Windows.Forms;

namespace StockViewer
{
    public static class AutoStartHelper
    {
        private const string REG_KEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string APP_NAME = "StockViewer";

        /// <summary>
        /// 设置开机自启动
        /// </summary>
        /// <param name="enable">是否启用</param>
        /// <returns>是否设置成功</returns>
        public static bool SetAutoStart(bool enable)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(REG_KEY, true))
                {
                    if (key == null)
                    {
                        return false;
                    }

                    if (enable)
                    {
                        string exePath = Application.ExecutablePath;
                        key.SetValue(APP_NAME, $"\"{exePath}\"");
                    }
                    else
                    {
                        key.DeleteValue(APP_NAME, false);
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"设置自启动失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// 检查是否已设置开机自启动
        /// </summary>
        /// <returns>是否已设置</returns>
        public static bool IsAutoStartEnabled()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(REG_KEY, false))
                {
                    if (key == null)
                    {
                        return false;
                    }

                    object value = key.GetValue(APP_NAME);
                    return value != null;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取自启动路径
        /// </summary>
        /// <returns>自启动的exe路径</returns>
        public static string GetAutoStartPath()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(REG_KEY, false))
                {
                    if (key == null)
                    {
                        return string.Empty;
                    }

                    object value = key.GetValue(APP_NAME);
                    return value?.ToString()?.Trim('"') ?? string.Empty;
                }
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}