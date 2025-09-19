using System;
using System.Threading;
using System.Windows.Forms;

namespace StockViewer
{
    internal static class Program
    {
        private static Mutex _mutex;

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            // 防止多实例运行
            bool createdNew;
            _mutex = new Mutex(true, "StockViewer_SingleInstance", out createdNew);
            
            if (!createdNew)
            {
                MessageBox.Show("程序已经在运行中！", "桌面看股小工具", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            try
            {
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"程序启动失败：{ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _mutex?.ReleaseMutex();
                _mutex?.Dispose();
            }
        }
    }
}