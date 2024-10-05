using System;
using System.Reflection;
using System.Runtime.Loader;

namespace AliceBot.Handlers.Manager.Plugins;

public class PluginLoadContext(string dll) : AssemblyLoadContext(true) {
    private readonly string _dll = dll;

    private readonly AssemblyDependencyResolver _resolver = new(dll);

    protected override Assembly? Load(AssemblyName assemblyName) {
        string? assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath != null) {
            return LoadFromAssemblyPath(assemblyPath);
        }

        return null;
    }

    protected override nint LoadUnmanagedDll(string unmanagedDllName) {
        string? libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        if (libraryPath != null) {
            return LoadUnmanagedDllFromPath(libraryPath);
        }

        return IntPtr.Zero;
    }

    public TDelegate LoadFoctory<TDelegate>(string typeFullName, string methodName) where TDelegate : Delegate {
        Assembly assembly = LoadFromAssemblyPath(_dll);
        Type type = assembly.GetType(typeFullName) ?? throw new Exception($"Type {typeFullName} not found in {_dll}.");
        MethodInfo method = type.GetMethod(methodName) ?? throw new Exception($"Method {methodName} not found in {typeFullName}.");

        return method.CreateDelegate<TDelegate>();
    }
}