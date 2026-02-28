using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace EntityFrameworkCore.MySql.SimpleBulks.Extensions;

public static class ObjectExtensions
{
    public static List<ParameterInfo> ToMySqlParameterInfors(this object parameters)
    {
        if (parameters == null)
        {
            return [];
        }

        var result = new List<ParameterInfo>();
        var properties = parameters.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            var value = property.GetValue(parameters);
            var parameterName = property.Name.StartsWith("@") ? property.Name : $"@{property.Name}";
            var underlyingType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

            var parameter = new MySqlParameter(parameterName, value ?? DBNull.Value)
            {
                MySqlDbType = underlyingType.ToMySqlDbType().ToMySqlDbType()
            };

            result.Add(new ParameterInfo
            {
                Name = parameter.ParameterName,
                Type = underlyingType.ToMySqlDbType(),
                Parameter = parameter
            });
        }

        return result;
    }
}
