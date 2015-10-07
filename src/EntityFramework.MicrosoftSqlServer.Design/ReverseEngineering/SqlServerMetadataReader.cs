// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Relational.Design;
using Microsoft.Data.Entity.Relational.Design.Model;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Internal;

namespace Microsoft.Data.Entity.SqlServer.Design.ReverseEngineering
{
    public class SqlServerMetadataReader : IMetadataReader
    {
        public Database GetDatabaseInfo(string connectionString)
            => GetDatabaseInfo(connectionString, TableSelectionSet.InclusiveAll);

        public Database GetDatabaseInfo(string connectionString, TableSelectionSet tableSelectionSet)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                return new Database
                {
                    Name = connection.Database,
                    Tables = GetTables(connection, tableSelectionSet),
                    Columns = GetColumns(connection, tableSelectionSet),
                    Indexes = GetIndexes(connection, tableSelectionSet),
                    ForeignKeys = GetForeignKeys(connection, tableSelectionSet)
                };
            }
        }

        private List<Table> GetTables(SqlConnection connection, TableSelectionSet tableSelectionSet)
        {
            var command = connection.CreateCommand();
            command.CommandText = "SELECT schema_name(t.schema_id) as [schema], t.name FROM sys.tables AS t " +
                                  $"WHERE t.name <> '{HistoryRepository.DefaultTableName}'";
            var tables = new List<Table>();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var table = new Table
                    {
                        SchemaName = reader.GetString(0),
                        Name = reader.GetString(1)
                    };

                    if (tableSelectionSet.Allows(table.SchemaName, table.Name))
                    {
                        tables.Add(table);
                    }
                }
            }
            return tables;
        }

        private List<Column> GetColumns(SqlConnection connection, TableSelectionSet tableSelectionSet)
        {
            var command = connection.CreateCommand();
            command.CommandText = @"SELECT DISTINCT 
    schema_name(t.schema_id) as [schema], 
    t.name as [table], 
    type_name(c.user_type_id) as [typename],
    c.name as [column_name], 
    c.column_id as [ordinal],
    c.is_nullable as [nullable],
    i.is_primary_key,
	object_definition(c.default_object_id) as [default_sql],
    CAST(c.precision as int) AS [precision],
    CAST(c.scale as int) AS [scale],
    CAST(c.max_length as int) AS [max_length],
    c.is_identity
FROM sys.indexes AS i
	INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
	RIGHT JOIN sys.columns c ON ic.object_id = c.object_id AND c.column_id = ic.column_id
JOIN sys.tables AS t ON t.object_id = c.object_id
WHERE t.name <> '" + HistoryRepository.DefaultTableName + "'";

            var columns = new List<Column>();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var schemaName = reader.GetString(0);
                    var tableName = reader.GetString(1);
                    if (!tableSelectionSet.Allows(schemaName, tableName))
                    {
                        continue;
                    }

                    var dataTypeName = reader.GetString(2);
                    var nullable = reader.GetBoolean(5);

                    var precision = reader.GetInt32(8);
                    var scale = reader.GetInt32(9);
                    var maxLength = reader.GetInt32(10);

                    if(dataTypeName == "nvarchar" || dataTypeName =="nchar")
                    {
                        maxLength /= 2;
                    }

                    var column = new Column
                    {
                        SchemaName = schemaName,
                        TableName = tableName,
                        DataType = dataTypeName,
                        Name = reader.GetString(3),
                        Ordinal = reader.GetInt32(4) - 1,
                        IsNullable = nullable,
                        IsPrimaryKey = !reader.IsDBNull(6) && reader.GetBoolean(6),
                        DefaultValue = reader.IsDBNull(7) ? null : reader.GetString(7),
                        Precision = (precision != 0) ? precision : default(int?),
                        Scale = (scale != 0) ? scale : default(int?),
                        MaxLength = (maxLength > 0) ? maxLength : default(int?),
                        IsIdentity = !reader.IsDBNull(11) && reader.GetBoolean(11)
                    };

                    columns.Add(column);
                }
            }
            return columns;
        }

        private List<Index> GetIndexes(SqlConnection connection, TableSelectionSet tableSelectionSet)
        {
            var command = connection.CreateCommand();
            command.CommandText = @"SELECT 
    i.name AS [index_name],
    schema_name(t.schema_id) as [schema_name],
    t.name AS [table_name],
	i.is_unique,
    c.name AS [column_name]
FROM sys.indexes i
    inner join sys.index_columns ic  ON i.object_id = ic.object_id AND i.index_id = ic.index_id
    inner join sys.columns c ON ic.object_id = c.object_id AND c.column_id = ic.column_id
JOIN sys.tables AS t ON t.object_id = c.object_id
WHERE i.type != 1 AND t.name <> '"+ HistoryRepository.DefaultTableName + @"'
ORDER BY i.name, ic.key_ordinal";

            var indexes = new List<Index>();
            using (var reader = command.ExecuteReader())
            {
                Index index = null;
                while (reader.Read())
                {
                    var indexName = reader.GetString(0);
                    var schemaName = reader.GetString(1);
                    var tableName = reader.GetString(2);

                    if (!tableSelectionSet.Allows(schemaName, tableName))
                    {
                        continue;
                    }

                    if (index == null
                        || index.Name != indexName)
                    {
                        index = new Index
                        {
                            SchemaName = schemaName,
                            TableName = tableName,
                            Name = indexName,
                            IsUnique = reader.GetBoolean(3)
                        };
                        indexes.Add(index);
                    }
                    index.Columns.Add(reader.GetString(4));
                }
            }
            return indexes;
        }

        private List<ForeignKey> GetForeignKeys(SqlConnection connection, TableSelectionSet tableSelectionSet)
        {
            var command = connection.CreateCommand();
            command.CommandText = @"SELECT 
    f.name AS foreign_key_name,
    schema_name(f.schema_id) as [schema_name],
    object_name(f.parent_object_id) AS table_name,
    object_name(f.referenced_object_id) AS referenced_object,
    col_name(fc.parent_object_id, fc.parent_column_id) AS constraint_column_name,
    col_name(fc.referenced_object_id, fc.referenced_column_id) AS referenced_column_name,
    is_disabled,
    delete_referential_action_desc,
    update_referential_action_desc
FROM sys.foreign_keys AS f
INNER JOIN sys.foreign_key_columns AS fc 
   ON f.object_id = fc.constraint_object_id";
            var foreignKeys = new List<ForeignKey>();
            using (var reader = command.ExecuteReader())
            {
                var lastFkName = "";
                ForeignKey fkInfo = null;
                while (reader.Read())
                {
                    var fkName = reader.GetString(0);
                    var schemaName = reader.GetString(1);
                    var tableName = reader.GetString(2);

                    if (!tableSelectionSet.Allows(schemaName, tableName))
                    {
                        continue;
                    }

                    if (fkInfo == null
                        || lastFkName != fkName)
                    {
                        lastFkName = fkName;
                        fkInfo = new ForeignKey
                        {
                            SchemaName = schemaName,
                            TableName = tableName,
                            PrincipalTableName = reader.GetString(3)
                        };
                        foreignKeys.Add(fkInfo);
                    }
                    fkInfo.From.Add(reader.GetString(4));
                    fkInfo.To.Add(reader.GetString(5));
                }
            }
            return foreignKeys;
        }
    }
}
