using EntityFrameworkCore.MySql.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EntityFrameworkCore.MySql.SimpleBulks;

public class MySqlTableInforBuilder<T>
{
    private string _name;

    private IReadOnlyList<string> _primaryKeys;

    private List<string> _propertyNames;

    private List<string> _insertablePropertyNames;

    private Dictionary<string, string> _columnNameMappings = new();

    private Dictionary<string, string> _columnTypeMappings = new();

    private Dictionary<string, ValueConverter> _valueConverters = new();

    private OutputId _outputId;

    private Func<T, string, MySqlParameter> _parameterConverter;

    public MySqlTableInforBuilder()
    {
        _propertyNames = PropertiesCache<T>.GetProperties().Select(x => x.Key).ToList();
        _insertablePropertyNames = PropertiesCache<T>.GetProperties().Select(x => x.Key).ToList();
    }

    public MySqlTableInforBuilder<T> TableName(string name)
    {
        _name = name;
        return this;
    }

    public MySqlTableInforBuilder<T> PrimaryKeys(IReadOnlyList<string> primaryKeys)
    {
        _primaryKeys = primaryKeys;
        return this;
    }

    public MySqlTableInforBuilder<T> PrimaryKeys(Expression<Func<T, object>> primaryKeysSelector)
    {
        var primaryKey = primaryKeysSelector.Body.GetMemberName();
        var primaryKeys = string.IsNullOrEmpty(primaryKey) ? primaryKeysSelector.Body.GetMemberNames() : [primaryKey];
        return PrimaryKeys(primaryKeys);
    }

    public MySqlTableInforBuilder<T> OutputId(string name, OutputIdMode outputIdMode)
    {
        _outputId = new OutputId
        {
            Name = name,
            Mode = outputIdMode
        };
        return this;
    }

    public MySqlTableInforBuilder<T> OutputId(Expression<Func<T, object>> nameSelector, OutputIdMode outputIdMode)
    {
        var propertyName = nameSelector.Body.GetMemberName();
        return OutputId(propertyName, outputIdMode);
    }

    public MySqlTableInforBuilder<T> ParameterConverter(Func<T, string, MySqlParameter> converter)
    {
        _parameterConverter = converter;
        return this;
    }

    public MySqlTableInforBuilder<T> IgnoreProperty(string name)
    {
        if (_propertyNames != null && _propertyNames.Contains(name))
        {
            _propertyNames.Remove(name);
        }

        if (_insertablePropertyNames != null && _insertablePropertyNames.Contains(name))
        {
            _insertablePropertyNames.Remove(name);
        }

        return this;
    }

    public MySqlTableInforBuilder<T> IgnoreProperty(Expression<Func<T, object>> nameSelector)
    {
        var propertyName = nameSelector.Body.GetMemberName();

        return IgnoreProperty(propertyName);
    }

    public MySqlTableInforBuilder<T> ReadOnlyProperty(string name)
    {
        if (_insertablePropertyNames != null && _insertablePropertyNames.Contains(name))
        {
            _insertablePropertyNames.Remove(name);
        }

        return this;
    }

    public MySqlTableInforBuilder<T> ReadOnlyProperty(Expression<Func<T, object>> nameSelector)
    {
        var propertyName = nameSelector.Body.GetMemberName();

        return ReadOnlyProperty(propertyName);
    }

    public MySqlTableInforBuilder<T> ConfigureProperty(string propertyName, string columnName = null, string columnType = null)
    {
        if (columnName != null)
        {
            _columnNameMappings[propertyName] = columnName;
        }

        if (columnType != null)
        {
            _columnTypeMappings[propertyName] = columnType;
        }

        return this;
    }

    public MySqlTableInforBuilder<T> ConfigureProperty(Expression<Func<T, object>> nameSelector, string columnName = null, string columnType = null)
    {
        var propertyName = nameSelector.Body.GetMemberName();

        return ConfigureProperty(propertyName, columnName, columnType);
    }

    public MySqlTableInfor<T> Build()
    {
        var tableInfor = new MySqlTableInfor<T>(_name)
        {
            PrimaryKeys = _primaryKeys,
            PropertyNames = _propertyNames,
            InsertablePropertyNames = _insertablePropertyNames,
            ColumnNameMappings = _columnNameMappings,
            ColumnTypeMappings = _columnTypeMappings,
            ValueConverters = _valueConverters,
            OutputId = _outputId,
            ParameterConverter = _parameterConverter,
        };
        return tableInfor;
    }
}
