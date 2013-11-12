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
    using System.Xml;

    using Helpmebot;
    using Helpmebot.Legacy.Configuration;
    using Helpmebot.Legacy.Database;
    using Helpmebot.Services.Interfaces;

    /// <summary>
    /// Count how many articles are in a category (if blank, assumes [[Category:Pending AfC submissions]]).
    /// </summary>
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
        /// <param name="messageService">
        /// The message Service.
        /// </param>
        public Categorysize(User source, string channel, string[] args, IMessageService messageService)
            : base(source, channel, args, messageService)
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
                // TODO: really?
                categoryName = "Pending AfC submissions";
            }

            return this.GetResultOfCommand(categoryName);
        }

        /// <summary>
        /// Gets the size of a category.
        /// </summary>
        /// <param name="categoryName">The category to retrieve the article count for.</param>
        /// <param name="channel">The channel the command was issued in. (Gets the correct base wiki)</param>
        /// <returns>the size of the category</returns>
        public int GetCategorySize(string categoryName, string channel)
        {
            if (categoryName == string.Empty)
            {
                throw new ArgumentNullException();
            }

            string baseWiki = LegacyConfig.singleton()["baseWiki", channel];

            DAL.Select q = new DAL.Select("site_api");
            q.setFrom("site");
            q.addWhere(new DAL.WhereConds("site_id", baseWiki));
            string api = DAL.singleton().executeScalarSelect(q);

            XmlTextReader creader =
                new XmlTextReader(
                    HttpRequest.get(api + "?action=query&format=xml&prop=categoryinfo&titles=Category:" +
                                    categoryName));
            do
            {
                creader.Read();
            }
            while (creader.Name != "page");

            if (creader.GetAttribute("missing") == string.Empty)
            {
                return -2;
            }

            creader.Read();
            string categorySize = creader.GetAttribute("size");
            if (categorySize != null)
            {
                return int.Parse(categorySize);
            }

            return -1;
        }

        /// <summary>
        /// The get result of command.
        /// </summary>
        /// <param name="categoryName">
        /// The category name.
        /// </param>
        /// <returns>
        /// The <see cref="CommandResponseHandler"/>.
        /// </returns>
        /// <remarks>
        /// TODO: rename me!
        /// </remarks>
        protected CommandResponseHandler GetResultOfCommand(string categoryName)
        {
            int categorySize = this.GetCategorySize(categoryName, this.Channel);

            switch (categorySize)
            {
                case -2:
                    {
                        string[] messageParams = { categoryName };
                        string message = new Message().GetMessage("categoryMissing", messageParams);
                        return new CommandResponseHandler(message);
                    }

                case -1:
                    {
                        string[] messageParams = { categoryName };
                        string message = new Message().GetMessage("categoryEmpty", messageParams);
                        return new CommandResponseHandler(message);
                    }

                default:
                    {
                        string[] messageParameters = { categorySize.ToString(), categoryName };
                        string message = new Message().GetMessage("categorySize", messageParameters);
                        return new CommandResponseHandler(message);
                    }
            }
        }
    }
}