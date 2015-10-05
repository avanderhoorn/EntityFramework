// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Scaffolding.Model
{
    public class Index
    {
        public virtual string Name { get; [param: NotNull] set; }
        public virtual string TableName { get; [param: NotNull] set; }
        public virtual List<string> Columns { get; [param: NotNull] set; } = new List<string>();

        //optional
        public virtual string SchemaName { get; [param: CanBeNull] set; }
        public virtual bool IsUnique { get; [param: CanBeNull] set; }
        public virtual string CreateStatement { get; [param: CanBeNull] set; }
    }
}
