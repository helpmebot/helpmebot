namespace Helpmebot.WebUI.Services
{
    using System.Reflection;

    public interface IApiFrontendTransportService
    {
        TResponse RemoteProcedureCall<TParam, TResponse>(MethodBase method, TParam parameter);
    }
}