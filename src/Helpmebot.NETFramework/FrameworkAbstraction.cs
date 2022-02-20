namespace Helpmebot
{
    using System.Reflection;
    using Helpmebot.CoreServices.Startup;

    public class FrameworkAbstraction : IFrameworkAbstraction
    {
        public Assembly LoadAssembly(string path) => Assembly.LoadFile(path);
    }
}