// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Scaffolding.Model
{
    public class ForeignKey
    {
        public virtual string TableName { get; [param: NotNull] set; }
        public virtual string PrincipalTableName { get; [param: NotNull] set; }
        public virtual List<string> From { get; [param: NotNull] set; } = new List<string>();
        public virtual List<string> To { get; [param: NotNull] set; } = new List<string>();

        //optional
        public virtual string Name { get; [param: CanBeNull] set; }
        public virtual string SchemaName { get; [param: CanBeNull] set; }

        // TODO foreign key triggers
        //public virtual string OnDelete { get; [param: NotNull] set; }

        // TODO https://github.com/aspnet/EntityFramework/issues/333
        //public virtual string OnUpdate { get; [param: NotNull] set; }

    }
}
