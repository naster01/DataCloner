﻿using DataCloner.Core.Data.Generator;
using DataCloner.Core.Data.Generator.MySql;

namespace DataCloner.Core.Data.MySql
{
    internal sealed class MySqlQueryProvider : QueryProvider 
    {
        private static MySqlQueryProvider _instance;

        public const string ProviderName = "MySql.Data.MySqlClient";

         protected override string SqlGetLastInsertedPk =>
            "SELECT LAST_INSERT_ID();"; 

        protected override string SqlEnforceIntegrityCheck =>
            "SET UNIQUE_CHECKS=@ACTIVE; SET FOREIGN_KEY_CHECKS=@ACTIVE;"; 

        public override DbEngine Engine => DbEngine.MySql;
        public override ISqlWriter SqlWriter { get; }

        public static MySqlQueryProvider Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new MySqlQueryProvider();
                return _instance;
            }
        }

        public MySqlQueryProvider()
        {
            SqlWriter = new MySqlWriter();
        }      
    }
}