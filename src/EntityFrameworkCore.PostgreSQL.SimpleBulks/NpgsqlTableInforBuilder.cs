using EntityFrameworkCore.PostgreSQL.SimpleBulks.Extensions;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks;

public class NpgsqlTableInforBuilder<T>
{
    private string _schema;

    private string _name;

    private IReadOnlyList<string> _primaryKeys;

    private List<string> _propertyNames;

    private List<string> _insertablePropertyNames;

    private Dictionary<string, string> _columnNameMappings = new();

    private Dictionary<string, string> _columnTypeMappings = new();

    private Dictionary<string, ValueConverter> _valueConverters = new();

    private OutputId _outputId;

    private Func<T, string, NpgsqlParameter> _parameterConverter;

    public NpgsqlTableInforBuilder()
    {
        _propertyNames = PropertiesCache<T>.GetProperties().Select(x => x.Key).ToList();
        _insertablePropertyNames = PropertiesCache<T>.GetProperties().Select(x => x.Key).ToList();
    }

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

    public NpgsqlTableInforBuilder<T> PrimaryKeys(Expression<Func<T, object>> primaryKeysSelector)
    {
        var primaryKey = primaryKeysSelector.Body.GetMemberName();
        var primaryKeys = string.IsNullOrEmpty(primaryKey) ? primaryKeysSelector.Body.GetMemberNames() : [primaryKey];
        return PrimaryKeys(primaryKeys);
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

    public NpgsqlTableInforBuilder<T> OutputId(Expression<Func<T, object>> nameSelector, OutputIdMode outputIdMode)
    {
        var propertyName = nameSelector.Body.GetMemberName();
        return OutputId(propertyName, outputIdMode);
    }

    public NpgsqlTableInforBuilder<T> ParameterConverter(Func<T, string, NpgsqlParameter> converter)
    {
        _parameterConverter = converter;
        return this;
    }

    public NpgsqlTableInforBuilder<T> IgnoreProperty(string name)
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

    public NpgsqlTableInforBuilder<T> IgnoreProperty(Expression<Func<T, object>> nameSelector)
    {
        var propertyName = nameSelector.Body.GetMemberName();

        return IgnoreProperty(propertyName);
    }

    public NpgsqlTableInforBuilder<T> ReadOnlyProperty(string name)
    {
        if (_insertablePropertyNames != null && _insertablePropertyNames.Contains(name))
        {
            _insertablePropertyNames.Remove(name);
        }

        return this;
    }

    public NpgsqlTableInforBuilder<T> ReadOnlyProperty(Expression<Func<T, object>> nameSelector)
    {
        var propertyName = nameSelector.Body.GetMemberName();

        return ReadOnlyProperty(propertyName);
    }

    public NpgsqlTableInforBuilder<T> ConfigureProperty(string propertyName, string columnName = null, string columnType = null)
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

    public NpgsqlTableInforBuilder<T> ConfigureProperty(Expression<Func<T, object>> nameSelector, string columnName = null, string columnType = null)
    {
        var propertyName = nameSelector.Body.GetMemberName();

        return ConfigureProperty(propertyName, columnName, columnType);
    }

    public NpgsqlTableInforBuilder<T> ConfigurePropertyConversion<TProperty, TProvider>(Expression<Func<T, TProperty>> nameSelector, Func<TProperty, TProvider?> convertToProvider, Func<TProvider?, TProperty?> convertFromProvider)
    {
        var propertyName = nameSelector.Body.GetMemberName();
        _valueConverters[propertyName] = new ValueConverter
        {
            ProviderClrType = typeof(TProvider),
            ConvertToProvider = obj => convertToProvider((TProperty?)obj),
            ConvertFromProvider = obj => convertFromProvider((TProvider?)obj),
        };
        return this;
    }

    public NpgsqlTableInfor<T> Build()
    {
        if (_outputId?.Mode == OutputIdMode.ServerGenerated && _insertablePropertyNames.Contains(_outputId.Name))
        {
            _insertablePropertyNames.Remove(_outputId.Name);
        }

        var tableInfor = new NpgsqlTableInfor<T>(_schema, _name)
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
