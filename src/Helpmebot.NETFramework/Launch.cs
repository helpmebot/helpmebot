namespace Helpmebot
{
    using Helpmebot.CoreServices.Startup;

    public class Launch
    {
        private static void Main(string[] args)
        {
            Entrypoint.MainEntrypoint(args, typeof(FrameworkAbstraction));
        }
    }
}