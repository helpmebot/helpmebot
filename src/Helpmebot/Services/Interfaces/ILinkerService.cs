namespace Helpmebot.Services.Interfaces
{
    using System.Collections;
    using Stwalkerster.IrcClient.Events;

    public interface ILinkerService
    {
        /// <summary>
        /// Gets the link.
        /// </summary>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="useSecureServer">
        /// if set to <c>true</c> [use secure server].
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        string GetLink(string destination, bool useSecureServer);

        /// <summary>
        /// Really parses the message.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <returns>
        /// The <see cref="ArrayList"/>.
        /// </returns>
        ArrayList ReallyParseMessage(string message);

        /// <summary>
        /// The IRC private message event.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        void IrcPrivateMessageEvent(object sender, MessageReceivedEventArgs e);
    }
}