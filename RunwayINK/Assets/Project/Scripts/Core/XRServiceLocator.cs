using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRServiceLocator : MonoBehaviour
{
    private static readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

    public static void Register<T>(T service)
    {
        var type = typeof(T);
        if (services.ContainsKey(type))
        {
            Debug.LogWarning($"[XRServiceLocator] Overwriting existing service: {type}");
            services[type] = service;
        }
        else
        {
            services.Add(type, service);
        }
    }

    public static T Resolve<T>()
    {
        var type = typeof(T);
        if (services.TryGetValue(type, out var service))
        {
            return (T)service;
        }
        throw new Exception($"[XRServiceLocator] Service not registered: {type}");
    }

    public static void Clear() => services.Clear();
}
