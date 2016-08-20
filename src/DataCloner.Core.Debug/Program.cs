﻿using DataCloner.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataCloner.Core.Framework;

namespace DataCloner.Core.Debug
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var project = new ProjectContainer { Name = "MainApp" };
            project.ConnectionStrings.Add(new Connection(1, "PROD", "DataCloner.Data.QueryProviderMySql", "server=localhost;user id=root; password=cdxsza; database=mysql; pooling=false"));
            project.ConnectionStrings.Add(new Connection(2, "UNI", "DataCloner.Data.QueryProviderMySql", "server=localhost;user id=root; password=cdxsza; database=mysql; pooling=false"));

            var table1 = new Table
            {
                Name = "table1",
                IsStatic = false,
                DataBuilders = new List<DataBuilder>
                {
                    new DataBuilder
                    {
                        BuilderName = "Client.Builder.CreatePK",
                        Name = "col1"
                    }
                },
                DerativeTables = new DerivativeTable
                {
                    GlobalAccess = DerivativeTableAccess.Forced,
                    GlobalCascade = true,
                    DerivativeSubTables = new List<DerivativeSubTable>
                    {
                        new DerivativeSubTable
                        {
                            ServerId = "1",
                            Database = "db",
                            Schema = "dbo",
                            Table = "table2",
                            Access = DerivativeTableAccess.Denied
                        }
                    }
                }
            };

            table1.ForeignKeys.ForeignKeyAdd.Add(new ForeignKeyAdd
            {
                ServerId = "1",
                Database = "db",
                Schema = "dbo",
                Table = "table55",
                Columns = new List<ForeignKeyColumn>
                {
                    new ForeignKeyColumn
                    {
                        NameFrom = "col1",
                        NameTo = "col1"
                    },
                    new ForeignKeyColumn
                    {
                        NameFrom = "col2",
                        NameTo = "col2"
                    }
                }
            });

            table1.ForeignKeys.ForeignKeyRemove = new ForeignKeyRemove
            {
                Columns = new List<ForeignKeyRemoveColumn>
                {
                    new ForeignKeyRemoveColumn
                    {
                        Name = "col3"
                    },
                    new ForeignKeyRemoveColumn
                    {
                        Name = "col4"
                    }
                }
            };

            var server1 = new ServerModifier
            {
                Id = "1",
                Databases = new List<Database>
                {
                    new Database
                    {
                        Var = "db",
                        Schemas = new List<Schema>
                        {
                            new Schema
                            {
                                Var = "dbo",
                                Tables = new List<Table> { table1 }
                            }
                        }
                    }
                }
            };

            var clonerBehaviour = new Behaviour
            {
                Id = 1,
                Name = "Basic clone",
                Description = "Only cloning besic data",
                //Servers = new List<ServerModifier> { server1 }
            };

            project.Behaviours.Add(clonerBehaviour);

            project.Maps = new List<Map>
            {
                new Map
                {
                     From = "UNI",
                     To = "FON",
                     UsableBehaviours = "1,2",
                     Variables = new List<Variable>
                     {
                         new Variable { Name = ""}
                     },
                     Roads = new List<Road>
                     {
                         new Road
                         {
                             ServerSrc = "1", SchemaSrc = "dbo", DatabaseSrc = "myDB",
                             ServerDst = "1", SchemaDst = "dbo", DatabaseDst = "myDB"
                         }
                     }
                }
            };
            project.Templates.Servers.Add(server1);

            var xml = project.SerializeXml();
        }
    }
}
