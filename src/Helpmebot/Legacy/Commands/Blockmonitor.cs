// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BlockInfo.cs" company="Helpmebot Development Team">
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
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Helpmebot;
using Helpmebot.Commands.Interfaces;
using Helpmebot.ExtensionMethods;
using Helpmebot.Legacy.Model;
using Helpmebot.Services.Interfaces;
using Microsoft.Practices.ServiceLocation;
using NHibernate;

namespace helpmebot6.Commands
{
    using Helpmebot.Legacy.Transitional;

    [LegacyCommandFlag(LegacyUserRights.Superuser)]
    public class Blockmonitor : GenericCommand
    {
        private ISession databaseSession;
        private IBlockMonitoringService blockMonitorService;

        public Blockmonitor(LegacyUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {
            this.databaseSession = ServiceLocator.Current.GetInstance<ISession>();
            this.blockMonitorService = ServiceLocator.Current.GetInstance<IBlockMonitoringService>();
        }

        /// <summary>
        /// Monitors a channel for blocked users, using this channel as the report channel
        /// </summary>
        /// <returns></returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            var response = new CommandResponseHandler();

            if (this.Arguments.Length == 0)
            {
                response.Respond(this.CommandServiceHelper.MessageService.NotEnoughParameters(this.Channel, "blockmonitor", 1, 0));
                return response;
            }

            List<string> argumentsList = this.Arguments.ToList();
            var mode = argumentsList.PopFromFront();

            switch (mode.ToLower())
            {
                case "add":
                    this.AddMode(argumentsList, response);
                    break;
                case "del":
                case "delete":
                case "remove":
                    this.DeleteMode(argumentsList, response);
                    break;
            }

            return response;
        }
        
        private void AddMode(List<string> argumentsList, CommandResponseHandler response)
        {
            try
            {
                this.blockMonitorService.AddMap(argumentsList.First(), this.Channel);

                response.Respond(this.CommandServiceHelper.MessageService.Done(this.Channel));
            }
            catch (Exception e)
            {
                this.Log.Error("Error occurred during addition of block monitor", e);
                response.Respond(e.Message);
            }
        }

        private void DeleteMode(List<string> argumentsList, CommandResponseHandler response)
        {
            try
            {
                this.blockMonitorService.DeleteMap(argumentsList.First(), this.Channel);

                response.Respond(this.CommandServiceHelper.MessageService.Done(this.Channel));
            }
            catch (Exception e)
            {
                this.Log.Error("Error occurred during addition of welcome mask.", e);
                response.Respond(e.Message);
            }
        }

    }
}