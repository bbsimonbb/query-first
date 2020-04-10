using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace QueryFirst
{
    public class _7RunQueryAndGetResultSchema
    {
        private ISchemaFetcher _schemaFetcher;
        private IProvider _provider;
        public _7RunQueryAndGetResultSchema(ISchemaFetcher schemaFetcher, IProvider provider)
        {
            _schemaFetcher = schemaFetcher;
            _provider = provider;
        }
        public State Go(ref State state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            var fields = _schemaFetcher.GetFields(state._4Config.defaultConnection, state._4Config.provider, state._6QueryWithParamsAdded);

            if (fields.Count == 0)
            {
                fields = _provider.GetQuerySchema2ndAttempt(state._6QueryWithParamsAdded, state._4Config.defaultConnection);
            }
            fields.ForEach(field =>
            {
                if (Regex.Match((field.ColumnName.Substring(0, 1)), "[0-9]").Success)
                    field.CSColumnName = "_" + field.ColumnName;
                else
                    field.CSColumnName = field.ColumnName;
                if (field.ColumnName.IndexOf('?') != -1)
                {
                    field.CSColumnName = field.CSColumnName.Substring(0, field.ColumnName.IndexOf('?'));

                    var columnOptions = new Dictionary<string, string>();

                    // todo let's have some tests for this little-used functionality.
                    var parts = field.ColumnName.Split('?');
                    if (parts.Length == 2)
                    {
                        var nameVals = parts[1].Split('&');
                        foreach (var option in nameVals)
                        {
                            var nameVal = option.Split('=');
                            columnOptions.Add(nameVal[0], nameVal.Length > 1 ? nameVal[1] : null);
                        }
                    }
                    field.ColumnOptions = columnOptions;
                }
            });
            state._7ResultFields = fields;
            if (state._7ResultFields != null && state._7ResultFields.Count > 0)
            {
                // Execute scalar return type should always be nullable, even when the underlying column is not nullable.
                if (IsNullable(Type.GetType(state._7ResultFields[0].TypeCs, false)) || state._7ResultFields[0].TypeCs == "System.String")
                {
                    state._7ExecuteScalarReturnType = state._7ResultFields[0].TypeCs;
                }
                else
                {
                    state._7ExecuteScalarReturnType = state._7ResultFields[0].TypeCs + "?";
                }
            }

            return state;
        }

        bool IsNullable(Type type) => type == null || Nullable.GetUnderlyingType(type) != null;

    }
}
