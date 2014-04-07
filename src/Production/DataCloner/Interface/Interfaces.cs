﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataCloner
{
    public interface ITableIdentifier
    {
        Int16 ServerID { get; set; }
        string DatabaseName { get; set; }
        string SchemaName { get; set; }
        string TableName { get; set; }
    }

    public interface IColumnIdentifier : ITableIdentifier
    {
        string ColumnName { get; set; }
    }

    public interface IRowIdentifier
    {
        ITableIdentifier TableIdentifier { get; set; }
        IDictionary<string, object> Columns { get; set; }
    }

    public interface IStaticTableDictionnary : IDictionary<ITableIdentifier, bool>
    {
    }
}
