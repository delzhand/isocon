using System;
using System.Collections.Generic;

public delegate void SimpleCallback();
public delegate IUnitData InterfaceCallback(string input);

public static class UnitTokenRegistry
{
    private static readonly List<string> _systems = new();
    private static readonly Dictionary<string, SimpleCallback> _simpleCallbacks = new();
    private static readonly Dictionary<string, InterfaceCallback> _interfaceCallbacks = new();

    public static List<string> GetAllSystems()
    {
        return _systems;
    }

    public static void DoCallback(string key)
    {
        if (_simpleCallbacks.TryGetValue(key, out var callback))
        {
            callback.Invoke();
        }
        else
        {
            throw new KeyNotFoundException($"Key '{key}' is not registered.");
        }
    }

    public static IUnitData DoInterfaceCallback(string system, string json)
    {
        if (_interfaceCallbacks.TryGetValue(system, out var callback))
        {
            return callback.Invoke(json);
        }
        else
        {
            throw new KeyNotFoundException($"Key '{system}' is not registered.");
        }
    }

    public static void RegisterSystem(string systemName)
    {
        if (_systems.Contains(systemName))
        {
            throw new InvalidOperationException($"A system token with the name '{systemName}' is already registered.");
        }

        _systems.Add(systemName);
    }

    public static void RegisterInterfaceCallback(string systemName, InterfaceCallback callback)
    {
        if (_interfaceCallbacks.ContainsKey(systemName))
        {
            throw new InvalidOperationException($"An interface with the name '{systemName}' is already registered.");
        }

        _interfaceCallbacks[systemName] = callback;

    }

    public static void RegisterSimpleCallback(string systemName, SimpleCallback callback)
    {
        if (_simpleCallbacks.ContainsKey(systemName))
        {
            throw new InvalidOperationException($"A system token with the name '{systemName}' is already registered.");
        }

        _simpleCallbacks[systemName] = callback;
    }
}
