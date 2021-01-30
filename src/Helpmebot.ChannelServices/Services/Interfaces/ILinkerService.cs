namespace Helpmebot.ChannelServices.Services.Interfaces
{
    using System.Collections.Generic;
    using Castle.Core;

    public interface ILinkerService : IStartable
    {
        /// <summary>
        /// Gets the link.
        /// </summary>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        string GetLastLinkForChannel(string destination);

        IList<string> ParseMessageForLinks(string message);
        string ConvertWikilinkToUrl(string destination, string link);
    }
}