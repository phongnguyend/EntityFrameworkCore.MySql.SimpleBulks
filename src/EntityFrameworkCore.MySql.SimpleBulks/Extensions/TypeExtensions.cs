using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

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

    public static Dictionary<string, Type> GetProviderClrTypes(this Type type, IEnumerable<string> propertyNames, IReadOnlyDictionary<string, ValueConverter> valueConverters)
    {
        var properties = TypeDescriptor.GetProperties(type);

        var updatablePros = new List<PropertyDescriptor>();
        foreach (PropertyDescriptor prop in properties)
        {
            if (propertyNames.Contains(prop.Name))
            {
                updatablePros.Add(prop);
            }
        }

        return updatablePros.ToDictionary(x => x.Name, x => GetProviderClrType(x, valueConverters));
    }

    private static Type GetProviderClrType(PropertyDescriptor property, IReadOnlyDictionary<string, ValueConverter> valueConverters)
    {
        if (valueConverters != null && valueConverters.TryGetValue(property.Name, out var converter))
        {
            return converter.ProviderClrType;
        }

        return Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
    }
}
