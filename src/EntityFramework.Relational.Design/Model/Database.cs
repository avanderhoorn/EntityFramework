// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Scaffolding.Model
{
    public class Database
    {
        public virtual string Name { get; [param: CanBeNull] set; }
        public virtual List<Column> Columns { get; [param: NotNull] set; } = new List<Column>();
        public virtual List<ForeignKey> ForeignKeys { get; [param: NotNull] set; } = new List<ForeignKey>();
        public virtual List<Index> Indexes { get; [param: NotNull] set; } = new List<Index>();
        public virtual List<Table> Tables { get; [param: NotNull] set; } = new List<Table>();
    }
}
