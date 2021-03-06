// <auto-generated />
namespace Microsoft.EntityFrameworkCore.Internal
{
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Resources;
    using JetBrains.Annotations;

    public static class RelationalDesignStrings
    {
        private static readonly ResourceManager _resourceManager
            = new ResourceManager("Microsoft.EntityFrameworkCore.Relational.Design.Properties.RelationalDesignStrings", typeof(RelationalDesignStrings).GetTypeInfo().Assembly);

        /// <summary>
        /// Could not find type mapping for column '{columnName}' with data type '{dateType}'. Skipping column.
        /// </summary>
        public static string CannotFindTypeMappingForColumn([CanBeNull] object columnName, [CanBeNull] object dateType)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("CannotFindTypeMappingForColumn", "columnName", "dateType"), columnName, dateType);
        }

        /// <summary>
        /// Could not scaffold the foreign key '{foreignKeyName}'. A key for '{columnsList}' was not found in the principal entity type '{principalEntityType}'.
        /// </summary>
        public static string ForeignKeyScaffoldErrorPrincipalKeyNotFound([CanBeNull] object foreignKeyName, [CanBeNull] object columnsList, [CanBeNull] object principalEntityType)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ForeignKeyScaffoldErrorPrincipalKeyNotFound", "foreignKeyName", "columnsList", "principalEntityType"), foreignKeyName, columnsList, principalEntityType);
        }

        /// <summary>
        /// Could not scaffold the foreign key '{foreignKeyName}'. The referenced table could not be found. This most likely occurred because the referenced table was excluded from scaffolding.
        /// </summary>
        public static string ForeignKeyScaffoldErrorPrincipalTableNotFound([CanBeNull] object foreignKeyName)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ForeignKeyScaffoldErrorPrincipalTableNotFound", "foreignKeyName"), foreignKeyName);
        }

        /// <summary>
        /// Could not scaffold the foreign key '{foreignKeyName}'. The referenced table '{principalTableName}' could not be scaffolded.
        /// </summary>
        public static string ForeignKeyScaffoldErrorPrincipalTableScaffoldingError([CanBeNull] object foreignKeyName, [CanBeNull] object principalTableName)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ForeignKeyScaffoldErrorPrincipalTableScaffoldingError", "foreignKeyName", "principalTableName"), foreignKeyName, principalTableName);
        }

        /// <summary>
        /// Could not scaffold the foreign key '{foreignKeyName}'.  The following columns in the foreign key could not be scaffolded: {columnNames}.
        /// </summary>
        public static string ForeignKeyScaffoldErrorPropertyNotFound([CanBeNull] object foreignKeyName, [CanBeNull] object columnNames)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ForeignKeyScaffoldErrorPropertyNotFound", "foreignKeyName", "columnNames"), foreignKeyName, columnNames);
        }

        /// <summary>
        /// Could not scaffold the primary key for '{tableName}'. The following columns in the primary key could not be scaffolded: {columnNames}.
        /// </summary>
        public static string PrimaryKeyErrorPropertyNotFound([CanBeNull] object tableName, [CanBeNull] object columnNames)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("PrimaryKeyErrorPropertyNotFound", "tableName", "columnNames"), tableName, columnNames);
        }

        /// <summary>
        /// Unable to identify the primary key for table '{tableName}'.
        /// </summary>
        public static string MissingPrimaryKey([CanBeNull] object tableName)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("MissingPrimaryKey", "tableName"), tableName);
        }

        /// <summary>
        /// Metadata model returned should not be null. Provider: {providerTypeName}.
        /// </summary>
        public static string ProviderReturnedNullModel([CanBeNull] object providerTypeName)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ProviderReturnedNullModel", "providerTypeName"), providerTypeName);
        }

        /// <summary>
        /// No files generated in directory {outputDirectoryName}. The following file(s) already exist and must be made writeable to continue: {readOnlyFiles}.
        /// </summary>
        public static string ReadOnlyFiles([CanBeNull] object outputDirectoryName, [CanBeNull] object readOnlyFiles)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ReadOnlyFiles", "outputDirectoryName", "readOnlyFiles"), outputDirectoryName, readOnlyFiles);
        }

        /// <summary>
        /// Unable to generate entity type for table '{tableName}'.
        /// </summary>
        public static string UnableToGenerateEntityType([CanBeNull] object tableName)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("UnableToGenerateEntityType", "tableName"), tableName);
        }

        /// <summary>
        /// Unable to scaffold the index '{indexName}'. The following columns could not be scaffolded: {columnNames}.
        /// </summary>
        public static string UnableToScaffoldIndexMissingProperty([CanBeNull] object indexName, [CanBeNull] object columnNames)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("UnableToScaffoldIndexMissingProperty", "indexName", "columnNames"), indexName, columnNames);
        }

        /// <summary>
        /// Cannot scaffold the connection string. The "UseProviderMethodName" is missing from the scaffolding model.
        /// </summary>
        public static string MissingUseProviderMethodNameAnnotation
        {
            get { return GetString("MissingUseProviderMethodNameAnnotation"); }
        }

        /// <summary>
        /// The following file(s) already exist in directory {outputDirectoryName}: {existingFiles}. Use the Force flag to overwrite these files.
        /// </summary>
        public static string ExistingFiles([CanBeNull] object outputDirectoryName, [CanBeNull] object existingFiles)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ExistingFiles", "outputDirectoryName", "existingFiles"), outputDirectoryName, existingFiles);
        }

        /// <summary>
        /// Sequence name cannot be null or empty. Entity Framework cannot model a sequence that does not have a name.
        /// </summary>
        public static string SequencesRequireName
        {
            get { return GetString("SequencesRequireName"); }
        }

        /// <summary>
        /// For sequence '{sequenceName}'. Unable to scaffold because it uses an unsupported type: '{typeName}'.
        /// </summary>
        public static string BadSequenceType([CanBeNull] object sequenceName, [CanBeNull] object typeName)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("BadSequenceType", "sequenceName", "typeName"), sequenceName, typeName);
        }

        /// <summary>
        /// Unable to find a schema in the database matching the selected schema {schema}.
        /// </summary>
        public static string MissingSchema([CanBeNull] object schema)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("MissingSchema", "schema"), schema);
        }

        /// <summary>
        /// Unable to find a table in the database matching the selected table {table}.
        /// </summary>
        public static string MissingTable([CanBeNull] object table)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("MissingTable", "table"), table);
        }

        private static string GetString(string name, params string[] formatterNames)
        {
            var value = _resourceManager.GetString(name);

            Debug.Assert(value != null);

            if (formatterNames != null)
            {
                for (var i = 0; i < formatterNames.Length; i++)
                {
                    value = value.Replace("{" + formatterNames[i] + "}", "{" + i + "}");
                }
            }

            return value;
        }
    }
}
