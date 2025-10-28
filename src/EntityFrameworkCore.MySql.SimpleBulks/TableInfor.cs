﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace EntityFrameworkCore.MySql.SimpleBulks;

public abstract class TableInfor
{
    public string Schema { get; private set; }

    public string Name { get; private set; }

    public string SchemaQualifiedTableName { get; private set; }

    public TableInfor(string schema, string tableName, Func<string, string, string> schemaTranslator)
    {
        Schema = schema;
        Name = tableName;

        SchemaQualifiedTableName = string.IsNullOrEmpty(schema) ? $"`{tableName}`" : $"`{schemaTranslator(schema, tableName)}`";
    }

    public TableInfor(string tableName) : this(null, tableName, null)
    {
    }
    public abstract List<MySqlParameter> CreateMySqlParameters<T>(MySqlCommand command, T data, IEnumerable<string> propertyNames);
}

public class DbContextTableInfor : TableInfor
{
    private readonly DbContext _dbContext;

    public IReadOnlyDictionary<string, string> ColumnNameMappings { get; init; }

    public IReadOnlyDictionary<string, string> ColumnTypeMappings { get; init; }

    public IReadOnlyDictionary<string, ValueConverter> ValueConverters { get; init; }

    public DbContextTableInfor(string schema, string tableName, Func<string, string, string> schemaTranslator, DbContext dbContext) : base(schema, tableName, schemaTranslator)
    {
        _dbContext = dbContext;
    }

    public DbContextTableInfor(string tableName, DbContext dbContext) : base(tableName)
    {
        _dbContext = dbContext;
    }

    public override List<MySqlParameter> CreateMySqlParameters<T>(MySqlCommand command, T data, IEnumerable<string> propertyNames)
    {
        var properties = TypeDescriptor.GetProperties(typeof(T));

        var updatablePros = new List<PropertyDescriptor>();
        foreach (PropertyDescriptor prop in properties)
        {
            if (propertyNames.Contains(prop.Name))
            {
                updatablePros.Add(prop);
            }
        }

        var parameters = new List<MySqlParameter>();

        var mappingSource = _dbContext.GetService<IRelationalTypeMappingSource>();

        foreach (PropertyDescriptor prop in updatablePros)
        {
            if (ColumnTypeMappings != null && ColumnTypeMappings.TryGetValue(prop.Name, out var columnType))
            {
                var mapping = mappingSource.FindMapping(columnType);
                var para = (MySqlParameter)mapping.CreateParameter(command, prop.Name, GetProviderValue(prop, data) ?? DBNull.Value);
                parameters.Add(para);
            }
        }

        return parameters;

    }

    private object GetProviderValue<T>(PropertyDescriptor property, T item)
    {
        if (ValueConverters != null && ValueConverters.TryGetValue(property.Name, out var converter))
        {
            return converter.ConvertToProvider(property.GetValue(item));
        }

        return property.GetValue(item);
    }
}

public class MySqlTableInfor : TableInfor
{
    public MySqlTableInfor(string tableName) : base(tableName)
    {
    }

    public override List<MySqlParameter> CreateMySqlParameters<T>(MySqlCommand command, T data, IEnumerable<string> propertyNames)
    {
        var properties = TypeDescriptor.GetProperties(typeof(T));

        var updatablePros = new List<PropertyDescriptor>();
        foreach (PropertyDescriptor prop in properties)
        {
            if (propertyNames.Contains(prop.Name))
            {
                updatablePros.Add(prop);
            }
        }

        var parameters = new List<MySqlParameter>();

        foreach (PropertyDescriptor prop in updatablePros)
        {
            var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

            var para = new MySqlParameter($"@{prop.Name}", prop.GetValue(data) ?? DBNull.Value);

            if (type == typeof(DateTime))
            {
                para.DbType = System.Data.DbType.DateTime2;
            }

            parameters.Add(para);
        }

        return parameters;

    }
}