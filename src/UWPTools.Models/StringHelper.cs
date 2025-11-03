using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWPTools.Models
{
    public class StringHelper
    {
        public static string GetURLFromURLWithQueryParameters(string url)
        {
            int queryIndex = url.IndexOf('?');
            if (queryIndex == -1)
            {
                return url;
            }

            string baseUrl = url.Substring(0, queryIndex);
            string queryString = url.Substring(queryIndex + 1);
            string[] parameters = queryString.Split('&');

            string newQuery = "";
            for (int i = 0; i < parameters.Length; i++)
            {
                int equalsIndex = parameters[i].IndexOf('=');
                if (equalsIndex != -1)
                {
                    string paramName = parameters[i].Substring(0, equalsIndex);
                    newQuery += (i > 0 ? "&" : "") + paramName + "={}";
                }
            }

            return baseUrl + "?" + newQuery;
        }

        public static Dictionary<string, string> GetURLQueryParameters(string url)
        {
            Dictionary<string, string> parameterDictionary = new Dictionary<string, string>();
            int queryIndex = url.IndexOf('?');
            if (queryIndex == -1)
            {
                return parameterDictionary;
            }

            string queryString = url.Substring(queryIndex + 1);
            string[] parameterPairs = queryString.Split('&');

            foreach (string pair in parameterPairs)
            {
                string[] keyValue = pair.Split('=');
                if (keyValue.Length == 2)
                {
                    string key = keyValue[0];
                    string value = keyValue[1];
                    parameterDictionary[key] = value;
                }
            }

            return parameterDictionary;
        }

        public static bool IsValidFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || fileName.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static string ByteArrayToString(byte[] bytes)
        {
            string result = Encoding.UTF8.GetString(bytes);
            return result;
        }

        public static bool IsIpaddr(string str)
        {
            System.Net.IPAddress ipAddress;
            if (System.Net.IPAddress.TryParse(str, out ipAddress))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static readonly string[] Units = { "B", "KB", "MB", "GB" };
        private static readonly double[] UnitSizes = {
            1,
            1024,
            1024 * 1024,
            1024 * 1024 * 1024,
        };

        public static string GetByteUnit(long byteNum)
        {
            int unitIndex = 0;
            double value = byteNum;

            // 确定最适合的单位
            while (value >= 1024 && unitIndex < Units.Length - 1)
            {
                value /= 1024;
                unitIndex++;
            }


            if (unitIndex > 3)
                unitIndex = 3;
            return value.ToString("0.00") + Units[unitIndex];
        }

        public static string TimeNumToString(int Time)
        {
            return Time.ToString().Length == 1 ? "0" + Time.ToString() : Time.ToString();
        }

        public static string StringArrayToString(string[] strArray, string separator)
        {
            if (strArray == null || strArray.Length == 0)
                return "";
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < strArray.Length; i++)
            {
                sb.Append(strArray[i]);
                if (i != strArray.Length - 1)
                    sb.Append(separator);
            }
            return sb.ToString();
        }
        public static string RemoveIllegalCharacter(String str)
        {
            return str.Replace("/", "").Replace("\\", "").Replace("*", "").Replace("?", "").Replace(":", "").Replace("|", "").Replace("\"", "").Replace("<", "").Replace(">", "");
        }

        public static TimeSpan ConvertLRCTimeToTimeSpan(string timeString)
        {
            // 检查输入字符串是否为空或者无效
            if (string.IsNullOrEmpty(timeString))
            {
                throw new ArgumentNullException(nameof(timeString), "输入字符串不能为空");
            }

            // 按冒号分割分钟和秒.毫秒部分
            string[] parts = timeString.Split(':');
            if (parts.Length != 2)
            {
                throw new FormatException("输入字符串格式应为'分钟:秒.毫秒'");
            }

            // 尝试解析分钟部分
            if (!int.TryParse(parts[0], out int minutes))
            {
                throw new FormatException("分钟部分解析失败");
            }

            // 按小数点分割秒和毫秒部分
            string[] secondParts = parts[1].Split('.');
            if (secondParts.Length != 2)
            {
                throw new FormatException("秒和毫秒部分格式应为'秒.毫秒'");
            }

            // 尝试解析秒和毫秒部分
            if (!int.TryParse(secondParts[0], out int seconds))
            {
                throw new FormatException("秒部分解析失败");
            }

            if (!int.TryParse(secondParts[1], out int milliseconds))
            {
                throw new FormatException("毫秒部分解析失败");
            }

            // 创建并返回TimeSpan对象
            return new TimeSpan(0, 0, minutes, seconds, milliseconds);
        }

        public static TimeSpan ConvertMinuteAndSecondTimeToTimeSpan(string timeString)
        {
            // 检查输入字符串是否为空或者无效
            if (string.IsNullOrEmpty(timeString))
            {
                throw new ArgumentNullException(nameof(timeString), "输入字符串不能为空");
            }

            // 按冒号分割分钟和秒.毫秒部分
            string[] parts = timeString.Split(':');
            if (parts.Length != 2)
            {
                throw new FormatException("输入字符串格式应为'分钟:秒'");
            }

            // 尝试解析分钟部分
            if (!int.TryParse(parts[0], out int minutes))
            {
                throw new FormatException("分钟部分解析失败");
            }

            // 尝试解析秒部分
            if (!int.TryParse(parts[1], out int seconds))
            {
                throw new FormatException("秒部分解析失败");
            }

            return new TimeSpan(0, 0, minutes, seconds);
        }
    }
}
