﻿// --------------------------------------------------------------------------------------------------------------------
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

    /// <summary>
    /// Count how many articles are in a category (if blank, assumes [[Category:Pending AfC submissions]]).
    /// </summary>
    internal class Categorysize : GenericCommand
    {
        public Categorysize(User source, string channel, string[] args)
            : base(source, channel, args)
        {
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <param name="source">The user who triggered the command.</param>
        /// <param name="channel">The channel the command was triggered in.</param>
        /// <param name="args">The arguments to the command.</param>
        /// <returns></returns>
        protected override CommandResponseHandler ExecuteCommand(User source, string channel, string[] args)
        {
            string categoryName;
            if (args.Length > 0 && args[0] != "")
            {
                categoryName = string.Join(" ", args);
            }
            else
            {
                categoryName = "Pending AfC submissions";
            }

            return getResultOfCommand(categoryName, channel);
        }

        // TODO: rename me!
        protected CommandResponseHandler getResultOfCommand(string categoryName, string channel)
        {
            int categorySize = getCategorySize(categoryName, channel);

            switch (categorySize)
            {
                case -2:
                    {
                        string[] messageParams = { categoryName };
                        string message = new Message().get("categoryMissing", messageParams);
                        return new CommandResponseHandler(message);
                    }
                case -1:
                    {
                        string[] messageParams = {categoryName};
                        string message = new Message().get("categoryEmpty", messageParams);
                        return new CommandResponseHandler(message);
                    }
                default:
                    {
                        string[] messageParameters = {categorySize.ToString(), categoryName};
                        string message = new Message().get("categorySize", messageParameters);
                        return new CommandResponseHandler(message);
                    }
            }
        }

        /// <summary>
        /// Gets the size of a category.
        /// </summary>
        /// <param name="categoryName">The category to retrieve the article count for.</param>
        /// <param name="channel">The channel the command was issued in. (Gets the correct base wiki)</param>
        /// <returns></returns>
        public int getCategorySize(string categoryName, string channel)
        {
            if (categoryName == string.Empty)
            {
                throw new ArgumentNullException();
            }

            string baseWiki = Configuration.singleton()["baseWiki",channel];

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
            } while (creader.Name != "page");
            if (creader.GetAttribute("missing") == "")
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
    }
}