// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Storage.Internal
{
    public class SqliteTypeMapper : RelationalTypeMapper
    {
        private readonly RelationalTypeMapping _integer = new RelationalTypeMapping("INTEGER");
        private readonly RelationalTypeMapping _real = new RelationalTypeMapping("REAL");
        private readonly RelationalTypeMapping _blob = new RelationalTypeMapping("BLOB");
        private readonly RelationalTypeMapping _text = new RelationalTypeMapping("TEXT");

        private readonly Dictionary<string, RelationalTypeMapping> _simpleNameMappings;

        private readonly Dictionary<Type, RelationalTypeMapping> _simpleMappings;

        public SqliteTypeMapper()
        {
            _simpleNameMappings
                = new Dictionary<string, RelationalTypeMapping>(StringComparer.OrdinalIgnoreCase);

            _simpleMappings
                = new Dictionary<Type, RelationalTypeMapping>
                {
                    { typeof(string), _text },
                    { typeof(byte[]), _blob },
                    { typeof(bool), _integer },
                    { typeof(byte), _integer },
                    { typeof(char), _integer },
                    { typeof(int), _integer },
                    { typeof(long), _integer },
                    { typeof(sbyte), _integer },
                    { typeof(short), _integer },
                    { typeof(uint), _integer },
                    { typeof(ulong), _integer },
                    { typeof(ushort), _integer },
                    { typeof(DateTime), _text },
                    { typeof(DateTimeOffset), _text },
                    { typeof(TimeSpan), _text },
                    { typeof(decimal), _text },
                    { typeof(double), _real },
                    { typeof(float), _real },
                    { typeof(Guid), _blob }
                };
        }

        protected override string GetColumnType(IProperty property) => property.Sqlite().ColumnType;

        /// <summary>
        ///     Returns a clr type for a SQLite column type. Defaults to typeof(string).
        ///     It uses the same heuristics from
        ///     <see href="https://www.sqlite.org/datatype3.html">"2.1 Determination of Column Affinity"</see>
        /// </summary>
        public override Type FindClrType([CanBeNull] string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return _default;
            }

            Type clrType;
            foreach (var rules in _typeRules)
            {
                clrType = rules(typeName);
                if (clrType != null)
                {
                    return clrType;
                }
            }

            return _default;
        }

        private static readonly Type _default = typeof(string);

        private readonly Func<string, Type>[] _typeRules =
        {
            name => Contains(name, "INT") ? typeof(long) : null,
            name => Contains(name, "CHAR")
                    || Contains(name, "CLOB")
                    || Contains(name, "TEXT") ? typeof(string) : null,
            name => Contains(name, "BLOB") ? typeof(byte[]) : null,
            name => Contains(name, "REAL")
                    || Contains(name, "FLOA")
                    || Contains(name, "DOUB") ? typeof(double) : null
        };

        private static bool Contains(string haystack, string needle)
            => haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;

        protected override IReadOnlyDictionary<Type, RelationalTypeMapping> SimpleMappings
            => _simpleMappings;

        protected override IReadOnlyDictionary<string, RelationalTypeMapping> SimpleNameMappings
            => _simpleNameMappings;
    }
}
