namespace Helpmebot
{
    using System.Reflection;
    using System.Runtime.Loader;
    using Helpmebot.CoreServices.Startup;

    public class FrameworkAbstraction : IFrameworkAbstraction
    {
        public Assembly LoadAssembly(string path) => AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
    }
}