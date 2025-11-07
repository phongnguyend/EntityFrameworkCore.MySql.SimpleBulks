using MySqlConnector;
using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace EntityFrameworkCore.MySql.SimpleBulks.Extensions;

public static class TypeExtensions
{
    private static readonly ConcurrentDictionary<Type, string> _mappings = new ConcurrentDictionary<Type, string>();

    private static readonly ConcurrentDictionary<string, MySqlDbType> _sqlTypeCache = new();

    static TypeExtensions()
    {
        ConfigureMySqlTypeMapping<bool>("tinyint(1)");
        ConfigureMySqlTypeMapping<DateTime>("datetime(6)");
        ConfigureMySqlTypeMapping<DateTimeOffset>("datetime(6)");
        ConfigureMySqlTypeMapping<decimal>("decimal(65,30)");
        ConfigureMySqlTypeMapping<double>("double");
        ConfigureMySqlTypeMapping<Guid>("char(36)");
        ConfigureMySqlTypeMapping<short>("smallint");
        ConfigureMySqlTypeMapping<int>("int");
        ConfigureMySqlTypeMapping<long>("bigint");
        ConfigureMySqlTypeMapping<float>("float");
        ConfigureMySqlTypeMapping<string>("longtext");
    }

    public static void ConfigureMySqlTypeMapping<T>(string mySqlType)
    {
        ConfigureMySqlTypeMapping(typeof(T), mySqlType);
    }

    public static void ConfigureMySqlTypeMapping(Type type, string mySqlType)
    {
        _mappings[type] = mySqlType;
    }

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
