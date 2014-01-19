// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BlockInformation.cs" company="Helpmebot Development Team">
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
namespace Helpmebot.Model
{
    using Helpmebot.Services.Interfaces;

    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    ///     Holds the block information of a specific user
    /// </summary>
    public struct BlockInformation
    {
        #region Fields

        /// <summary>
        /// The allow user talk.
        /// </summary>
        public bool AllowUserTalk;

        /// <summary>
        /// The auto-block.
        /// </summary>
        public bool AutoBlock;

        /// <summary>
        /// The block reason.
        /// </summary>
        public string BlockReason;

        /// <summary>
        /// The blocked by.
        /// </summary>
        public string BlockedBy;

        /// <summary>
        /// The expiry.
        /// </summary>
        public string Expiry;

        /// <summary>
        /// The no create.
        /// </summary>
        public bool NoCreate;

        /// <summary>
        /// The no email.
        /// </summary>
        public bool NoEmail;

        /// <summary>
        /// The start.
        /// </summary>
        public string Start;

        /// <summary>
        /// The target.
        /// </summary>
        public string Target;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public string Id { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this block.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents this block.
        /// </returns>
        /// <remarks>
        ///     TODO: fixes for context, localisation, etc,
        /// </remarks>
        public override string ToString()
        {
            var ms = ServiceLocator.Current.GetInstance<IMessageService>();

            string[] emptyMessageParams =
                {
                    string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, 
                    string.Empty, string.Empty
                };
            string emptyMessage = ms.RetrieveMessage("blockInfoShort", null, emptyMessageParams);

            string info = string.Empty;

            if (this.NoCreate)
            {
                info += "NOCREATE ";
            }

            if (this.AutoBlock)
            {
                info += "AUTOBLOCK ";
            }

            if (this.NoEmail)
            {
                info += "NOEMAIL ";
            }

            if (this.AllowUserTalk)
            {
                info += "ALLOWUSERTALK ";
            }

            string[] messageParams =
                {
                    this.Id, this.Target, this.BlockedBy, this.Expiry, this.Start, this.BlockReason, 
                    info
                };
            string message = ms.RetrieveMessage("blockInfoShort", null, messageParams);

            if (message == emptyMessage)
            {
                message = ms.RetrieveMessage("noBlocks", null, null);
            }

            return message;
        }

        #endregion
    }
}