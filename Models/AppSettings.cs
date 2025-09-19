using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace StockViewer
{
    public enum ColorMode
    {
        固定颜色,
        红涨绿跌,
        绿涨红跌
    }

    public class AppSettings
    {
        private static readonly string SettingsFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "StockViewer", "settings.json");

        public List<string> StockCodes { get; set; } = new List<string>();
        public int SwitchInterval { get; set; } = 3; // 多股票切换间隔（秒）
        public bool ShowStockName { get; set; } = true;
        public ColorMode ColorMode { get; set; } = ColorMode.红涨绿跌;
        public string FontFamily { get; set; } = "微软雅黑";
        public float FontSize { get; set; } = 8F;
        public FontStyle FontStyle { get; set; } = FontStyle.Bold;
        public Color FontColor { get; set; } = Color.White;
        public int Transparency { get; set; } = 255; // 0-255
        public Point WindowLocation { get; set; } = Point.Empty;
        public bool AutoStart { get; set; } = false;

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsFile))
                {
                    string json = File.ReadAllText(SettingsFile);
                    var settings = JsonConvert.DeserializeObject<AppSettings>(json);
                    return settings ?? CreateDefaultSettings();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载设置失败: {ex.Message}", "警告",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return CreateDefaultSettings();
        }

        private static AppSettings CreateDefaultSettings()
        {
            var settings = new AppSettings();
            // 只在第一次创建时添加默认股票代码
            if (settings.StockCodes.Count == 0)
            {
                settings.StockCodes.Add("sh000001");
            }
            return settings;
        }

        public void Save()
        {
            try
            {
                string directory = Path.GetDirectoryName(SettingsFile);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(SettingsFile, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存设置失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}