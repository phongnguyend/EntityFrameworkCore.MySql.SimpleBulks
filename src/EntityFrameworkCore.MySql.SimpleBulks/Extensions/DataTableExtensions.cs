using MySqlConnector;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace EntityFrameworkCore.MySql.SimpleBulks.Extensions;

public static class DataTableExtensions
{
    public static string GenerateTempTableDefinition(this DataTable table, string tableName, IDictionary<string, string> dbColumnMappings = null)
    {
        var sql = new StringBuilder();

        sql.AppendFormat("CREATE TEMPORARY TABLE {0} (", tableName);

        for (int i = 0; i < table.Columns.Count; i++)
        {
            if (i > 0)
            {
                sql.Append(",");
            }

            sql.Append($"\n\t`{GetDbColumnName(table.Columns[i].ColumnName, dbColumnMappings)}`");
            var sqlType = table.Columns[i].DataType.ToSqlType();
            sql.Append($" {sqlType} NULL");
        }

        sql.Append("\n);");

        return sql.ToString();
    }

    public static void SqlBulkCopy(this DataTable dataTable, string tableName, IDictionary<string, string> dbColumnMappings, MySqlConnection connection, MySqlTransaction transaction, BulkOptions options = null)
    {
        options ??= new BulkOptions()
        {
            BatchSize = 0,
            Timeout = 30,
        };

        var bulkCopy = new MySqlBulkCopy(connection, transaction)
        {
            BulkCopyTimeout = options.Timeout,
            DestinationTableName = $"{tableName}"
        };

        int idx = 0;

        foreach (DataColumn dtColum in dataTable.Columns)
        {
            bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(idx, GetDbColumnName(dtColum.ColumnName, dbColumnMappings)));
            idx++;
        }

        bulkCopy.WriteToServer(dataTable);
    }

    private static string GetDbColumnName(string columnName, IDictionary<string, string> dbColumnMappings)
    {
        if (dbColumnMappings == null)
        {
            return columnName;
        }

        return dbColumnMappings.TryGetValue(columnName, out string value) ? value : columnName;
    }
}
