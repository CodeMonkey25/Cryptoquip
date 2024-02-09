using System;
using Splat;

namespace Cryptoquip.Extensions;

public static class ReadonlyDependencyResolverExtensions
{
    public static T GetRequiredService<T>(this IReadonlyDependencyResolver resolver, string? contract = null)
    {
        if (resolver.GetService<T>(contract) is { } obj)
            return obj;

        throw new Exception($"Unable to create required dependency of type {typeof(T).FullName}: IReadonlyDependencyResolver.GetService() returned null");
    }
}