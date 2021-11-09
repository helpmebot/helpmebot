namespace Helpmebot.WebUI.Services.Api
{
    using System.Reflection;

    public interface IApiFrontendTransportService
    {
        TResponse RemoteProcedureCall<TParam, TResponse>(MethodBase method, TParam parameter);
    }
}