// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Tests.TestUtilities;
using Microsoft.Data.Entity.TestUtilities.FakeProvider;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.Data.Entity.Storage
{
    public class RelationalCommandTest
    {
        [Fact]
        public void Configures_DbCommand()
        {
            var options = CreateOptions();

            var fakeConnection = new FakeRelationalConnection(options);

            var relationalCommand = new RelationalCommand(
                CreateSensitiveDataLogger(options),
                "CommandText",
                new RelationalParameter[0]);

            relationalCommand.ExecuteNonQuery(fakeConnection);

            Assert.Equal(1, fakeConnection.DbConnections.Count);
            Assert.Equal(1, fakeConnection.DbConnections[0].DbCommands.Count);

            var command = fakeConnection.DbConnections[0].DbCommands[0];

            Assert.Equal("CommandText", command.CommandText);
            Assert.Null(command.Transaction);
            Assert.Equal(FakeDbCommand.DefaultCommandTimeout, command.CommandTimeout);
        }

        [Fact]
        public void Configures_DbCommand_with_transaction()
        {
            var options = CreateOptions();

            var fakeConnection = new FakeRelationalConnection(options);

            var relationalTransaction = fakeConnection.BeginTransaction();

            var relationalCommand = new RelationalCommand(
                CreateSensitiveDataLogger(options),
                "CommandText",
                new RelationalParameter[0]);

            relationalCommand.ExecuteNonQuery(fakeConnection);

            Assert.Equal(1, fakeConnection.DbConnections.Count);
            Assert.Equal(1, fakeConnection.DbConnections[0].DbCommands.Count);

            var command = fakeConnection.DbConnections[0].DbCommands[0];

            Assert.Same(relationalTransaction.GetService(), command.Transaction);
        }

        [Fact]
        public void Configures_DbCommand_with_timeout()
        {
            var optionsExtension = new FakeRelationalOptionsExtension
            {
                ConnectionString = ConnectionString,
                CommandTimeout = 42
            };

            var options = CreateOptions(optionsExtension);

            var fakeConnection = new FakeRelationalConnection(options);

            var relationalCommand = new RelationalCommand(
                CreateSensitiveDataLogger(options),
                "CommandText",
                new RelationalParameter[0]);

            relationalCommand.ExecuteNonQuery(fakeConnection);

            Assert.Equal(1, fakeConnection.DbConnections.Count);
            Assert.Equal(1, fakeConnection.DbConnections[0].DbCommands.Count);

            var command = fakeConnection.DbConnections[0].DbCommands[0];

            Assert.Equal(42, command.CommandTimeout);
        }

        [Fact]
        public void Configures_DbCommand_with_parameters()
        {
            var options = CreateOptions();

            var fakeConnection = new FakeRelationalConnection(options);

            var relationalCommand = new RelationalCommand(
                CreateSensitiveDataLogger(options),
                "CommandText",
                new[]
                {
                    new RelationalParameter("FirstParameter", 17, new RelationalTypeMapping("int", DbType.Int32), false),
                    new RelationalParameter("SecondParameter", 18L,  new RelationalTypeMapping("long", DbType.Int64), true),
                    new RelationalParameter("ThirdParameter", null,  new RelationalTypeMapping("null", FakeDbParameter.DefaultDbType), null)
                });

            relationalCommand.ExecuteNonQuery(fakeConnection);

            Assert.Equal(1, fakeConnection.DbConnections.Count);
            Assert.Equal(1, fakeConnection.DbConnections[0].DbCommands.Count);
            Assert.Equal(3, fakeConnection.DbConnections[0].DbCommands[0].Parameters.Count);

            var parameter = fakeConnection.DbConnections[0].DbCommands[0].Parameters[0];

            Assert.Equal("FirstParameter", parameter.ParameterName);
            Assert.Equal(17, parameter.Value);
            Assert.Equal(ParameterDirection.Input, parameter.Direction);
            Assert.Equal(false, parameter.IsNullable);
            Assert.Equal(DbType.Int32, parameter.DbType);

            parameter = fakeConnection.DbConnections[0].DbCommands[0].Parameters[1];

            Assert.Equal("SecondParameter", parameter.ParameterName);
            Assert.Equal(18L, parameter.Value);
            Assert.Equal(ParameterDirection.Input, parameter.Direction);
            Assert.Equal(true, parameter.IsNullable);
            Assert.Equal(DbType.Int64, parameter.DbType);

            parameter = fakeConnection.DbConnections[0].DbCommands[0].Parameters[2];

            Assert.Equal("ThirdParameter", parameter.ParameterName);
            Assert.Equal(DBNull.Value, parameter.Value);
            Assert.Equal(ParameterDirection.Input, parameter.Direction);
            Assert.Equal(FakeDbParameter.DefaultDbType, parameter.DbType);
        }

        public static TheoryData CommandActions
            => new TheoryData<Delegate>
                {
                    { new Action<RelationalCommand, IRelationalConnection>( (command, connection) => command.ExecuteNonQuery(connection)) },
                    { new Action<RelationalCommand, IRelationalConnection>( (command, connection) => command.ExecuteScalar(connection)) },
                    { new Action<RelationalCommand, IRelationalConnection>( (command, connection) => command.ExecuteReader(connection)) }
                };

        [Theory]
        [MemberData(nameof(CommandActions))]
        public void Logs_commands_without_parameter_values(Delegate commandDelegate)
        {
            var options = CreateOptions();

            var fakeConnection = new FakeRelationalConnection(options);

            var log = new List<Tuple<LogLevel, string>>();

            var relationalCommand = new RelationalCommand(
                CreateSensitiveDataLogger(options, log),
                "Command Text",
                new[]
                {
                    new RelationalParameter("FirstParameter", 17, new RelationalTypeMapping("int", DbType.Int32), false)
                });

            ((Action<RelationalCommand, IRelationalConnection>)commandDelegate)(relationalCommand, fakeConnection);

            Assert.Equal(1, log.Count);
            Assert.Equal(LogLevel.Information, log[0].Item1);
            Assert.Equal(
                @"Executing DbCommand: [Parameters=[FirstParameter='?'], CommandType='0', CommandTimeout='30']

Command Text
",
                log[0].Item2);
        }

        [Theory]
        [MemberData(nameof(CommandActions))]
        public void Logs_commands_parameter_values(Delegate commandDelegate)
        {
            var optionsExtension = new FakeRelationalOptionsExtension
            {
                ConnectionString = ConnectionString,
                LogSqlParameterValues = true,
                LogSqlParameterValuesWarned = false
            };

            var options = CreateOptions(optionsExtension);

            var fakeConnection = new FakeRelationalConnection(options);

            var log = new List<Tuple<LogLevel, string>>();

            var relationalCommand = new RelationalCommand(
                CreateSensitiveDataLogger(options, log),
                "Command Text",
                new[]
                {
                    new RelationalParameter("FirstParameter", 17, new RelationalTypeMapping("int", DbType.Int32), false)
                });

            ((Action<RelationalCommand, IRelationalConnection>)commandDelegate)(relationalCommand, fakeConnection);

            Assert.Equal(2, log.Count);
            Assert.Equal(LogLevel.Warning, log[0].Item1);
            Assert.Equal(
@"SQL parameter value logging is enabled. As SQL parameter values may include sensitive application data, this mode should only be enabled during development.",
                log[0].Item2);

            Assert.Equal(LogLevel.Information, log[1].Item1);
            Assert.Equal(
                @"Executing DbCommand: [Parameters=[FirstParameter='17'], CommandType='0', CommandTimeout='30']

Command Text
",
                log[1].Item2);
        }

        [Theory]
        [MemberData(nameof(CommandActions))]
        public void Logs_commands_parameter_values_and_warnings(Delegate commandDelegate)
        {
            var optionsExtension = new FakeRelationalOptionsExtension
            {
                ConnectionString = ConnectionString,
                LogSqlParameterValues = true
            };

            var options = CreateOptions(optionsExtension);

            var fakeConnection = new FakeRelationalConnection(options);

            var log = new List<Tuple<LogLevel, string>>();

            var relationalCommand = new RelationalCommand(
                CreateSensitiveDataLogger(options, log),
                "Command Text",
                new[]
                {
                    new RelationalParameter("FirstParameter", 17, new RelationalTypeMapping("int", DbType.Int32), false)
                });

            ((Action<RelationalCommand, IRelationalConnection>)commandDelegate)(relationalCommand, fakeConnection);

            Assert.Equal(2, log.Count);
            Assert.Equal(LogLevel.Warning, log[0].Item1);
            Assert.Equal(
@"SQL parameter value logging is enabled. As SQL parameter values may include sensitive application data, this mode should only be enabled during development.",
                log[0].Item2);

            Assert.Equal(LogLevel.Information, log[1].Item1);
            Assert.Equal(
                @"Executing DbCommand: [Parameters=[FirstParameter='17'], CommandType='0', CommandTimeout='30']

Command Text
",
                log[1].Item2);
        }

        public static TheoryData AsyncCommandActions
            => new TheoryData<Delegate>
        {
                    { new Func<RelationalCommand, IRelationalConnection, Task>( (command, connection) => command.ExecuteNonQueryAsync(connection)) },
                    { new Func<RelationalCommand, IRelationalConnection, Task>( (command, connection) => command.ExecuteScalarAsync(connection)) },
                    { new Func<RelationalCommand, IRelationalConnection, Task>( (command, connection) => command.ExecuteReaderAsync(connection)) }
        };

        [Theory]
        [MemberData(nameof(AsyncCommandActions))]
        public async Task Logs_async_commands_without_parameter_values(Delegate commandDelegate)
        {
            var options = CreateOptions();

            var fakeConnection = new FakeRelationalConnection(options);

            var log = new List<Tuple<LogLevel, string>>();

            var relationalCommand = new RelationalCommand(
                CreateSensitiveDataLogger(options, log),
                "Command Text",
                new[]
                {
                    new RelationalParameter("FirstParameter", 17, new RelationalTypeMapping("int", DbType.Int32), false)
                });

            await ((Func<RelationalCommand, IRelationalConnection, Task>)commandDelegate)(relationalCommand, fakeConnection);

            Assert.Equal(1, log.Count);
            Assert.Equal(LogLevel.Information, log[0].Item1);
            Assert.Equal(
                @"Executing DbCommand: [Parameters=[FirstParameter='?'], CommandType='0', CommandTimeout='30']

Command Text
",
                log[0].Item2);
        }

        [Theory]
        [MemberData(nameof(AsyncCommandActions))]
        public async Task Logs_async_commands_parameter_values(Delegate commandDelegate)
        {
            var optionsExtension = new FakeRelationalOptionsExtension
            {
                ConnectionString = ConnectionString,
                LogSqlParameterValues = true,
                LogSqlParameterValuesWarned = false
            };

            var options = CreateOptions(optionsExtension);

            var fakeConnection = new FakeRelationalConnection(options);

            var log = new List<Tuple<LogLevel, string>>();

            var relationalCommand = new RelationalCommand(
                CreateSensitiveDataLogger(options, log),
                "Command Text",
                new[]
                {
                    new RelationalParameter("FirstParameter", 17, new RelationalTypeMapping("int", DbType.Int32), false)
                });

            await ((Func<RelationalCommand, IRelationalConnection, Task>)commandDelegate)(relationalCommand, fakeConnection);

            Assert.Equal(2, log.Count);
            Assert.Equal(LogLevel.Warning, log[0].Item1);
            Assert.Equal(
@"SQL parameter value logging is enabled. As SQL parameter values may include sensitive application data, this mode should only be enabled during development.",
                log[0].Item2);

            Assert.Equal(LogLevel.Information, log[1].Item1);
            Assert.Equal(
                @"Executing DbCommand: [Parameters=[FirstParameter='17'], CommandType='0', CommandTimeout='30']

Command Text
",
                log[1].Item2);
        }

        [Theory]
        [MemberData(nameof(AsyncCommandActions))]
        public async Task Logs_async_commands_parameter_values_and_warnings(Delegate commandDelegate)
        {
            var optionsExtension = new FakeRelationalOptionsExtension
            {
                ConnectionString = ConnectionString,
                LogSqlParameterValues = true
            };

            var options = CreateOptions(optionsExtension);

            var fakeConnection = new FakeRelationalConnection(options);

            var log = new List<Tuple<LogLevel, string>>();

            var relationalCommand = new RelationalCommand(
                CreateSensitiveDataLogger(options, log),
                "Command Text",
                new[]
                {
                    new RelationalParameter("FirstParameter", 17, new RelationalTypeMapping("int", DbType.Int32), false)
                });

            await ((Func<RelationalCommand, IRelationalConnection, Task>)commandDelegate)(relationalCommand, fakeConnection);

            Assert.Equal(2, log.Count);
            Assert.Equal(LogLevel.Warning, log[0].Item1);
            Assert.Equal(
@"SQL parameter value logging is enabled. As SQL parameter values may include sensitive application data, this mode should only be enabled during development.",
                log[0].Item2);

            Assert.Equal(LogLevel.Information, log[1].Item1);
            Assert.Equal(
                @"Executing DbCommand: [Parameters=[FirstParameter='17'], CommandType='0', CommandTimeout='30']

Command Text
",
                log[1].Item2);
        }

        [Fact]
        public void Can_ExecuteNonQuery()
        {
            var executeNonQueryCount = 0;
            var disposeCount = -1;

            var fakeDbConnection = new FakeDbConnection(
                ConnectionString,
                new FakeCommandExecutor(
                    executeNonQuery: c =>
                    {
                        executeNonQueryCount++;
                        disposeCount = c.DisposeCount;
                        return 1;
                    }));

            var optionsExtension = new FakeRelationalOptionsExtension { Connection = fakeDbConnection };

            var options = CreateOptions(optionsExtension);

            var fakeConnection = new FakeRelationalConnection(options);

            var relationalCommand = new RelationalCommand(
                CreateSensitiveDataLogger(options),
                "ExecuteNonQuery Command",
                new RelationalParameter[0]);

            relationalCommand.ExecuteNonQuery(fakeConnection);

            // Durring command execution
            Assert.Equal(1, executeNonQueryCount);
            Assert.Equal(0, disposeCount);

            // After command execution
            Assert.Equal(1, fakeDbConnection.DbCommands[0].DisposeCount);
        }

        [Fact]
        public virtual async Task Can_ExecuteNonQueryAsync()
        {
            var executeNonQueryCount = 0;
            var disposeCount = -1;

            var fakeDbConnection = new FakeDbConnection(
                ConnectionString,
                new FakeCommandExecutor(
                    executeNonQueryAsync: (c, ct) =>
                    {
                        executeNonQueryCount++;
                        disposeCount = c.DisposeCount;
                        return Task.FromResult(1);
                    }));

            var optionsExtension = new FakeRelationalOptionsExtension { Connection = fakeDbConnection };

            var options = CreateOptions(optionsExtension);

            var fakeConnection = new FakeRelationalConnection(options);

            var relationalCommand = new RelationalCommand(
                CreateSensitiveDataLogger(options),
                "ExecuteNonQuery Command",
                new RelationalParameter[0]);

            await relationalCommand.ExecuteNonQueryAsync(fakeConnection);

            // Durring command execution
            Assert.Equal(1, executeNonQueryCount);
            Assert.Equal(0, disposeCount);

            // After command execution
            Assert.Equal(1, fakeDbConnection.DbCommands[0].DisposeCount);
        }

        [Fact]
        public void Can_ExecuteScalar()
        {
            var executeScalarCount = 0;
            var disposeCount = -1;

            var fakeDbConnection = new FakeDbConnection(
                ConnectionString,
                new FakeCommandExecutor(
                    executeScalar: c =>
                    {
                        executeScalarCount++;
                        disposeCount = c.DisposeCount;
                        return "ExecuteScalar Result";
                    }));

            var optionsExtension = new FakeRelationalOptionsExtension { Connection = fakeDbConnection };

            var options = CreateOptions(optionsExtension);

            var fakeConnection = new FakeRelationalConnection(options);

            var relationalCommand = new RelationalCommand(
                CreateSensitiveDataLogger(options),
                "ExecuteScalar Command",
                new RelationalParameter[0]);

            var result = (string)relationalCommand.ExecuteScalar(fakeConnection);

            Assert.Equal("ExecuteScalar Result", result);

            // Durring command execution
            Assert.Equal(1, executeScalarCount);
            Assert.Equal(0, disposeCount);

            // After command execution
            Assert.Equal(1, fakeDbConnection.DbCommands[0].DisposeCount);
        }

        [Fact]
        public async Task Can_ExecuteScalarAsync()
        {
            var executeScalarCount = 0;
            var disposeCount = -1;

            var fakeDbConnection = new FakeDbConnection(
                ConnectionString,
                new FakeCommandExecutor(
                    executeScalarAsync: (c, ct) =>
                    {
                        executeScalarCount++;
                        disposeCount = c.DisposeCount;
                        return Task.FromResult<object>("ExecuteScalar Result");
                    }));

            var optionsExtension = new FakeRelationalOptionsExtension { Connection = fakeDbConnection };

            var options = CreateOptions(optionsExtension);

            var fakeConnection = new FakeRelationalConnection(options);

            var relationalCommand = new RelationalCommand(
                CreateSensitiveDataLogger(options),
                "ExecuteScalar Command",
                new RelationalParameter[0]);

            var result = (string)await relationalCommand.ExecuteScalarAsync(fakeConnection);

            Assert.Equal("ExecuteScalar Result", result);

            // Durring command execution
            Assert.Equal(1, executeScalarCount);
            Assert.Equal(0, disposeCount);

            // After command execution
            Assert.Equal(1, fakeDbConnection.DbCommands[0].DisposeCount);
        }

        [Fact]
        public void Can_ExecuteReader()
        {
            var executeReaderCount = 0;
            var disposeCount = -1;

            var dbDataReader = new FakeDbDataReader();

            var fakeDbConnection = new FakeDbConnection(
                ConnectionString,
                new FakeCommandExecutor(
                    executeReader: (c, b) =>
                    {
                        executeReaderCount++;
                        disposeCount = c.DisposeCount;
                        return dbDataReader;
                    }));

            var optionsExtension = new FakeRelationalOptionsExtension { Connection = fakeDbConnection };

            var options = CreateOptions(optionsExtension);

            var fakeConnection = new FakeRelationalConnection(options);

            var relationalCommand = new RelationalCommand(
                CreateSensitiveDataLogger(options),
                "ExecuteReader Command",
                new RelationalParameter[0]);

            var result = relationalCommand.ExecuteReader(fakeConnection);

            Assert.Same(dbDataReader, result.DbDataReader);

            // Durring command execution
            Assert.Equal(1, executeReaderCount);
            Assert.Equal(0, disposeCount);

            // After command execution
            Assert.Equal(0, dbDataReader.DisposeCount);
            Assert.Equal(0, fakeDbConnection.DbCommands[0].DisposeCount);

            // After reader dispose
            result.Dispose();
            Assert.Equal(1, dbDataReader.DisposeCount);
            Assert.Equal(1, fakeDbConnection.DbCommands[0].DisposeCount);
        }

        [Fact]
        public async Task Can_ExecuteReaderAsync()
        {
            var executeReaderCount = 0;
            var disposeCount = -1;

            var dbDataReader = new FakeDbDataReader();

            var fakeDbConnection = new FakeDbConnection(
                ConnectionString,
                new FakeCommandExecutor(
                    executeReaderAsync: (c, b, ct) =>
                    {
                        executeReaderCount++;
                        disposeCount = c.DisposeCount;
                        return Task.FromResult<DbDataReader>(dbDataReader);
                    }));

            var optionsExtension = new FakeRelationalOptionsExtension { Connection = fakeDbConnection };

            var options = CreateOptions(optionsExtension);

            var fakeConnection = new FakeRelationalConnection(options);

            var relationalCommand = new RelationalCommand(
                CreateSensitiveDataLogger(options),
                "ExecuteReader Command",
                new RelationalParameter[0]);

            var result = await relationalCommand.ExecuteReaderAsync(fakeConnection);

            Assert.Same(dbDataReader, result.DbDataReader);

            // Durring command execution
            Assert.Equal(1, executeReaderCount);
            Assert.Equal(0, disposeCount);

            // After command execution
            Assert.Equal(0, dbDataReader.DisposeCount);
            Assert.Equal(0, fakeDbConnection.DbCommands[0].DisposeCount);

            // After reader dispose
            result.Dispose();
            Assert.Equal(1, dbDataReader.DisposeCount);
            Assert.Equal(1, fakeDbConnection.DbCommands[0].DisposeCount);
        }



        [Fact]
        public void ExecuteReader_disposes_command_on_exception()
        {
            var fakeDbConnection = new FakeDbConnection(
                ConnectionString,
                new FakeCommandExecutor(
                    executeReader: (c, b) =>
                    {
                        throw new DbUpdateException("ExecuteReader Exception", new InvalidOperationException());
                    }));

            var optionsExtension = new FakeRelationalOptionsExtension { Connection = fakeDbConnection };

            var options = CreateOptions(optionsExtension);

            var fakeConnection = new FakeRelationalConnection(options);

            var relationalCommand = new RelationalCommand(
                CreateSensitiveDataLogger(options),
                "ExecuteReader Command",
                new RelationalParameter[0]);

            Assert.Throws<DbUpdateException>(() => relationalCommand.ExecuteReader(fakeConnection));
            Assert.Equal(1, fakeDbConnection.DbCommands[0].DisposeCount);
        }

        [Fact]
        public async Task ExecuteReaderAsync_disposes_command_on_exception()
        {
            var fakeDbConnection = new FakeDbConnection(
                ConnectionString,
                new FakeCommandExecutor(
                    executeReaderAsync: (c, b, ct) =>
                    {
                        throw new DbUpdateException("ExecuteReader Exception", new InvalidOperationException());
                    }));

            var optionsExtension = new FakeRelationalOptionsExtension { Connection = fakeDbConnection };

            var options = CreateOptions(optionsExtension);

            var fakeConnection = new FakeRelationalConnection(options);

            var relationalCommand = new RelationalCommand(
                CreateSensitiveDataLogger(options),
                "ExecuteReader Command",
                new RelationalParameter[0]);

            await Assert.ThrowsAsync<DbUpdateException>(() => relationalCommand.ExecuteReaderAsync(fakeConnection));
            Assert.Equal(1, fakeDbConnection.DbCommands[0].DisposeCount);
        }

        private const string ConnectionString = "Fake Connection String";

        public static IDbContextOptions CreateOptions(FakeRelationalOptionsExtension optionsExtension = null)
        {
            var optionsBuilder = new DbContextOptionsBuilder();

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder)
                .AddOrUpdateExtension(optionsExtension ?? new FakeRelationalOptionsExtension { ConnectionString = ConnectionString });

            return optionsBuilder.Options;
        }

        private static ISensitiveDataLogger<RelationalCommand> CreateSensitiveDataLogger(
            IDbContextOptions options,
            List<Tuple<LogLevel, string>> logMessages = null)
            => new SensitiveDataLogger<RelationalCommand>(
                new ListLogger<RelationalCommand>(logMessages),
                options);
    }
}
