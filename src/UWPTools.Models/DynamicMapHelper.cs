using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWPTools.Models
{
    public class DynamicMapHelper
    {
        /// <summary>
        /// 动态类型版本（无需预先定义实体类）
        /// </summary>
        public static Func<SqliteDataReader, ExpandoObject> CreateDynamicMapFunc(string jsonMapping)
        {
            var mappingDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonMapping);
            return reader =>
            {
                var dynamicObj = new ExpandoObject() as IDictionary<string, object>;
                foreach (var kvp in mappingDict)
                {
                    try
                    {
                        var value = reader[kvp.Key];
                        dynamicObj[kvp.Value] = value == DBNull.Value ? null : value;
                    }
                    catch
                    {
                        dynamicObj[kvp.Value] = null;
                    }
                }
                return dynamicObj as ExpandoObject;
            };
        }

        public static Dictionary<string,string> GetColumnHeaders(string jsonStr)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonStr);
        }

        /// <summary>
        /// 将ExpandoObject列表转换为控制台表格
        /// </summary>
        /// <param name="items">ExpandoObject列表</param>
        /// <param name="columnHeaders">自定义列标题（键：属性名，值：显示标题）</param>
        public static void PrintAsTableToConsole(List<ExpandoObject> items, Dictionary<string, string> columnHeaders = null)
        {
            if (items == null || items.Count == 0)
            {
                Debug.WriteLine("没有数据可显示");
                return;
            }

            // 获取所有属性名（取第一个对象的属性作为基准）
            var firstItem = items[0] as IDictionary<string, object>;
            var propertyNames = firstItem.Keys.ToList();

            // 准备列标题和最大宽度
            var columnTitles = new Dictionary<string, string>();
            var maxWidths = new Dictionary<string, int>();

            foreach (var propName in propertyNames)
            {
                // 确定列标题
                var title = columnHeaders != null && columnHeaders.ContainsKey(propName)
                    ? columnHeaders[propName]
                    : propName;
                columnTitles[propName] = title;

                // 计算该列最大宽度（标题宽度 vs 所有值的宽度）
                var maxValueWidth = items
                    .Select(item => (item as IDictionary<string, object>)[propName]?.ToString()?.Length ?? 0)
                    .Max();
                maxWidths[propName] = Math.Max(title.Length, maxValueWidth);
            }

            // 打印表头分隔线
            PrintSeparatorToConsole(maxWidths);

            // 打印表头
            foreach (var propName in propertyNames)
            {
                Debug.Write($"| {columnTitles[propName].PadRight(maxWidths[propName])} ");
            }
            Debug.WriteLine("|");

            // 打印表头分隔线
            PrintSeparatorToConsole(maxWidths);

            // 打印数据行
            foreach (var item in items)
            {
                var dictItem = item as IDictionary<string, object>;
                foreach (var propName in propertyNames)
                {
                    var value = dictItem[propName]?.ToString() ?? "null";
                    Debug.Write($"| {value.PadRight(maxWidths[propName])} ");
                }
                Debug.WriteLine("|");
            }

            // 打印表尾分隔线
            PrintSeparatorToConsole(maxWidths);
        }

        /// <summary>
        /// 打印表格分隔线
        /// </summary>
        private static void PrintSeparatorToConsole(Dictionary<string, int> maxWidths)
        {
            foreach (var width in maxWidths.Values)
            {
                Debug.Write("+");
                Debug.Write(new string('-', width + 2)); // 加2是因为有两边的空格
            }
            Debug.WriteLine("+");
        }


        /// <summary>
        /// 将ExpandoObject列表转换为控制台表格
        /// </summary>
        /// <param name="items">ExpandoObject列表</param>
        /// <param name="columnHeaders">自定义列标题（键：属性名，值：显示标题）</param>
        public static string PrintAsTableToString(List<ExpandoObject> items, Dictionary<string, string> columnHeaders = null)
        {
            string response = "";
            if (items == null || items.Count == 0)
            {
                //Debug.WriteLine("没有数据可显示");
                response += "没有数据可显示\n";
                return response;
            }

            // 获取所有属性名（取第一个对象的属性作为基准）
            var firstItem = items[0] as IDictionary<string, object>;
            var propertyNames = firstItem.Keys.ToList();

            // 准备列标题和最大宽度
            var columnTitles = new Dictionary<string, string>();
            var maxWidths = new Dictionary<string, int>();

            foreach (var propName in propertyNames)
            {
                // 确定列标题
                var title = columnHeaders != null && columnHeaders.ContainsKey(propName)
                    ? columnHeaders[propName]
                    : propName;
                columnTitles[propName] = title;

                // 计算该列最大宽度（标题宽度 vs 所有值的宽度）
                var maxValueWidth = items
                    .Select(item => (item as IDictionary<string, object>)[propName]?.ToString()?.Length ?? 0)
                    .Max();
                maxWidths[propName] = Math.Max(title.Length, maxValueWidth);
            }

            // 打印表头分隔线
            PrintSeparatorToString(maxWidths);

            // 打印表头
            foreach (var propName in propertyNames)
            {
                //Debug.Write($"| {columnTitles[propName].PadRight(maxWidths[propName])} ");
                response += $"| {columnTitles[propName].PadRight(maxWidths[propName])} ";
            }
            //Debug.WriteLine("|");
            response += "|\n";

            // 打印表头分隔线
            PrintSeparatorToString(maxWidths);

            // 打印数据行
            foreach (var item in items)
            {
                var dictItem = item as IDictionary<string, object>;
                foreach (var propName in propertyNames)
                {
                    var value = dictItem[propName]?.ToString() ?? "null";
                    //Debug.Write($"| {value.PadRight(maxWidths[propName])} ");
                    response += $"| {value.PadRight(maxWidths[propName])} ";
                }
                //Debug.WriteLine("|");
                response += "|\n";
            }

            // 打印表尾分隔线
            PrintSeparatorToString(maxWidths);

            response += "\n";

            return response;
        }

        /// <summary>
        /// 打印表格分隔线
        /// </summary>
        private static string PrintSeparatorToString(Dictionary<string, int> maxWidths)
        {
            string response = "";
            foreach (var width in maxWidths.Values)
            {
                //Debug.Write("+");
                //Debug.Write(new string('-', width + 2)); // 加2是因为有两边的空格
                response += "+";
                response += new string('-', width + 2);
            }
            //Debug.WriteLine("+");
            response += "+\n";

            return response;
        }
    }
}
