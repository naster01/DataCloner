﻿using DataCloner.Core.Data.Generator;
using System;
using System.Data;

namespace DataCloner.Core.Data
{
    public delegate void ColumnReader(IDataReader reader, Int16 serverId, string database, ISqlTypeConverter typeConverter);
    public delegate void ForeignKeyReader(IDataReader reader, Int16 serverId, string database);
    public delegate void UniqueKeyReader(IDataReader reader, Int16 serverId, string database);
    public delegate void SqlToClrTypeConverter(string fullType, out DbType type, out string size);

    public interface IQueryHelper : IDisposable
    {
        event QueryCommitingEventHandler QueryCommmiting;
        IDbConnection Connection { get; }
        ISqlTypeConverter TypeConverter { get; }
        ISqlWriter SqlWriter { get; }
        DbEngine Engine { get; }
        string[] GetDatabasesName();
        void GetColumns(ColumnReader reader, Int16 serverId, String database);
        void GetForeignKeys(ForeignKeyReader reader, Int16 serverId, String database);
        void GetUniqueKeys(UniqueKeyReader reader, Int16 serverId, String database);
        object GetLastInsertedPk();
        void EnforceIntegrityCheck(bool active);
        object[][] Select(RowIdentifier row);
        void Execute(Internal.ExecutionPlan plan);
    }
}
