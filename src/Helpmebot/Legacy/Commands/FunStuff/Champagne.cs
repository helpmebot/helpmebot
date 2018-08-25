using Helpmebot.Commands.FunStuff;
using Helpmebot.Legacy.Model;

namespace helpmebot6.Commands
{
    using Helpmebot.Legacy;
    using Helpmebot.Legacy.Transitional;
    using Stwalkerster.IrcClient.Model.Interfaces;

    /// <summary>
    /// The tea.
    /// </summary>
    [LegacyCommandFlag(LegacyUserRights.Advanced)]
    internal class Champagne : TargetedFunCommand
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Champagne"/> class.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <param name="commandServiceHelper">
        /// The message Service.
        /// </param>
        public Champagne(IUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {
        }

        /// <summary>
        /// Gets the target message.
        /// </summary>
        protected override string TargetMessage
        {
            get
            {
                return "cmdChampagne";
            }
        }
    }
}