using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace StockViewer
{
    public class StockDataService
    {
        private readonly HttpClient _httpClient;
        private const string TENCENT_API_URL = "http://qt.gtimg.cn/q=";

        public StockDataService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
            
            // 设置请求头
            _httpClient.DefaultRequestHeaders.Add("User-Agent", 
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        }

        public async Task<StockData> GetStockDataAsync(string stockCode)
        {
            try
            {
                // 格式化股票代码（腾讯API格式）
                string formattedCode = FormatStockCode(stockCode);
                string url = $"{TENCENT_API_URL}{formattedCode}";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                
                // 解析腾讯API返回的数据
                return ParseTencentData(content, stockCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取股票数据失败: {ex.Message}");
                return null;
            }
        }

        private string FormatStockCode(string stockCode)
        {
            // 处理股票代码格式
            stockCode = stockCode.ToLower().Trim();
            
            if (stockCode.StartsWith("sh") || stockCode.StartsWith("sz"))
            {
                return stockCode;
            }
            
            // 根据代码判断市场
            if (stockCode.StartsWith("00") || stockCode.StartsWith("30"))
            {
                return "sz" + stockCode;
            }
            else if (stockCode.StartsWith("60") || stockCode.StartsWith("68"))
            {
                return "sh" + stockCode;
            }
            else
            {
                // 默认上海
                return "sh" + stockCode;
            }
        }

        private StockData ParseTencentData(string data, string originalCode)
        {
            try
            {
                // 腾讯API返回格式：v_股票代码="股票信息";
                if (string.IsNullOrEmpty(data) || !data.Contains("=\""))
                {
                    return null;
                }

                int startIndex = data.IndexOf("=\"") + 2;
                int endIndex = data.IndexOf("\";", startIndex);
                
                if (startIndex < 2 || endIndex < 0)
                {
                    return null;
                }

                string stockInfo = data.Substring(startIndex, endIndex - startIndex);
                string[] parts = stockInfo.Split('~');

                if (parts.Length < 50) // 腾讯API通常返回50+个字段
                {
                    return null;
                }

                var stockData = new StockData
                {
                    Code = originalCode,
                    Name = parts[1], // 股票名称
                    Price = ParseDecimal(parts[3]), // 当前价格
                    Change = ParseDecimal(parts[31]), // 涨跌额
                    ChangePercent = ParseDecimal(parts[32]), // 涨跌幅%
                    OpenPrice = ParseDecimal(parts[5]), // 开盘价
                    ClosePrice = ParseDecimal(parts[4]), // 昨收价
                    HighPrice = ParseDecimal(parts[33]), // 最高价
                    LowPrice = ParseDecimal(parts[34]), // 最低价
                    Volume = ParseLong(parts[36]), // 成交量
                    UpdateTime = DateTime.Now
                };

                return stockData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"解析股票数据失败: {ex.Message}");
                return null;
            }
        }

        private decimal ParseDecimal(string value)
        {
            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
            {
                return result;
            }
            return 0;
        }

        private long ParseLong(string value)
        {
            if (long.TryParse(value, out long result))
            {
                return result;
            }
            return 0;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}