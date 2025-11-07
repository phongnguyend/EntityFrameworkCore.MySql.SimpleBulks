using MySqlConnector;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EntityFrameworkCore.MySql.SimpleBulks.Extensions;

public static class TypeExtensions
{
    private static Dictionary<Type, string> _mappings = new Dictionary<Type, string>
    {
        {typeof(bool), "tinyint(1)"},
        {typeof(DateTime), "datetime(6)"},
        {typeof(DateTimeOffset), "datetime(6)"},
        {typeof(decimal), "decimal(65,30)"},
        {typeof(double), "double"},
        {typeof(Guid), "char(36)"},
        {typeof(short), "smallint"},
        {typeof(int), "int"},
        {typeof(long), "bigint"},
        {typeof(float), "float"},
        {typeof(string), "longtext"},
    };

    private static readonly ConcurrentDictionary<string, MySqlDbType> _sqlTypeCache = new();

    public static string ToMySqlDbType(this Type type)
    {
        if (type.IsEnum)
        {
            return "int";
        }

        var sqlType = _mappings.TryGetValue(type, out string value) ? value : "longtext";
        return sqlType;
    }

    public static MySqlDbType ToMySqlDbType(this string sqlTypeText)
    {
        if (string.IsNullOrWhiteSpace(sqlTypeText))
        {
            return MySqlDbType.LongText;
        }

        return _sqlTypeCache.GetOrAdd(sqlTypeText, static sqlType =>
        {
            // Extract the base type name by removing scale/precision parameters
            var baseType = Regex.Replace(sqlType.ToLowerInvariant(), @"\([^)]*\)", "").Trim();

            return baseType switch
            {
                "tinyint" => MySqlDbType.Byte,
                "smallint" => MySqlDbType.Int16,
                "int" => MySqlDbType.Int32,
                "bigint" => MySqlDbType.Int64,
                "float" => MySqlDbType.Float,
                "double" => MySqlDbType.Double,
                "decimal" => MySqlDbType.Decimal,
                "char" => MySqlDbType.String,
                "varchar" => MySqlDbType.VarChar,
                "text" => MySqlDbType.Text,
                "longtext" => MySqlDbType.LongText,
                "mediumtext" => MySqlDbType.MediumText,
                "tinytext" => MySqlDbType.TinyText,
                "datetime" => MySqlDbType.DateTime,
                "date" => MySqlDbType.Date,
                "time" => MySqlDbType.Time,
                "timestamp" => MySqlDbType.Timestamp,
                "binary" => MySqlDbType.Binary,
                "varbinary" => MySqlDbType.VarBinary,
                "blob" => MySqlDbType.Blob,
                "longblob" => MySqlDbType.LongBlob,
                "mediumblob" => MySqlDbType.MediumBlob,
                "tinyblob" => MySqlDbType.TinyBlob,
                "bit" => MySqlDbType.Bit,
                "year" => MySqlDbType.Year,
                "enum" => MySqlDbType.Enum,
                "set" => MySqlDbType.Set,
                "json" => MySqlDbType.JSON,
                "geometry" => MySqlDbType.Geometry,
                _ => MySqlDbType.LongText
            };
        });
    }
}
