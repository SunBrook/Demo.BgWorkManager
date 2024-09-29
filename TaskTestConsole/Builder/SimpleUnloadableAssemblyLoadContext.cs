using System.Reflection;
using System.Runtime.Loader;

namespace TaskTestConsole.Builder;

internal sealed class SimpleUnloadableAssemblyLoadContext : AssemblyLoadContext
{
    public SimpleUnloadableAssemblyLoadContext() : base(true)
    {

    }
    protected override Assembly Load(AssemblyName assemblyName) => null;
}