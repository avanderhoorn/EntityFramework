// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.TestUtilities
{
    public class FakeRelationalCommandBuilder : IRelationalCommandBuilder
    {
        public virtual IRelationalCommandBuilder AppendLine() => this;

        public virtual IRelationalCommandBuilder Append([NotNull] object o) => this;

        public virtual IRelationalCommandBuilder AppendLine([NotNull] object o) => this;

        public virtual IRelationalCommandBuilder AppendLines([NotNull] object o) => this;

        public virtual IRelationalCommandBuilder AddParameter(
            [NotNull] string name,
            [CanBeNull] object value) => this;

        public virtual IRelationalCommandBuilder AddParameter(
            [NotNull] string name,
            [CanBeNull] object value,
            [NotNull] Type type) => this;

        public virtual IRelationalCommandBuilder AddParameter(
            [NotNull] string name,
            [CanBeNull] object value,
            [NotNull] IProperty property) => this;

        public virtual IRelationalCommand BuildRelationalCommand() => null;

        public virtual IDisposable Indent() => null;

        public virtual int Length { get; }

        public virtual IRelationalCommandBuilder IncrementIndent() => this;

        public virtual IRelationalCommandBuilder DecrementIndent() => this;
    }
}
