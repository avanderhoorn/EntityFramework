using JetBrains.Annotations;
using Microsoft.Data.Entity.Scaffolding.Model;

namespace Microsoft.Data.Entity.Scaffolding
{
    public interface IMetadataReader
    {
        Database GetDatabaseInfo([NotNull] string connectionString);
        Database GetDatabaseInfo([NotNull] string connectionString, [NotNull] TableSelectionSet tableSelectionSet);
    }
}