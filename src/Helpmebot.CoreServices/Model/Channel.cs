// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Channel.cs" company="Helpmebot Development Team">
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
    using Helpmebot.Persistence;

    /// <summary>
    /// The channel.
    /// </summary>
    public class Channel : EntityBase
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        public virtual string Password { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether enabled.
        /// </summary>
        public virtual bool Enabled { get; set; }
        
        public virtual bool AutoLink { get; set; }
        public virtual bool Silenced { get; set; }
        public virtual bool HedgehogMode { get; set; }
        
        public virtual string WelcomerFlag { get; set; }
        
        public virtual string BaseWikiId { get; set; }

        /// <summary>
        /// The to string.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }
    }
}