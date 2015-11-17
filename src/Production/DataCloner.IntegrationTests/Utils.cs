﻿using DataCloner.Configuration;
using DataCloner.Data;
using System.Collections.Generic;

namespace DataCloner.IntegrationTests
{
    public static class Utils
    {
        public static Settings MakeDefaultSettings(SqlConnection conn)
        {
            return new Settings
            {
                UseInMemoryCacheOnly = true,
                BehaviourId = 1,
                MapId = 1,
                Project = new ProjectContainer
                {
                    Name = "chinook",
                    ConnectionStrings = new List<Connection>
                    {
                        new Connection
                        {
                            Id = conn.Id,
                            ProviderName = conn.ProviderName,
                            ConnectionString = conn.ConnectionString
                        }
                    },
                    Maps = new List<Map>
                    {
                        new Map
                        {
                            Id = 1,
                            UsableBehaviours = "1"
                        }
                    },
                    Behaviours = new List<Behaviour>
                    {
                        new Behaviour
                        {
                             Id = 1,
                              Modifiers = new Modifiers
                              {
                                   ServerModifiers = new List<ServerModifier>
                                   {
                                        new ServerModifier
                                        {
                                             Id = "1",
                                              Databases = new List<DatabaseModifier>
                                              {
                                                   new DatabaseModifier
                                                   {
                                                        Name = "chinook",
                                                         Schemas = new List<SchemaModifier>
                                                         {
                                                              new SchemaModifier
                                                              {
                                                                   Name = "dbo",
                                                                   Tables = new List<TableModifier>{}
                                                              }
                                                         }
                                                   }
                                              }
                                        }
                                   }
                              }
                        }
                    }
                }
            };
        }

        public static List<TableModifier> GetDefaultSchema(this Settings settings)
        {
            return settings.Project.Behaviours[0].Modifiers.ServerModifiers[0].Databases[0].Schemas[0].Tables;
        }
    }
}