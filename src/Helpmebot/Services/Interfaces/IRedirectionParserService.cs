using System.Collections.Generic;

namespace Helpmebot.Services.Interfaces
{
    public interface IRedirectionParserService
    {
        RedirectionParserService.RedirectionParserResult Parse(IEnumerable<string> messageData);
    }
}