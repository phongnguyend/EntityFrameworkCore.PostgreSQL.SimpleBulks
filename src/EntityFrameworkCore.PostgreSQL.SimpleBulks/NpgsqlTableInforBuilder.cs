using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql;
using System;
using System.Collections.Generic;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks;

public class NpgsqlTableInforBuilder<T>
{
    private string _schema;

    private string _name;

    private IReadOnlyList<string> _primaryKeys;

    private IReadOnlyList<string> _propertyNames;

    private IReadOnlyList<string> _insertablePropertyNames;

    private IReadOnlyDictionary<string, Type> _propertyTypes;

    private IReadOnlyDictionary<string, string> _columnNameMappings;

    private IReadOnlyDictionary<string, string> _columnTypeMappings;

    private IReadOnlyDictionary<string, ValueConverter> _valueConverters;

    private OutputId _outputId;

    private Func<T, string, NpgsqlParameter> _parameterConverter;

    public NpgsqlTableInforBuilder<T> Schema(string schema)
    {
        _schema = schema;
        return this;
    }

    public NpgsqlTableInforBuilder<T> TableName(string name)
    {
        _name = name;
        return this;
    }

    public NpgsqlTableInforBuilder<T> PrimaryKeys(IReadOnlyList<string> primaryKeys)
    {
        _primaryKeys = primaryKeys;
        return this;
    }

    public NpgsqlTableInforBuilder<T> PropertyNames(IReadOnlyList<string> propertyNames)
    {
        _propertyNames = propertyNames;
        return this;
    }

    public NpgsqlTableInforBuilder<T> InsertablePropertyNames(IReadOnlyList<string> insertablePropertyNames)
    {
        _insertablePropertyNames = insertablePropertyNames;
        return this;
    }

    public NpgsqlTableInforBuilder<T> PropertyTypes(IReadOnlyDictionary<string, Type> propertyTypes)
    {
        _propertyTypes = propertyTypes;
        return this;
    }

    public NpgsqlTableInforBuilder<T> ColumnNameMappings(IReadOnlyDictionary<string, string> columnNameMappings)
    {
        _columnNameMappings = columnNameMappings;
        return this;
    }

    public NpgsqlTableInforBuilder<T> ColumnTypeMappings(IReadOnlyDictionary<string, string> columnTypeMappings)
    {
        _columnTypeMappings = columnTypeMappings;
        return this;
    }

    public NpgsqlTableInforBuilder<T> ValueConverters(IReadOnlyDictionary<string, ValueConverter> valueConverters)
    {
        _valueConverters = valueConverters;
        return this;
    }

    public NpgsqlTableInforBuilder<T> OutputId(string name, OutputIdMode outputIdMode)
    {
        _outputId = new OutputId
        {
            Name = name,
            Mode = outputIdMode
        };
        return this;
    }

    public NpgsqlTableInforBuilder<T> ParameterConverter(Func<T, string, NpgsqlParameter> converter)
    {
        _parameterConverter = converter;
        return this;
    }

    public NpgsqlTableInfor<T> Build()
    {
        var tableInfor = new NpgsqlTableInfor<T>(_schema, _name)
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
