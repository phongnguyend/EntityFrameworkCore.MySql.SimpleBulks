using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

    public static string ToSqlType(this Type type)
    {
        var sqlType = _mappings.TryGetValue(type, out string value) ? value : "longtext";
        return sqlType;
    }

    public static string[] GetDbColumnNames(this Type type, params string[] ignoredColumns)
    {
        var names = type.GetProperties()
            .Where(x => IsSupportedType(x))
            .Where(x => ignoredColumns == null || !ignoredColumns.Contains(x.Name))
            .Select(x => x.Name);
        return names.ToArray();
    }

    public static string[] GetUnSupportedPropertyNames(this Type type)
    {
        var names = type.GetProperties()
            .Where(x => !IsSupportedType(x))
            .Select(x => x.Name);
        return names.ToArray();
    }

    private static bool IsSupportedType(PropertyInfo property)
    {
        return _mappings.ContainsKey(Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType) || property.PropertyType.IsValueType;
    }
}
