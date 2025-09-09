using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UWPTools.Models
{
    public class JSONHelper
    {
        public static string FormatJson(string json, int indentSize = 4)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return json;
            }

            // 移除所有现有的空白字符
            json = Regex.Replace(json, @"\s+", "");

            // 定义缩进字符
            string indent = new string(' ', indentSize);
            int level = 0;
            bool inString = false;
            string result = "";

            for (int i = 0; i < json.Length; i++)
            {
                char c = json[i];

                // 检查是否在字符串内部（处理包含引号的字符串）
                if (c == '"' && (i == 0 || json[i - 1] != '\\'))
                {
                    inString = !inString;
                    result += c;
                    continue;
                }

                // 如果在字符串内部，直接添加字符
                if (inString)
                {
                    result += c;
                    continue;
                }

                // 处理不同的JSON符号
                switch (c)
                {
                    case '{':
                    case '[':
                        result += c;
                        level++;
                        result += Environment.NewLine + new string(' ', level * indentSize);
                        break;
                    case '}':
                    case ']':
                        level--;
                        result += Environment.NewLine + new string(' ', level * indentSize);
                        result += c;
                        break;
                    case ',':
                        result += c;
                        result += Environment.NewLine + new string(' ', level * indentSize);
                        break;
                    case ':':
                        result += ": ";
                        break;
                    default:
                        result += c;
                        break;
                }
            }

            return result;
        }
    }
}
