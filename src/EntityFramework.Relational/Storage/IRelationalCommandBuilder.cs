// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Storage
{
    public interface IRelationalCommandBuilder
    {
        IRelationalCommandBuilder AppendLine();

        IRelationalCommandBuilder Append([NotNull] object o);

        IRelationalCommandBuilder AppendLine([NotNull] object o);

        IRelationalCommandBuilder AppendLines([NotNull] object o);

        IRelationalCommandBuilder AddParameter(
            [NotNull] string name,
            [CanBeNull] object value);

        IRelationalCommandBuilder AddParameter(
            [NotNull] string name,
            [CanBeNull] object value,
            [NotNull] Type type);

        IRelationalCommandBuilder AddParameter(
            [NotNull] string name,
            [CanBeNull] object value,
            [NotNull] IProperty property);

        IRelationalCommand BuildRelationalCommand();

        IDisposable Indent();

        int Length { get; }

        IRelationalCommandBuilder IncrementIndent();

        IRelationalCommandBuilder DecrementIndent();
    }
}
