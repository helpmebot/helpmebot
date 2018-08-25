// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategorySize.cs" company="Helpmebot Development Team">
//   Helpmebot is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//   
//   Helpmebot is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
//   
//   You should have received a copy of the GNU General Public License
//   along with Helpmebot.  If not, see http://www.gnu.org/licenses/ .
// </copyright>
// <summary>
//   Count how many articles are in a category (if blank, assumes [[Category:Pending AfC submissions]]).
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System;
    using System.Globalization;

    using Helpmebot;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Legacy.Transitional;

    /// <summary>
    /// Count how many articles are in a category.
    /// </summary>
    [LegacyCommandFlag(LegacyUserRights.Normal)]
    internal class Categorysize : GenericCommand
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Categorysize"/> class.
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
        public Categorysize(LegacyUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            string categoryName;
            if (this.Arguments.Length > 0 && this.Arguments[0] != string.Empty)
            {
                categoryName = string.Join(" ", this.Arguments);
            }
            else
            {
                return new CommandResponseHandler(this.CommandServiceHelper.MessageService.NotEnoughParameters(this.Channel, "CategorySize", 1, 0));
            }

            return this.GetSizeOfCategory(categoryName);
        }

        /// <summary>
        /// Gets the size of a category for a command's usage.
        /// </summary>
        /// <param name="categoryName">
        /// The category name.
        /// </param>
        /// <returns>
        /// The <see cref="CommandResponseHandler"/>.
        /// </returns>
        /// <remarks>
        /// If you wish to get the size of a category for another purpose, look at using the MediaWikiSite.GetCategorySize() extension method.
        /// </remarks>
        protected CommandResponseHandler GetSizeOfCategory(string categoryName)
        {
            var mediaWikiSite = this.GetLocalMediawikiSite();

            var messageService = this.CommandServiceHelper.MessageService;
            try
            {
                int categorySize = mediaWikiSite.GetCategorySize(categoryName);

                string[] messageParameters = { categorySize.ToString(CultureInfo.InvariantCulture), categoryName };
                string message = messageService.RetrieveMessage("categorySize", this.Channel, messageParameters);
                return new CommandResponseHandler(message);
            }
            catch (ArgumentException)
            {
                string[] messageParams = { categoryName };
                string message = messageService.RetrieveMessage("categoryMissing", this.Channel, messageParams);
                return new CommandResponseHandler(message);
            }
        }
    }
}
