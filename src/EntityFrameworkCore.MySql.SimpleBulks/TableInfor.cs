using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityFrameworkCore.MySql.SimpleBulks;

public abstract class TableInfor<T>
{
    public string Schema { get; private set; }

    public string Name { get; private set; }

    public string SchemaQualifiedTableName { get; private set; }

    public IReadOnlyList<string> PrimaryKeys { get; init; }

    public IReadOnlyList<string> PropertyNames { get; init; }

    public IReadOnlyList<string> InsertablePropertyNames { get; init; }

    public IReadOnlyDictionary<string, string> ColumnNameMappings { get; init; }

    public IReadOnlyDictionary<string, string> ColumnTypeMappings { get; init; }

    public IReadOnlyDictionary<string, ValueConverter> ValueConverters { get; init; }

    public OutputId OutputId { get; init; }

    public TableInfor(string schema, string tableName, Func<string, string, string> schemaTranslator)
    {
        Schema = schema;
        Name = tableName;

        SchemaQualifiedTableName = string.IsNullOrEmpty(schema) ? $"`{tableName}`" : $"`{schemaTranslator(schema, tableName)}`";
    }

    public TableInfor(string tableName) : this(null, tableName, null)
    {
    }

    public string GetDbColumnName(string propertyName)
    {
        if (ColumnNameMappings == null)
        {
            return propertyName;
        }

        return ColumnNameMappings.TryGetValue(propertyName, out string value) ? value : propertyName;
    }

    public Type GetProviderClrType(string propertyName)
    {
        return PropertiesCache<T>.GetPropertyUnderlyingType(propertyName, ValueConverters);
    }

    public object GetProviderValue(string name, T item)
    {
        return PropertiesCache<T>.GetPropertyValue(name, item, ValueConverters);
    }

    public string CreateParameterName(string propertyName)
    {
        if (propertyName.Contains('.'))
        {
            return $"@{propertyName.Replace(".", "_")}";
        }

        return $"@{propertyName}";
    }

    public string CreateParameterNames(IReadOnlyCollection<string> propertyNames)
    {
        return string.Join(", ", propertyNames.Select(CreateParameterName));
    }

    public abstract List<ParameterInfo> CreateMySqlParameters(MySqlCommand command, T data, IReadOnlyCollection<string> propertyNames, bool autoAdd);
}

public class DbContextTableInfor<T> : TableInfor<T>
{
    private readonly DbContext _dbContext;

    public DbContextTableInfor(string schema, string tableName, Func<string, string, string> schemaTranslator, DbContext dbContext) : base(schema, tableName, schemaTranslator)
    {
        _dbContext = dbContext;
    }

    public DbContextTableInfor(string tableName, DbContext dbContext) : base(tableName)
    {
        _dbContext = dbContext;
    }

    public override List<ParameterInfo> CreateMySqlParameters(MySqlCommand command, T data, IReadOnlyCollection<string> propertyNames, bool autoAdd)
    {
        var parameters = new List<ParameterInfo>();

        var mappingSource = _dbContext.GetService<IRelationalTypeMappingSource>();

        foreach (var propName in propertyNames)
        {
            if (ColumnTypeMappings != null && ColumnTypeMappings.TryGetValue(propName, out var columnType))
            {
                var mapping = mappingSource.FindMapping(columnType);
                var para = (MySqlParameter)mapping.CreateParameter(command, CreateParameterName(propName), GetProviderValue(propName, data) ?? DBNull.Value);

                parameters.Add(new ParameterInfo
                {
                    Name = para.ParameterName,
                    Type = columnType,
                    Parameter = para
                });

                if (autoAdd)
                {
                    command.Parameters.Add(para);
                }
            }
        }

        return parameters;

    }
}

public class MySqlTableInfor<T> : TableInfor<T>
{
    public Func<T, string, MySqlParameter> ParameterConverter { get; init; }

    public MySqlTableInfor(string tableName) : base(tableName)
    {
    }

    public override List<ParameterInfo> CreateMySqlParameters(MySqlCommand command, T data, IReadOnlyCollection<string> propertyNames, bool autoAdd)
    {
        var parameters = new List<ParameterInfo>();

        foreach (var propName in propertyNames)
        {
            var para = ParameterConverter?.Invoke(data, propName);

            if (para == null)
            {
                para = new MySqlParameter(CreateParameterName(propName), GetProviderValue(propName, data) ?? DBNull.Value);

                var paraInfo = new ParameterInfo
                {
                    Name = para.ParameterName,
                    Parameter = para
                };

                if (ColumnTypeMappings != null && ColumnTypeMappings.TryGetValue(propName, out var columnType))
                {
                    paraInfo.Type = columnType;
                }
                else
                {
                    var type = GetProviderClrType(propName);
                    paraInfo.Type = type.ToMySqlDbType();
                }

                para.MySqlDbType = paraInfo.Type.ToMySqlDbType();

                parameters.Add(paraInfo);
            }
            else
            {
                parameters.Add(new ParameterInfo
                {
                    Name = para.ParameterName,
                    Type = para.MySqlDbType.ToString(),
                    Parameter = para,
                    FromConverter = true
                });
            }

            if (autoAdd)
            {
                command.Parameters.Add(para);
            }
        }

        return parameters;

    }
}