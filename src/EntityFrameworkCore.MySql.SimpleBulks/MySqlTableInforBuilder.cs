using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MySqlConnector;
using System;
using System.Collections.Generic;

namespace EntityFrameworkCore.MySql.SimpleBulks;

public class MySqlTableInforBuilder<T>
{
    private string _name;

    private IReadOnlyList<string> _primaryKeys;

    private IReadOnlyList<string> _propertyNames;

    private IReadOnlyList<string> _insertablePropertyNames;

    private IReadOnlyDictionary<string, Type> _propertyTypes;

    private IReadOnlyDictionary<string, string> _columnNameMappings;

    private IReadOnlyDictionary<string, string> _columnTypeMappings;

    private IReadOnlyDictionary<string, ValueConverter> _valueConverters;

    private OutputId _outputId;

    private Func<T, string, MySqlParameter> _parameterConverter;

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

    public MySqlTableInforBuilder<T> PropertyNames(IReadOnlyList<string> propertyNames)
    {
        _propertyNames = propertyNames;
        return this;
    }

    public MySqlTableInforBuilder<T> InsertablePropertyNames(IReadOnlyList<string> insertablePropertyNames)
    {
        _insertablePropertyNames = insertablePropertyNames;
        return this;
    }

    public MySqlTableInforBuilder<T> PropertyTypes(IReadOnlyDictionary<string, Type> propertyTypes)
    {
        _propertyTypes = propertyTypes;
        return this;
    }

    public MySqlTableInforBuilder<T> ColumnNameMappings(IReadOnlyDictionary<string, string> columnNameMappings)
    {
        _columnNameMappings = columnNameMappings;
        return this;
    }

    public MySqlTableInforBuilder<T> ColumnTypeMappings(IReadOnlyDictionary<string, string> columnTypeMappings)
    {
        _columnTypeMappings = columnTypeMappings;
        return this;
    }

    public MySqlTableInforBuilder<T> ValueConverters(IReadOnlyDictionary<string, ValueConverter> valueConverters)
    {
        _valueConverters = valueConverters;
        return this;
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

    public MySqlTableInforBuilder<T> ParameterConverter(Func<T, string, MySqlParameter> converter)
    {
        _parameterConverter = converter;
        return this;
    }

    public MySqlTableInfor<T> Build()
    {
        var tableInfor = new MySqlTableInfor<T>(_name)
        {
            PrimaryKeys = _primaryKeys,
            PropertyNames = _propertyNames,
            InsertablePropertyNames = _insertablePropertyNames,
            PropertyTypes = _propertyTypes,
            ColumnNameMappings = _columnNameMappings,
            ColumnTypeMappings = _columnTypeMappings,
            ValueConverters = _valueConverters,
            OutputId = _outputId,
            ParameterConverter = _parameterConverter,
        };
        return tableInfor;
    }
}
