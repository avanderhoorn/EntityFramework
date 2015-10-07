// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Design;
using Microsoft.Data.Entity.Relational.Design.Model;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Internal;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Sqlite;

namespace Microsoft.Data.Entity.Sqlite.Design.ReverseEngineering
{
    public class SqliteMetadataReader : IMetadataReader
    {
        public virtual Database GetDatabaseInfo([NotNull] string connectionString, [NotNull] TableSelectionSet tableSelectionSet)
        {
            Check.NotEmpty(connectionString, nameof(connectionString));
            Check.NotNull(tableSelectionSet, nameof(tableSelectionSet));

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var databaseInfo = new Database
                {
                    Name = connection.DataSource
                };

                var definitions = GetSqliteMaster(connection, databaseInfo, tableSelectionSet);

                GetColumns(connection, databaseInfo);
                GetIndexes(connection, databaseInfo, definitions);

                foreach (var table in databaseInfo.Tables)
                {
                    SqliteDmlParser.ParseTableDefinition(databaseInfo, table, definitions.Tables[table.Name]);
                }

                GetForeignKeys(connection, databaseInfo);

                return databaseInfo;
            }
        }

        private SqlDefinitions GetSqliteMaster(SqliteConnection connection, Database databaseInfo, TableSelectionSet tableSelectionSet)
        {
            var command = connection.CreateCommand();
            command.CommandText = "SELECT type, name, sql, tbl_name FROM sqlite_master";
            var definitions = new SqlDefinitions();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var type = reader.GetString(0);
                    var name = reader.GetString(1);
                    var sql = reader.GetValue(2) as string; // can be null
                    var tableName = reader.GetString(3);

                    if (type == "table"
                        && name != "sqlite_sequence"
                        && tableSelectionSet.Allows(TableSelection.Any, name))
                    {
                        databaseInfo.Tables.Add(new Table
                        {
                            Name = name
                        });
                        definitions.Tables[name] = sql;
                    }
                    else if (type == "index"
                             && tableSelectionSet.Allows(TableSelection.Any, tableName))
                    {
                        databaseInfo.Indexes.Add(new Index
                        {
                            Name = name,
                            TableName = tableName
                        });

                        definitions.Indexes[name] = sql;
                    }
                }
            }
            return definitions;
        }

        private enum TableInfoColumns
        {
            Cid,
            Name,
            Type,
            NotNull,
            DefaultValue,
            Pk
        }

        private void GetColumns(SqliteConnection connection, Database databaseInfo)
        {
            databaseInfo.Columns = new List<Column>();

            foreach (var table in databaseInfo.Tables)
            {
                var command = connection.CreateCommand();
                command.CommandText = $"PRAGMA table_info(\"{table.Name.Replace("\"", "\"\"")}\");";

                using (var reader = command.ExecuteReader())
                {
                    var ordinal = 0;
                    while (reader.Read())
                    {
                        var isPk = reader.GetBoolean((int)TableInfoColumns.Pk);
                        var typeName = reader.GetString((int)TableInfoColumns.Type);
                        var notNull = isPk || reader.GetBoolean((int)TableInfoColumns.NotNull);

                        var column = new Column
                        {
                            TableName = table.Name,
                            Name = reader.GetString((int)TableInfoColumns.Name),
                            DataType = typeName,
                            IsPrimaryKey = reader.GetBoolean((int)TableInfoColumns.Pk),
                            IsNullable = !notNull,
                            DefaultValue = reader.GetValue((int)TableInfoColumns.DefaultValue) as string,
                            Ordinal = ordinal++
                        };

                        databaseInfo.Columns.Add(column);
                    }
                }
            }
        }

        private enum IndexInfoColumns
        {
            Seqno,
            Cid,
            Name
        }

        private void GetIndexes(SqliteConnection connection, Database databaseInfo, SqlDefinitions definitions)
        {
            foreach (var index in databaseInfo.Indexes)
            {
                var indexInfo = connection.CreateCommand();
                indexInfo.CommandText = $"PRAGMA index_info(\"{index.Name.Replace("\"", "\"\"")}\");";

                index.Columns = new List<string>();
                using (var reader = indexInfo.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var name = reader.GetValue((int)IndexInfoColumns.Name) as string;
                        if (!string.IsNullOrEmpty(name))
                        {
                            index.Columns.Add(name);
                        }

                        var sql = definitions.Indexes[index.Name];

                        if (!string.IsNullOrEmpty(sql))
                        {
                            var uniqueKeyword = sql.IndexOf("UNIQUE", StringComparison.OrdinalIgnoreCase);
                            var indexKeyword = sql.IndexOf("INDEX", StringComparison.OrdinalIgnoreCase);

                            index.IsUnique = uniqueKeyword > 0 && uniqueKeyword < indexKeyword;
                        }
                    }
                }
            }

            databaseInfo.Indexes = databaseInfo.Indexes.Where(i => i.Columns.Count > 0).ToList();
        }

        private enum ForeignKeyList
        {
            Id,
            Seq,
            Table,
            From,
            To,
            OnUpdate,
            OnDelete,
            Match
        }

        private void GetForeignKeys(SqliteConnection connection, Database databaseInfo)
        {
            foreach (var dependentTable in databaseInfo.Tables)
            {
                var fkList = connection.CreateCommand();
                fkList.CommandText = $"PRAGMA foreign_key_list(\"{dependentTable.Name.Replace("\"", "\"\"")}\");";

                var tableForeignKeys = new Dictionary<int, ForeignKey>();

                using (var reader = fkList.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = reader.GetInt32((int)ForeignKeyList.Id);
                        var principalTable = reader.GetString((int)ForeignKeyList.Table);
                        ForeignKey foreignKey;
                        if (!tableForeignKeys.TryGetValue(id, out foreignKey))
                        {
                            foreignKey = new ForeignKey
                            {
                                TableName = dependentTable.Name,
                                PrincipalTableName = principalTable
                            };
                            tableForeignKeys.Add(id, foreignKey);
                        }
                        foreignKey.From.Add(reader.GetString((int)ForeignKeyList.From));
                        foreignKey.To.Add(reader.GetString((int)ForeignKeyList.To));
                    }
                }

                databaseInfo.ForeignKeys.AddRange(tableForeignKeys.Values);
            }
        }

        private class SqlDefinitions
        {
            public readonly Dictionary<string, string> Tables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            public readonly Dictionary<string, string> Indexes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }
}
