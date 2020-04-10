using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QueryFirst
{
    public class _5ScaffoldUpdateOrInsert
    {
        ISchemaFetcher _schemaFetcher;
        public _5ScaffoldUpdateOrInsert(ISchemaFetcher schemaFetcher)
        {
            _schemaFetcher = schemaFetcher;
        }
        public State Go(ref State state)
        {
            var matchInsert = Regex.Match(state._3InitialQueryText, @"^insert\s+into\s+(?<tableName>\w+)\.\.\.", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var matchUpdate = Regex.Match(state._3InitialQueryText, @"^update\s+(?<tableName>\w+)\.\.\.", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (matchInsert.Success || matchUpdate.Success)
            {
                var targetTable = matchInsert.Success ? matchInsert.Groups["tableName"].Value : matchUpdate.Groups["tableName"].Value;
                // get schema
                var cols = _schemaFetcher.GetFields(
                    state._4Config.defaultConnection,
                    state._4Config.provider,
                    $"select * from {targetTable}"
                ).Where(col => (!col.IsIdentity && !(new string[] { "text","ntext","image","timestamp"}).Contains(col.TypeDb) /* && !col.IsComputed*/));
                if (matchInsert.Success)
                {

                    state._5QueryAfterScaffolding =
    @"/* .sql query managed by QueryFirst add-in */


-- designTime - put parameter declarations and design time initialization here


-- endDesignTime
INSERT INTO " + targetTable + @" (
" + string.Join(",\r\n", cols.Select(col => col.ColumnName)) + @"
)
VALUES (
" + string.Join(",\r\n", cols.Select(col => "@" + col.ColumnName)) + @"
)";
                }
                else if (matchUpdate.Success)
                {
                    state._5QueryAfterScaffolding =
@"/* .sql query managed by QueryFirst add-in */


-- designTime - put parameter declarations and design time initialization here


-- endDesignTime
UPDATE " + targetTable + @"
SET 
" + string.Join(",\r\n", cols.Select(col=> col.ColumnName + " = @" + col.ColumnName ));
                }
            }
            else state._5QueryAfterScaffolding = state._3InitialQueryText;
            return state;
        }
    }
}
