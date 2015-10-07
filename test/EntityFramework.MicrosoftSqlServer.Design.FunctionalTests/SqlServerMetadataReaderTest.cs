// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Relational.Design;
using Microsoft.Data.Entity.Relational.Design.Model;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Internal;
using Microsoft.Data.Entity.SqlServer.Design.ReverseEngineering;
using Microsoft.Data.Entity.SqlServer.FunctionalTests;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Design.FunctionalTests.ReverseEngineering
{
    public class SqlServerMetadataReaderTest
    {
        [Fact]
        public void It_reads_tables()
        {
            var sql = @"
CREATE TABLE [dbo].[Everest] ( id int );
CREATE TABLE [dbo].[Denali] ( id int );";
            var dbInfo = GetDatabaseInfo(sql);

            Assert.Collection(dbInfo.Tables, e =>
                {
                    Assert.Equal("dbo", e.SchemaName);
                    Assert.Equal("Everest", e.Name);
                },
                d =>
                    {
                        Assert.Equal("dbo", d.SchemaName);
                        Assert.Equal("Denali", d.Name);
                    });
        }

        [Fact]
        public void It_reads_foreign_keys()
        {
            var sql = "CREATE TABLE Ranges ( Id INT IDENTITY (1,1) PRIMARY KEY);" +
                      "CREATE TABLE Mountains ( RangeId INT NOT NULL, FOREIGN KEY (RangeId) REFERENCES Ranges(Id) );";
            var dbInfo = GetDatabaseInfo(sql);

            var fk = Assert.Single(dbInfo.ForeignKeys);
            Assert.Equal("dbo", fk.SchemaName);
            Assert.Equal("Mountains", fk.TableName);
            Assert.Equal("Ranges", fk.PrincipalTableName);
            Assert.Equal("RangeId", fk.From.Single());
            Assert.Equal("Id", fk.To.Single());
        }

        [Fact]
        public void It_reads_indexes()
        {
            var sql = "CREATE TABLE Ranges ( Name int UNIQUE, Location int, INDEX loc_idx (Location, Name) );";
            var dbInfo = GetDatabaseInfo(sql);

            Assert.Collection(dbInfo.Indexes,
                index =>
                    {
                        Assert.Equal("loc_idx", index.Name);
                        Assert.False(index.IsUnique);
                        Assert.Equal("dbo", index.SchemaName);
                        Assert.Equal("Ranges", index.TableName);
                        Assert.Equal(new List<string> { "Location", "Name" }, index.Columns);
                    },
                unique =>
                    {
                        Assert.True(unique.IsUnique);
                        Assert.Equal("dbo", unique.SchemaName);
                        Assert.Equal("Ranges", unique.TableName);
                        Assert.Equal("Name", unique.Columns.Single());
                    });
        }

        [Fact]
        public void It_reads_columns()
        {
            var sql = @"
CREATE TABLE [dbo].[Mountains] (
    Id int PRIMARY KEY,
    Name nvarchar(100) NOT NULL,
    Latitude decimal( 5, 2 ) DEFAULT 0.0
);";
            var dbInfo = GetDatabaseInfo(sql);

            Assert.Collection(dbInfo.Columns.OrderBy(c => c.Ordinal),
                id =>
                    {
                        Assert.Equal("dbo", id.SchemaName);
                        Assert.Equal("Mountains", id.TableName);
                        Assert.Equal("Id", id.Name);
                        Assert.Equal("int", id.DataType);
                        Assert.True(id.IsPrimaryKey);
                        Assert.False(id.IsNullable);
                        Assert.Equal(0, id.Ordinal);
                        Assert.Null(id.DefaultValue);
                    },
                name =>
                    {
                        Assert.Equal("dbo", name.SchemaName);
                        Assert.Equal("Mountains", name.TableName);
                        Assert.Equal("Name", name.Name);
                        Assert.Equal("nvarchar", name.DataType);
                        Assert.False(name.IsPrimaryKey);
                        Assert.False(name.IsNullable);
                        Assert.Equal(1, name.Ordinal);
                        Assert.Null(name.DefaultValue);
                        Assert.Equal(100, name.MaxLength);
                    },
                lat =>
                    {
                        Assert.Equal("dbo", lat.SchemaName);
                        Assert.Equal("Mountains", lat.TableName);
                        Assert.Equal("Latitude", lat.Name);
                        Assert.Equal("decimal", lat.DataType);
                        Assert.False(lat.IsPrimaryKey);
                        Assert.True(lat.IsNullable);
                        Assert.Equal(2, lat.Ordinal);
                        Assert.Equal("((0.0))", lat.DefaultValue);
                        Assert.Equal(5, lat.Precision);
                        Assert.Equal(2, lat.Scale);
                    });
        }

        [Theory]
        [InlineData("nvarchar(55)", 55)]
        [InlineData("varchar(341)", 341)]
        [InlineData("nchar(14)", 14)]
        [InlineData("char(89)", 89)]
        [InlineData("varchar(max)", null)]
        public void It_reads_max_length(string type, int? length)
        {
            var sql = "CREATE TABLE [dbo].[Mountains] ( CharColumn " + type + ");";
            var db = GetDatabaseInfo(sql);

            Assert.Equal(length, db.Columns.Single().MaxLength);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void It_reads_identity(bool isIdentity)
        {
            var dbInfo = GetDatabaseInfo(@"CREATE TABLE [dbo].[Mountains] ( Id INT " + (isIdentity ? "IDENTITY(1,1)" : "") + ")");

            Assert.Equal(isIdentity, dbInfo.Columns.Single().IsIdentity.Value);
        }

        [Fact]
        public void It_filters_tables()
        {
            var sql = @"CREATE TABLE [dbo].[K2] ( Id int, A varchar, UNIQUE (A ) );
CREATE TABLE [dbo].[Kilimanjaro] ( Id int,B varchar, UNIQUE (B ), FOREIGN KEY (B) REFERENCES K2 (A) );";

            var selectionSet = new TableSelectionSet();
            selectionSet.AddSelections(new TableSelection
            {
                Table = "Kilimanjaro",
                Exclude = true
            });

            var dbInfo = GetDatabaseInfo(sql, selectionSet);

            Assert.Collection(dbInfo.Tables,
                k2 => { Assert.Equal("K2", k2.Name); });
            Assert.Equal(2, dbInfo.Columns.Count);
            Assert.Equal(1, dbInfo.Indexes.Count);
            Assert.Empty(dbInfo.ForeignKeys);
        }

        public Database GetDatabaseInfo(string createSql, TableSelectionSet selection = null)
        {
            using (var connection = SqlServerTestStore.CreateScratch().Connection)
            {
                var command = connection.CreateCommand();
                command.CommandText = createSql;
                command.ExecuteNonQuery();

                var reader = new SqlServerMetadataReader();

                return reader.GetDatabaseInfo(connection.ConnectionString, selection ?? TableSelectionSet.InclusiveAll);
            }
        }
    }
}
