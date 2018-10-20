namespace Helpmebot.Services.Interfaces
{
    using System.Net;

    public interface IWhoisService
    {
        string GetOrganisationName(IPAddress ip);
    }
}