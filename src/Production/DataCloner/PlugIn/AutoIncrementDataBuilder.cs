﻿using System;
using System.Collections.Generic;
using System.Data;
using DataCloner.DataClasse.Cache;

namespace DataCloner.PlugIn
{
    internal class AutoIncrementDataBuilder : IDataBuilder
    {
        private static Dictionary<string, object> _autoIncrementCache = new Dictionary<string, object>();

        public object BuildData(IDbConnection conn, DbEngine engine, Int16 serverId, string database, string schema, ITableSchema table, IColumnDefinition column)
        {
            var cacheId = serverId + database + schema + table.Name + column.Name;

            if (_autoIncrementCache.ContainsKey(cacheId))
                IncrementNumber(cacheId);
            else
            {
                switch (engine)
                {
                    case DbEngine.MySql:
                        _autoIncrementCache.Add(cacheId, GetNewKeyMySql(conn, database, table, column));
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
            return _autoIncrementCache[cacheId];
        }

        private static void IncrementNumber(string cacheId)
        {
            Type t = _autoIncrementCache[cacheId].GetType();
            if (t == typeof(Int16))
            {
                var n = (Int16)_autoIncrementCache[cacheId];
                _autoIncrementCache[cacheId] = ++n;
            }
            else if (t == typeof(Int32))
            {
                var n = (Int32)_autoIncrementCache[cacheId];
                _autoIncrementCache[cacheId] = ++n;
            }
            else if (t == typeof(Int64))
            {
                var n = (Int64)_autoIncrementCache[cacheId];
                _autoIncrementCache[cacheId] = ++n;
            }
            else if (t == typeof(UInt16))
            {
                var n = (UInt16)_autoIncrementCache[cacheId];
                _autoIncrementCache[cacheId] = ++n;
            }
            else if (t == typeof(UInt32))
            {
                var n = (UInt32)_autoIncrementCache[cacheId];
                _autoIncrementCache[cacheId] = ++n;
            }
            else if (t == typeof(UInt64))
            {
                var n = (UInt64)_autoIncrementCache[cacheId];
                _autoIncrementCache[cacheId] = ++n;
            }
        }

        private object GetNewKeyMySql(IDbConnection conn, string database, ITableSchema table, IColumnDefinition column)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = String.Format("SELECT MAX({0})+1 FROM {1}.{2}", column.Name, database, table.Name);
            conn.Open();
            var result = cmd.ExecuteScalar();
            conn.Close();
            return result;
        }

        public void ClearCache()
        {
        }
    }
}
