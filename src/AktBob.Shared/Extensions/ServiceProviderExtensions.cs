﻿namespace AktBob.Shared.Extensions;

public static class ServiceProviderExtensions
{
    public static T GetRequiredServiceOrThrow<T>(this IServiceProvider serviceProvider)
    {
        var service = (T?)serviceProvider.GetService(typeof(T));
        if (service == null)
        {
            throw new InvalidOperationException($"Service '{typeof(T).Name}' is not registered.");
        }

        return service;
    }
}
