using BenchmarkDotNet.Attributes;
using EntityFrameworkCore.PostgreSQL.SimpleBulks.Benchmarks.Database;

namespace EntityFrameworkCore.PostgreSQL.SimpleBulks.Benchmarks;

[MemoryDiagnoser]
public class PropertiesCacheBenchmarks
{
    private static string[] _propertyNames;

    [GlobalSetup]
    public void GlobalSetup()
    {
        var propertyNames = typeof(ConfigurationEntry).GetProperties().Select(x => x.Name).ToList();
        propertyNames.AddRange(["NonExistentProperty1", "NonExistentProperty2", "NonExistentProperty3"]);
        _propertyNames = propertyNames.ToArray();
    }

    [Benchmark]
    public int Type_GetProperties()
    {
        var properties = typeof(ConfigurationEntry).GetProperties();

        int count = 0;

        foreach (var prop in _propertyNames)
        {
            var propInfo = properties.FirstOrDefault(x => x.Name == prop);
            if (propInfo != null)
            {
                count++;
            }
        }

        return count;
    }

    [Benchmark]
    public int Type_GetProperties_2()
    {
        var properties = typeof(ConfigurationEntry).GetProperties();

        int count = 0;

        foreach (var prop in properties)
        {
            if (_propertyNames.Contains(prop.Name))
            {
                count++;
            }
        }

        return count;
    }

    [Benchmark]
    public int Type_GetProperty()
    {
        var type = typeof(ConfigurationEntry);

        int count = 0;

        foreach (var prop in _propertyNames)
        {
            var propInfo = type.GetProperty(prop);
            if (propInfo != null)
            {
                count++;
            }
        }

        return count;
    }

    [Benchmark]
    public int PropertiesCache_GetProperties()
    {
        var properties = PropertiesCache<ConfigurationEntry>.GetProperties();

        int count = 0;

        foreach (var prop in _propertyNames)
        {
            if (properties.TryGetValue(prop, out _))
            {
                count++;
            }
        }

        return count;
    }

    [Benchmark]
    public int PropertiesCache_GetProperties_2()
    {
        var properties = PropertiesCache<ConfigurationEntry>.GetProperties();

        int count = 0;

        foreach (var prop in properties)
        {
            if (_propertyNames.Contains(prop.Key))
            {
                count++;
            }
        }

        return count;
    }
}
