namespace Helpmebot.CoreServices.Startup
{
    using System.Reflection;

    public interface IFrameworkAbstraction
    {
        Assembly LoadAssembly(string path);
    }
}