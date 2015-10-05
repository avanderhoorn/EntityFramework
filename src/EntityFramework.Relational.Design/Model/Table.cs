// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Scaffolding.Model
{
    public class Table
    {
        public virtual string Name { get; [param: NotNull] set; }

        // optional
        public virtual string SchemaName { get; [param: CanBeNull] set; }
        public virtual string CreateStatement { get; [param: CanBeNull] set; }
    }
}
