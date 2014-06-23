﻿using System;
using System.Text;
using System.Data;
using System.Collections.Generic;

using DataCloner.DataClasse.Cache;
using DataCloner.Interface;
using MySql.Data.MySqlClient;
using IQueryProvider = DataCloner.Interface.IQueryProvider;

namespace DataCloner.DataAccess
{
    public class QueryProviderMySql : IQueryProvider
    {
        private MySqlConnection _conn;
        private readonly Int16 _serverIdCtx;
        private readonly bool _isReadOnly;

        public QueryProviderMySql(string connectionString, Int16 serverId)
        {
            _serverIdCtx = serverId;
            _conn = new MySqlConnection(connectionString);
            _conn.Open();
        }

        public QueryProviderMySql(string connectionString, Int16 serverId, bool readOnly)
            : this(connectionString, serverId)
        {
            _isReadOnly = readOnly;
        }

        ~QueryProviderMySql()
        {
            Dispose(false);
        }

        public IDbConnection Connection
        {
            get { return _conn; }
        }

        public bool IsReadOnly
        {
            get { return _isReadOnly; }
        }

        public string[] GetDatabasesName()
        {
            List<string> databases = new List<string>();

            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText = "SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA " +
                                  "WHERE SCHEMA_NAME NOT IN ('information_schema','performance_schema','mysql')";
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        databases.Add(r.GetString(0));
                }
            }
            return databases.ToArray();
        }

        public void FillForeignKeys(Action<IDataReader,Int16,string> reader, string database)
        {
            var sql =
                "SELECT " +
                    "'' AS SHEMA," +
                    "TABLE_NAME," +
                    "COLUMN_NAME," +
                    "DATA_TYPE," +
                    "COLUMN_KEY = 'PRI' AS 'IsPrimaryKey'," +
                    "1 AS 'IsForeignKey'," +
                    "EXTRA = 'auto_increment' AS 'IsAutoIncrement' " +
                "FROM INFORMATION_SCHEMA.COLUMNS " +
                "WHERE TABLE_SCHEMA = @DATABASE " +
                "ORDER BY " +
                    "TABLE_NAME," +
                    "ORDINAL_POSITION";

            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.Parameters.Add("@DATABASE", database);
                using (var r = cmd.ExecuteReader())
                {
                    reader(r, _serverIdCtx, database);
                }
            }
        }

        public DataTable GetFk(ITableIdentifier ti)
        {
            var dtReturn = new DataTable();

            var sql =
                "SELECT " +
                "	TC.TABLE_SCHEMA," +
                "	TC.TABLE_NAME," +
                " k.COLUMN_NAME," +
                " K.REFERENCED_TABLE_SCHEMA," +
                "	K.REFERENCED_TABLE_NAME," +
                "	K.REFERENCED_COLUMN_NAME " +
                "FROM information_schema.TABLE_CONSTRAINTS TC " +
                "LEFT JOIN information_schema.KEY_COLUMN_USAGE K ON TC.CONSTRAINT_NAME = K.CONSTRAINT_NAME " +
                "WHERE TC.CONSTRAINT_TYPE = 'FOREIGN KEY' " +
                "AND TC.TABLE_SCHEMA = @shema " +
                "AND TC.TABLE_NAME = @table";

            var cmd = new MySqlCommand(sql, _conn);
            cmd.Parameters.AddWithValue("@shema", ti.SchemaName);
            cmd.Parameters.AddWithValue("@table", ti.TableName);

            new MySqlDataAdapter(cmd).Fill(dtReturn);

            return dtReturn;
        }

        public Int64 GetLastInsertedPk()
        {
            var cmd = new MySqlCommand("SELECT LAST_INSERT_ID();", _conn);
            return (Int64)cmd.ExecuteScalar();
        }

        public DataTable Select(IRowIdentifier ri)
        {
            var dtReturn = new DataTable();
            var cmd = new MySqlCommand();
            var sql = new StringBuilder("SELECT * FROM ");
            sql.Append(ri.TableIdentifier.DatabaseName)
               .Append(".")
               .Append(ri.TableIdentifier.TableName);

            if (ri.Columns.Count > 1)
                sql.Append(" WHERE 1=1");

            foreach (var kv in ri.Columns)
            {
                sql.Append(" AND ")
                   .Append(kv.Key)
                   .Append(" = @")
                   .Append(kv.Key);

                cmd.Parameters.AddWithValue("@" + kv.Key, kv.Value);
            }

            cmd.CommandText = sql.ToString();
            cmd.Connection = _conn;

            new MySqlDataAdapter(cmd).Fill(dtReturn);

            return dtReturn;
        }

        public void Insert(ITableIdentifier ti, DataRow[] rows)
        {
            var cmd = new MySqlCommand();
            var sql = new StringBuilder("INSERT INTO  ");
            sql.Append(ti.DatabaseName)
               .Append(".")
               .Append(ti.TableName)
               .Append(" VALUES(");



            /*TODO : RÉCUPÉRER LE SHÉMA DE LA TABLE
             * Pour chaque colonne qui n'est pas une PK autoincrement, 
             * construire la requête

            */
            /*            foreach (var row in rows)
                        {
                            sql.Append(" AND ")
                               .Append(kv.Key)
                               .Append(" = @")
                               .Append(kv.Key);

                            cmd.Parameters.AddWithValue("@" + kv.Key, kv.Value);
                        }

                        cmd.CommandText = sql.ToString();
                        cmd.Connection = _conn;

                        new MySqlDataAdapter(cmd).*/

        }

        public void Update(IRowIdentifier ri, DataRow[] rows)
        {
            throw new NotImplementedException();
        }

        public void Delete(IRowIdentifier ri)
        {
            var cmd = new MySqlCommand();
            var sql = new StringBuilder("DELETE FROM ");
            sql.Append(ri.TableIdentifier.DatabaseName)
               .Append(".")
               .Append(ri.TableIdentifier.TableName);

            if (ri.Columns.Count > 1)
                sql.Append(" WHERE 1=1");

            foreach (var kv in ri.Columns)
            {
                sql.Append(" AND ")
                   .Append(kv.Key)
                   .Append(" = @")
                   .Append(kv.Key);

                cmd.Parameters.AddWithValue("@" + kv.Key, kv.Value);
            }

            cmd.CommandText = sql.ToString();
            cmd.Connection = _conn;
            cmd.ExecuteNonQuery();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_conn != null)
                {
                    if (_conn.State != ConnectionState.Closed)
                        _conn.Close();
                    _conn.Dispose();
                    _conn = null;
                }
            }
        }

        //public bool OpenConnection()
        //{
        //    try
        //    {
        //        _conn.Open();
        //        return true;
        //    }
        //    catch (MySqlException ex)
        //    {
        //        switch (ex.Number)
        //        {
        //            case 0:
        //                throw new MySqlException("Cannot connect to server", ex.Number);
        //                break;

        //            case 1042:
        //                MessageBox.Show("Unable to connect to any of the specified MySQL hosts");
        //                break;

        //            case 1045:
        //                MessageBox.Show("Invalid username/password");
        //                break;
        //        }
        //        return false;
        //    }
        //}

        public void Init()
        {
            throw new NotImplementedException();
        }
    }
}
