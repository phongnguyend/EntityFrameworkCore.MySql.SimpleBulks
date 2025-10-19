using MySqlConnector;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.MySql.SimpleBulks.Extensions;

public static class DataTableExtensions
{
    public static string GenerateTempTableDefinition(this DataTable table, string tableName, IReadOnlyDictionary<string, string> columnNameMappings, IReadOnlyDictionary<string, string> columnTypeMappings)
    {
        var sql = new StringBuilder();

        sql.AppendFormat("CREATE TEMPORARY TABLE {0} (", tableName);

        for (int i = 0; i < table.Columns.Count; i++)
        {
            if (i > 0)
            {
                sql.Append(",");
            }

            sql.Append($"\n\t`{GetDbColumnName(table.Columns[i].ColumnName, columnNameMappings)}`");
            var sqlType = GetDbColumnType(table.Columns[i], columnTypeMappings);
            sql.Append($" {sqlType} NULL");
        }

        sql.Append("\n);");

        return sql.ToString();
    }

    public static void SqlBulkCopy(this DataTable dataTable, string tableName, IReadOnlyDictionary<string, string> columnNameMappings, MySqlConnection connection, MySqlTransaction transaction, BulkOptions options = null)
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
            bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(idx, GetDbColumnName(dtColum.ColumnName, columnNameMappings)));
            idx++;
        }

        bulkCopy.WriteToServer(dataTable);
    }

    public static async Task SqlBulkCopyAsync(this DataTable dataTable, string tableName, IReadOnlyDictionary<string, string> columnNameMappings, MySqlConnection connection, MySqlTransaction transaction, BulkOptions options = null, CancellationToken cancellationToken = default)
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
            bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(idx, GetDbColumnName(dtColum.ColumnName, columnNameMappings)));
            idx++;
        }

        await bulkCopy.WriteToServerAsync(dataTable, cancellationToken);
    }

    private static string GetDbColumnName(string columnName, IReadOnlyDictionary<string, string> columnNameMappings)
    {
        if (columnNameMappings == null)
        {
            return columnName;
        }

        return columnNameMappings.TryGetValue(columnName, out string value) ? value : columnName;
    }

    private static string GetDbColumnType(DataColumn dataColumn, IReadOnlyDictionary<string, string> columnTypeMappings)
    {
        if (columnTypeMappings == null)
        {
            return dataColumn.DataType.ToSqlType();
        }

        return columnTypeMappings.TryGetValue(dataColumn.ColumnName, out string value) ? value : dataColumn.DataType.ToSqlType();
    }
}
