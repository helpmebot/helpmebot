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
// <summary>
//   Holds the block information of a specific user
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Model
{
    using Helpmebot.Services.Interfaces;

    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    /// Holds the block information of a specific user
    /// </summary>
    public struct BlockInformation
    {
        private string id;

        public string target;

        public string blockedBy;

        public string blockReason;

        public string expiry;

        public string start;

        public bool nocreate;

        public bool autoblock;

        public bool noemail;

        public bool allowusertalk;

        public string Id
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = value;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this block.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this block.
        /// </returns>
        /// <remarks>
        /// TODO: fixes for context, localisation, etc,
        /// </remarks>
        public override string ToString()
        {
            var ms = ServiceLocator.Current.GetInstance<IMessageService>();

            string[] emptyMessageParams = { string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty };
            string emptyMessage = ms.RetrieveMessage("blockInfoShort", null, emptyMessageParams);

            string info = string.Empty;

            if (this.nocreate)
            {
                info += "NOCREATE ";
            }

            if (this.autoblock)
            {
                info += "AUTOBLOCK ";
            }

            if (this.noemail)
            {
                info += "NOEMAIL ";
            }

            if (this.allowusertalk)
            {
                info += "ALLOWUSERTALK ";
            }

            string[] messageParams =
                {
                    this.Id, this.target, this.blockedBy, this.expiry, this.start, this.blockReason,
                    info
                };
            string message = ms.RetrieveMessage("blockInfoShort", null, messageParams);

            if (message == emptyMessage)
            {
                message = ms.RetrieveMessage("noBlocks", null, null);
            }

            return message;
        }
    }
}
