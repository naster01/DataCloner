﻿using DataCloner.Core.Metadata;
using System;
using System.Data.Common;

namespace DataCloner.Core.Data
{
    internal static class QueryHelperFactory 
    {
        public static IQueryHelper GetQueryHelper(AppMetadata schema, string providerName, string connectionString)
        {
            switch (providerName)
            {
                case QueryHelperMsSql.ProviderName:
                    return new QueryHelperMsSql(schema, connectionString);
                case QueryHelperMySql.ProviderName:
                    return new QueryHelperMySql(schema, connectionString);
                case QueryHelperPostgreSql.ProviderName:
                    return new QueryHelperPostgreSql(schema, connectionString);
                //case OracleProvider.ProviderName:
                //    return new OracleProvider();
                //case SqlServerCEProvider.ProviderName:
                //    return new SqlServerCEProvider();
                //case SqliteProvider.ProviderName:
                //    return new SqliteProvider();
            }
            throw new Exception("Unkown provider");
        }

        public static IQueryHelper GetQueryHelper(this DbConnection cnx, AppMetadata schema)
        {
            var type = cnx.GetType().Name;

            //if (type.Equals("SqlConnection", StringComparison.InvariantCultureIgnoreCase))
            //    return new SqlServerProvider();

            if (type.StartsWith("MySql"))
                return new QueryHelperMySql(schema, cnx.ConnectionString);

            //if (type.StartsWith("Npgsql"))
            //    return new PostgresProvider();

            //if (type.StartsWith("SQLite"))
            //    return new SqliteProvider();

            //if (type.Equals("SqlCeConnection", StringComparison.InvariantCultureIgnoreCase))
            //    return new SqlServerCEProvider();

            throw new NotSupportedException();
        }
    }
}
