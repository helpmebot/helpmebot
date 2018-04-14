// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IBlockMonitoringService.cs" company="Helpmebot Development Team">
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
//   Defines the IBlockMonitoringService type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Services.Interfaces
{
    using System;
    using Stwalkerster.IrcClient.Events;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    /// <summary>
    /// The BlockMonitoringService interface.
    /// </summary>
    public interface IBlockMonitoringService
    {
        /// <summary>
        /// The do event processing.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="sender">
        /// The sender.
        /// </param>
        [Obsolete]
        void DoEventProcessing(string channel, IUser user, IIrcClient sender);

        void OnJoinEvent(object sender, JoinEventArgs e);

        void AddMap(string monitorChannel, string reportChannel);
        void DeleteMap(string monitorChannel, string reportChannel);
    }
}
