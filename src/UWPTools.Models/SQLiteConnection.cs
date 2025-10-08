using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace UWPTools.Models
{
    public class SQLiteConnection : IDisposable
    {
        // SQLite 连接对象
        private SqliteConnection _connection;
        // 数据库文件路径（UWP 沙盒内的 LocalFolder）
        private readonly string _dbPath;

        /// <summary>
        /// 构造函数（指定数据库名称，如 "MyDataBase.db"）
        /// </summary>
        /// <param name="dbPath">数据库文件完整路径</param>
        public SQLiteConnection(string dbPath)
        {
            // UWP 数据库文件需放在 LocalFolder（沙盒路径）
            _dbPath = dbPath;
        }

        /// <summary>
        /// 打开数据库连接
        /// </summary>
        private async Task OpenConnectionAsync()
        {
            if (_connection == null)
            {
                _connection = new SqliteConnection($"Filename={_dbPath}");
            }
            if (_connection.State != System.Data.ConnectionState.Open)
            {
                await _connection.OpenAsync();
            }
        }

        /// <summary>
        /// 执行无返回值SQL（插入/更新/删除）
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">参数（避免SQL注入）</param>
        /// <returns>受影响的行数</returns>
        public async Task<int> ExecuteNonQueryAsync(string sql, params SqliteParameter[] parameters)
        {
            await OpenConnectionAsync();
            using (var command = new SqliteCommand(sql, _connection))
            {
                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }
                return await command.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// 执行单值查询（如 COUNT、SUM）
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">参数</param>
        /// <returns>查询结果（默认返回类型默认值）</returns>
        public async Task<T> ExecuteScalarAsync<T>(string sql, params SqliteParameter[] parameters)
        {
            await OpenConnectionAsync();
            using (var command = new SqliteCommand(sql, _connection))
            {
                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }
                var result = await command.ExecuteScalarAsync();

                return result == DBNull.Value ? default(T) : (T)Convert.ChangeType(result, typeof(T));
            }
        }

        /// <summary>
        /// 执行查询（返回单个实体）
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="sql">SQL语句</param>
        /// <param name="mapFunc">DataReader到实体的映射函数</param>
        /// <param name="parameters">参数</param>
        /// <returns>单个实体（无结果则返回null）</returns>
        public async Task<T> QuerySingleAsync<T>(string sql, Func<SqliteDataReader, T> mapFunc, params SqliteParameter[] parameters)
        {
            await OpenConnectionAsync();
            using (var command = new SqliteCommand(sql, _connection))
            {
                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }
                using (var reader = await command.ExecuteReaderAsync())
                {
                    return await reader.ReadAsync() ? mapFunc(reader) : default(T);
                }
            }
        }

        /// <summary>
        /// 执行查询（返回实体列表）
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="sql">SQL语句</param>
        /// <param name="mapFunc">DataReader到实体的映射函数</param>
        /// <param name="parameters">参数</param>
        /// <returns>实体列表</returns>
        public async Task<List<T>> QueryListAsync<T>(string sql, Func<SqliteDataReader, T> mapFunc, params SqliteParameter[] parameters)
        {
            var list = new List<T>();
            await OpenConnectionAsync();
            using (var command = new SqliteCommand(sql, _connection))
            {
                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(mapFunc(reader));
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// 分页查询（返回分页实体列表）
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="sql">基础查询SQL（不含ORDER BY和LIMIT）</param>
        /// <param name="orderField">排序字段（如 "Id DESC"）</param>
        /// <param name="pageIndex">页码（从1开始）</param>
        /// <param name="pageSize">每页条数</param>
        /// <param name="mapFunc">DataReader到实体的映射函数</param>
        /// <param name="parameters">基础查询参数</param>
        /// <returns>分页结果（实体列表）</returns>
        public async Task<List<T>> QueryByPageAsync<T>(
            string sql,
            string orderField,
            int pageIndex,
            int pageSize,
            Func<SqliteDataReader, T> mapFunc,
            params SqliteParameter[] parameters)
        {
            // 计算分页偏移量（SQLite LIMIT 偏移量, 条数）
            int offset = (pageIndex - 1) * pageSize;
            // 拼接分页SQL（ORDER BY必须在LIMIT前）
            string pageSql = $"{sql} ORDER BY {orderField} LIMIT {offset}, {pageSize}";
            // 执行分页查询
            return await QueryListAsync(pageSql, mapFunc, parameters);
        }

        /// <summary>
        /// 释放数据库连接资源
        /// </summary>
        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}
