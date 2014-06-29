﻿using System;
using System.Collections.Generic;
using DataCloner.Interface;

namespace DataCloner.DataClasse
{
    public class RowIdentifier : IRowIdentifier
    {
        public Int16 ServerId { get; set; }
        public string DatabaseName { get; set; }
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public IDictionary<string, object> Columns { get; set; }

        public RowIdentifier()
        {
            Columns = new Dictionary<string, object>();
        }
    }
}
