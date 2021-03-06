﻿using DataCloner.Core.Configuration;
using DataCloner.Core.Data;
using DataCloner.Core.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace DataCloner.Core.Metadata
{
    public sealed class SchemaMetadata : HashSet<TableMetadata> { }
    public sealed class DatabaseMetadata : Dictionary<string, SchemaMetadata>
    {
        public DatabaseMetadata() : base(StringComparer.OrdinalIgnoreCase) { }
    }
    
    public sealed class ServerMetadata : Dictionary<string, DatabaseMetadata>
    {
        public ServerMetadata() : base(StringComparer.OrdinalIgnoreCase) { }
    }

    /// <summary>
    /// Contient les tables statiques de la base de données
    /// </summary>
    /// <remarks>ServerId / Database / Schema -> TableMetadata[]</remarks>
    public sealed class AppMetadata : Dictionary<Int16, ServerMetadata>
    {
        public TableMetadata GetTable(Int16 server, string database, string schema, string table)
        {
            if (ContainsKey(server) &&
                this[server].ContainsKey(database) &&
                this[server][database].ContainsKey(schema))
                return this[server][database][schema].FirstOrDefault(t => t.Name.Equals(table, StringComparison.OrdinalIgnoreCase));
            return null;
        }

        public void Add(Int16 server, string database, string schema, TableMetadata table)
        {
            if (!ContainsKey(server))
                Add(server, new ServerMetadata());

            if (!this[server].ContainsKey(database))
                this[server].Add(database, new DatabaseMetadata());

            if (!this[server][database].ContainsKey(schema))
                this[server][database].Add(schema, new SchemaMetadata { table });
            else
            {
                if (!this[server][database][schema].Contains(table))
                    this[server][database][schema].Add(table);
            }
        }

        public bool Remove(Int16 server, string database, string schema, TableMetadata table)
        {
            if (ContainsKey(server) &&
                this[server].ContainsKey(database) &&
                this[server][database].ContainsKey(schema) &&
                this[server][database][schema].Contains(table))
            {
                this[server][database][schema].Remove(table);

                if (this[server][database][schema].Count == 0)
                {
                    this[server][database].Remove(schema);
                    if (this[server][database].Count == 0)
                    {
                        this[server].Remove(database);
                        if (this[server].Count == 0)
                            Remove(server);
                    }
                }
                return true;
            }
            return false;
        }

        public SchemaMetadata this[Int16 server, string database, string schema]
        {
            get
            {
                if (ContainsKey(server) &&
                    this[server].ContainsKey(database) &&
                    this[server][database].ContainsKey(schema))
                {
                    return this[server][database][schema];
                }
                return null;
            }
            set
            {
                if (!ContainsKey(server))
                    Add(server, new ServerMetadata());

                if (!this[server].ContainsKey(database))
                    this[server].Add(database, new DatabaseMetadata());

                if (!this[server][database].ContainsKey(schema))
                    this[server][database].Add(schema, value);
                else
                    this[server][database][schema] = value;
            }
        }

        internal void LoadForeignKeys(IDataReader reader, Int16 serverId, String database)
        {
            var lstForeignKeys = new List<ForeignKey>();
            var lstForeignKeyColumns = new List<ForeignKeyColumn>();

            if (!reader.Read())
                return;

            //Init first row
            var currentSchema = reader.GetString(0);
            var previousTable = this[serverId][database][currentSchema].First(t => t.Name.Equals(reader.GetString(1), StringComparison.OrdinalIgnoreCase));
            var previousConstraintName = reader.GetString(2);
            var previousConstraint = new ForeignKey
            {
                ServerIdTo = serverId,
                DatabaseTo = database,
                SchemaTo = currentSchema,
                TableTo = reader.GetString(5)
            };

            //Pour chaque ligne
            do
            {
                currentSchema = reader.GetString(0);
                var currentTable = reader.GetString(1);
                var currentConstraint = reader.GetString(2);

                //Si on change de constraint
                if (currentTable != previousTable.Name || currentConstraint != previousConstraintName)
                {
                    previousConstraint.Columns = lstForeignKeyColumns;
                    lstForeignKeys.Add(previousConstraint);

                    lstForeignKeyColumns = new List<ForeignKeyColumn>();
                    previousConstraint = new ForeignKey
                    {
                        ServerIdTo = serverId,
                        DatabaseTo = database,
                        SchemaTo = currentSchema,
                        TableTo = reader.GetString(5)
                    };
                    previousConstraintName = currentConstraint;
                }

                //Si on change de table
                if (currentTable != previousTable.Name)
                {
                    previousTable.ForeignKeys = lstForeignKeys;

                    //Change de table
                    previousTable = this[serverId][database][currentSchema].First(t => t.Name.Equals(reader.GetString(1), StringComparison.OrdinalIgnoreCase));
                    lstForeignKeys = new List<ForeignKey>();
                }

                //Ajoute la colonne
                var colName = reader.GetString(3);
                lstForeignKeyColumns.Add(new ForeignKeyColumn
                {
                    NameFrom = colName,
                    NameTo = reader.GetString(6)
                });

                //Affecte l'indicateur dans le schema
                var col = previousTable.ColumnsDefinition.FirstOrDefault(c => c.Name.Equals(colName, StringComparison.OrdinalIgnoreCase));
                if (col == null)
                    throw new Exception($"The column {colName} has not been found in the metadata for the table {previousTable.Name}.");
                col.IsForeignKey = true;
            } while (reader.Read());

            //Ajoute la dernière table / schema
            if (lstForeignKeyColumns.Count > 0)
            {
                previousConstraint.Columns = lstForeignKeyColumns;
                lstForeignKeys.Add(previousConstraint);
                previousTable.ForeignKeys = lstForeignKeys;
            }
        }

        internal void LoadUniqueKeys(IDataReader reader, Int16 serverId, String database)
        {
            var lstUniqueKeys = new List<UniqueKey>();
            var lstUniqueKeyColumns = new List<string>();

            if (!reader.Read())
                return;

            //Init first row
            var currentSchema = reader.GetString(0);
            var previousTable = this[serverId][database][currentSchema].First(t => t.Name.Equals(reader.GetString(1), StringComparison.OrdinalIgnoreCase));
            var previousConstraintName = reader.GetString(2);
            var previousConstraint = new UniqueKey();

            //Pour chaque ligne
            do
            {
                currentSchema = reader.GetString(0);
                var currentTable = reader.GetString(1);
                var currentConstraint = reader.GetString(2);

                //Si on change de constraint
                if (currentTable != previousTable.Name || currentConstraint != previousConstraintName)
                {
                    previousConstraint.Columns = lstUniqueKeyColumns;
                    lstUniqueKeys.Add(previousConstraint);

                    lstUniqueKeyColumns = new List<string>();
                    previousConstraint = new UniqueKey();
                    previousConstraintName = currentConstraint;
                }

                //Si on change de table
                if (currentTable != previousTable.Name)
                {
                    previousTable.UniqueKeys = lstUniqueKeys;

                    //Change de table
                    previousTable = this[serverId][database][currentSchema].First(t => t.Name.Equals(reader.GetString(1), StringComparison.OrdinalIgnoreCase));
                    lstUniqueKeys = new List<UniqueKey>();
                }

                //Ajoute la colonne
                var colName = reader.GetString(3);
                lstUniqueKeyColumns.Add(colName);

                //Affecte l'indicateur dans le schema
                var col = previousTable.ColumnsDefinition.FirstOrDefault(c => c.Name.Equals(colName, StringComparison.OrdinalIgnoreCase));
                if (col == null)
                    throw new Exception($"The column {colName} has not been found in the metadata for the table {previousTable.Name}.");
                col.IsUniqueKey = true;
            } while (reader.Read());

            //Ajoute la dernière table / schema
            if (lstUniqueKeyColumns.Count > 0)
            {
                previousConstraint.Columns = lstUniqueKeyColumns;
                lstUniqueKeys.Add(previousConstraint);
                previousTable.UniqueKeys = lstUniqueKeys;
            }
        }

        /// <summary>
        /// Load the metadata with the columns read from the IDataReader.
        /// The query must have this defenition in order : 
        /// Schema, Table, Column, DataType, Precision, Scale, IsUnsigned, IsPrimaryKey, IsAutoIncrement
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="serverId"></param>
        /// <param name="database"></param>
        /// <param name="typeConverter"></param>
		internal void LoadColumns(IDataReader reader, Int16 serverId, String database, ISqlTypeConverter typeConverter)
        {
            var schemaMetadata = new SchemaMetadata();
            var lstSchemaColumn = new List<ColumnDefinition>();
            string currentSchema;

            if (!reader.Read())
                return;

            //Init first row
            var previousSchema = reader.GetString(0);
            var previousTable = new TableMetadata(reader.GetString(1));

            //Pour chaque ligne
            do
            {
                currentSchema = reader.GetString(0);
                var currentTable = reader.GetString(1);

                //Si on change de table
                if (currentSchema != previousSchema || currentTable != previousTable.Name)
                {
                    previousTable.ColumnsDefinition = lstSchemaColumn;
                    schemaMetadata.Add(previousTable);

                    lstSchemaColumn = new List<ColumnDefinition>();
                    previousTable = new TableMetadata(currentTable);
                }

                //Si on change de schema
                if (currentSchema != previousSchema)
                {
                    this[serverId, database, currentSchema] = schemaMetadata;
                    schemaMetadata = new SchemaMetadata();
                }

                //Ajoute la colonne
                var col = new ColumnDefinition
                {
                    Name = reader.GetString(2),
                    SqlType = new SqlType
                    {
                        DataType = reader.GetString(3),
                        Precision = reader.GetInt32(4),
                        Scale = reader.GetInt32(5),
                        IsUnsigned = reader.GetBoolean(6)
                    },
                    IsPrimary = reader.GetBoolean(7),
                    IsAutoIncrement = reader.GetBoolean(8)
                };
                col.DbType = typeConverter.ConvertFromSql(col.SqlType);

                lstSchemaColumn.Add(col);

            } while (reader.Read());

            //Ajoute la dernière table / schema
            if (lstSchemaColumn.Count > 0)
            {
                previousTable.ColumnsDefinition = lstSchemaColumn;
                schemaMetadata.Add(previousTable);
                this[serverId, database, currentSchema] = schemaMetadata;
            }
        }

        /// <summary>
        /// Termine la construction des metadata avec les configurations utilisateurs.
        /// </summary>
        /// <param name="behaviour"></param>
        /// <remarks>Le schéma de la BD doit préalablement avoir été obtenu. GetColumns() et GetForeignKeys()</remarks>
        internal void FinalizeMetadata(Behaviour behaviour)
        {
            GenerateCommands();
            MergeFk(behaviour);
            GenerateDerivativeTables();
            MergeBehaviour(behaviour);
        }

        /// <summary>
        /// Termine la construction du schema de base de la BD.
        /// </summary>
        /// <remarks>Le schéma de la BD doit préalablement avoir été obtenu. GetColumns() et GetForeignKeys()</remarks>
        internal void FinalizeSchema()
        {
            GenerateDerivativeTables();
        }

        private void GenerateCommands()
        {
            foreach (var server in this)
            {
                foreach (var database in server.Value)
                {
                    foreach (var schema in database.Value)
                    {
                        foreach (var table in schema.Value)
                        {
                            var sbInsert = new StringBuilder("INSERT INTO ");
                            var sbSelect = new StringBuilder("SELECT ");

                            sbInsert.Append(database.Key);
                            if (!string.IsNullOrEmpty(schema.Key))
                                sbInsert.Append(".\"").Append(schema.Key).Append('"');
                            sbInsert.Append(".\"")
                                         .Append(table.Name)
                                         .Append("\" (");

                            //Nom des colonnes
                            var nbCols = table.ColumnsDefinition.Count;
                            for (var j = 0; j < nbCols; j++)
                            {
                                //Select
                                sbSelect.Append('"').Append(table.ColumnsDefinition[j].Name).Append('"');
                                if (j < nbCols - 1) sbSelect.Append(",");

                                //Insert
                                if (!table.ColumnsDefinition[j].IsAutoIncrement)
                                {
                                    sbInsert.Append('"').Append(table.ColumnsDefinition[j].Name).Append('"');
                                    if (j < nbCols - 1) sbInsert.Append(",");
                                }
                            }
                            sbInsert.Append(") VALUES(");

                            //Valeur des colonnes Insert
                            for (var j = 0; j < nbCols; j++)
                            {
                                if (!table.ColumnsDefinition[j].IsAutoIncrement)
                                {
                                    sbInsert.Append("@").Append(table.ColumnsDefinition[j].Name);
                                    if (j < nbCols - 1)
                                        sbInsert.Append(",");
                                }
                            }
                            sbInsert.Append(");");

                            //Finalisation du select
                            sbSelect.Append(" FROM \"")
                                    .Append(database.Key)
                                    .Append('"');
                            if (!string.IsNullOrEmpty(schema.Key))
                                sbSelect.Append(".\"").Append(schema.Key).Append('"');
                            sbSelect.Append(".\"")
                                    .Append(table.Name)
                                    .Append("\"");

                            table.InsertCommand = sbInsert.ToString();
                            table.SelectCommand = sbSelect.ToString();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Génène les tables dérivées depuis les FK.
        /// </summary>
        /// <remarks>La configuration utilisateur des FK doit avoir été fusionnée au container avant la création des tables dérivées.</remarks>
        private void GenerateDerivativeTables()
        {
            foreach (var server in this)
            {
                foreach (var database in server.Value)
                {
                    foreach (var schema in database.Value)
                    {
                        foreach (var table in schema.Value)
                        {
                            //On trouve les dérivées de la table
                            foreach (var databaseDeriv in server.Value)
                            {
                                foreach (var schemaDeriv in databaseDeriv.Value)
                                {
                                    foreach (var tableDeriv in schemaDeriv.Value)
                                    {
                                        foreach (var fk in tableDeriv.ForeignKeys)
                                        {
                                            //Si correspondance
                                            if (fk.ServerIdTo == server.Key && fk.DatabaseTo == database.Key &&
                                                fk.SchemaTo == schema.Key && fk.TableTo == table.Name)
                                            {
                                                //Si non présente
                                                if (!table.DerivativeTables.Any(t => t.ServerId == fk.ServerIdTo && t.Schema == fk.SchemaTo &&
                                                    t.Database == fk.DatabaseTo && t.Table == fk.TableTo))
                                                {
                                                    table.DerivativeTables.Add(new DerivativeTable
                                                    {
                                                        ServerId = server.Key,
                                                        Database = databaseDeriv.Key,
                                                        Schema = schemaDeriv.Key,
                                                        Table = tableDeriv.Name
                                                    });
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
        }

        private void MergeFk(Behaviour behaviour)
        {
            if (behaviour == null)
                return;

            foreach (var server in this)
            {
                var serModifier = behaviour.Modifiers.ServerModifiers.Find(s => s.Id.Equals(server.Key.ToString(), StringComparison.OrdinalIgnoreCase));
                if (serModifier != null)
                {
                    foreach (var database in server.Value)
                    {
                        var dbModifier = serModifier.Databases.Find(d => d.Name.Equals(database.Key, StringComparison.OrdinalIgnoreCase));
                        if (dbModifier != null)
                        {
                            foreach (var schema in database.Value)
                            {
                                var scheModifier = dbModifier.Schemas.Find(s => s.Name.Equals( schema.Key, StringComparison.OrdinalIgnoreCase));
                                if (scheModifier != null)
                                    MergeFkModifierSchema(schema.Value, scheModifier.Tables);
                            }
                        }
                    }
                }
            }
        }

        private void MergeFkModifierSchema(SchemaMetadata schemaMetadata, List<TableModifier> tablesModifier)
        {
            foreach (var table in schemaMetadata)
            {
                var tblModifier = tablesModifier.Find(t => t.Name.Equals(table.Name, StringComparison.OrdinalIgnoreCase));
                if (tblModifier != null)
                {
                    //On affecte les changements de la configuration

                    //On supprime les clefs
                    foreach (var colConfig in tblModifier.ForeignKeys.ForeignKeyRemove.Columns)
                    {
                        for (var j = 0; j < table.ForeignKeys.Count(); j++)
                        {
                            var fk = table.ForeignKeys[j];

                            for (var i = 0; i < fk.Columns.Count(); i++)
                            {
                                if (fk.Columns[i].NameFrom.Equals(colConfig.Name, StringComparison.OrdinalIgnoreCase))
                                {
                                    fk.Columns.RemoveAt(i);
                                    i--;

                                    if (fk.Columns.Count == 0)
                                    {
                                        table.ForeignKeys.RemoveAt(j);
                                        j--;
                                    }
                                }
                            }
                        }
                    }

                    //On ajoute les clefs
                    foreach (var fkModifier in tblModifier.ForeignKeys.ForeignKeyAdd)
                    {
                        var newFk = new ForeignKey
                        {
                            ServerIdTo = Int16.Parse(fkModifier.ServerId),
                            DatabaseTo = fkModifier.Database,
                            SchemaTo = fkModifier.Schema,
                            TableTo = fkModifier.Table,
                            Columns = (from fk in fkModifier.Columns select new ForeignKeyColumn { NameFrom = fk.NameFrom, NameTo = fk.NameTo }).ToList()
                        };

                        table.ForeignKeys.Add(newFk);
                    }
                }
            }
        }

        private void MergeBehaviour(Behaviour behaviour)
        {
            if (behaviour == null)
                return;

            foreach (var server in this)
            {
                var serModifier = behaviour.Modifiers.ServerModifiers.Find(s => s.Id.Equals(server.Key.ToString(), StringComparison.OrdinalIgnoreCase));
                if (serModifier != null)
                {
                    foreach (var database in server.Value)
                    {
                        var dbModifier = serModifier.Databases.Find(d => d.Name.Equals(database.Key, StringComparison.OrdinalIgnoreCase));
                        if (dbModifier != null)
                        {
                            foreach (var schema in database.Value)
                            {
                                var scheModifier = dbModifier.Schemas.Find(s => s.Name.Equals(schema.Key, StringComparison.OrdinalIgnoreCase));
                                if (scheModifier != null)
                                {
                                    foreach (var table in schema.Value)
                                    {
                                        var tblModifier = scheModifier.Tables.Find(t => t.Name.Equals(table.Name, StringComparison.OrdinalIgnoreCase));
                                        if (tblModifier != null)
                                        {
                                            //On affecte les changements de la configuration
                                            table.IsStatic = tblModifier.IsStatic;

                                            //Derivative tables
                                            var globalAccess = tblModifier.DerativeTables.GlobalAccess;
                                            var globalCascade = tblModifier.DerativeTables.GlobalCascade;

                                            foreach (var derivTbl in table.DerivativeTables)
                                            {
                                                var derivTblModifier = tblModifier.DerativeTables.DerativeSubTables
                                                                              .FirstOrDefault(t => t.ServerId.Equals( derivTbl.ServerId.ToString(), StringComparison.OrdinalIgnoreCase) &&
                                                                                              t.Database.Equals( derivTbl.Database, StringComparison.OrdinalIgnoreCase) &&
                                                                                              t.Schema.Equals(derivTbl.Schema, StringComparison.OrdinalIgnoreCase) &&
                                                                                              t.Table.Equals( derivTbl.Table, StringComparison.OrdinalIgnoreCase));
                                                if (derivTblModifier != null)
                                                {
                                                    derivTbl.Access = derivTblModifier.Access;
                                                    derivTbl.Cascade = derivTblModifier.Cascade;
                                                }
                                                else
                                                {
                                                    derivTbl.Access = globalAccess;
                                                    derivTbl.Cascade = globalCascade;
                                                }
                                            }

                                            //Data builder
                                            foreach (var builderCol in tblModifier.DataBuilders)
                                            {
                                                var col = table.ColumnsDefinition.FirstOrDefault(c => c.Name.Equals(builderCol.Name, StringComparison.OrdinalIgnoreCase));
                                                if (col != null)
                                                    col.BuilderName = builderCol.BuilderName;
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

        public void Serialize(Stream stream, FastAccessList<object> referenceTracking = null)
        {
            Serialize(new BinaryWriter(stream, Encoding.UTF8, true), referenceTracking);
        }

        public static AppMetadata Deserialize(Stream stream, FastAccessList<object> referenceTracking = null)
        {
            return Deserialize(new BinaryReader(stream, Encoding.UTF8, true), referenceTracking);
        }

        public void Serialize(BinaryWriter output, FastAccessList<object> referenceTracking = null)
        {
            output.Write(Count);
            foreach (var server in this)
            {
                output.Write(server.Key);
                output.Write(server.Value.Count);
                foreach (var database in server.Value)
                {
                    output.Write(database.Key);
                    output.Write(database.Value.Count);
                    foreach (var schema in database.Value)
                    {
                        var nbRows = schema.Value.Count;
                        output.Write(schema.Key);
                        output.Write(nbRows);
                        foreach (var table in schema.Value)
                        {
                            if (referenceTracking == null)
                                table.Serialize(output);
                            else
                            {
                                var id = referenceTracking.TryAdd(table);
                                output.Write(id);
                            }
                        }
                    }
                }
            }
        }

        public static AppMetadata Deserialize(BinaryReader stream, FastAccessList<object> referenceTracking = null)
        {
            var cTables = new AppMetadata();

            var nbServers = stream.ReadInt32();
            for (var n = 0; n < nbServers; n++)
            {
                var serverId = stream.ReadInt16();
                cTables.Add(serverId, new ServerMetadata());

                var nbDatabases = stream.ReadInt32();
                for (var j = 0; j < nbDatabases; j++)
                {
                    var database = stream.ReadString();
                    cTables[serverId].Add(database, new DatabaseMetadata());

                    var nbSchemas = stream.ReadInt32();
                    for (var k = 0; k < nbSchemas; k++)
                    {
                        var schemaMetadata = new SchemaMetadata();
                        var schema = stream.ReadString();

                        var nbTablesFrom = stream.ReadInt32();
                        for (var l = 0; l < nbTablesFrom; l++)
                        {
                            TableMetadata table = null;

                            if (referenceTracking == null)
                                table = TableMetadata.Deserialize(stream);
                            else
                            {
                                var id = stream.ReadInt32();
                                table = (TableMetadata)referenceTracking[id];
                            }
                            schemaMetadata.Add(table);
                        }

                        cTables[serverId][database].Add(schema, schemaMetadata);
                    }
                }
            }
            return cTables;
        }
    }
}