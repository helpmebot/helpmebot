namespace Helpmebot.CoreServices.Services.Interfaces
{
    using System.Collections.Generic;
    using Helpmebot.CoreServices.Services;

    public interface IRedirectionParserService
    {
        RedirectionParserService.RedirectionParserResult Parse(IEnumerable<string> messageData);
    }
}