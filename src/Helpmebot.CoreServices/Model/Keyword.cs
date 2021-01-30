// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Keyword.cs" company="Helpmebot Development Team">
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
//   Defines the Keyword type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Model
{
    using Helpmebot.Model.Interfaces;
    using Helpmebot.Persistence;

    /// <summary>
    /// The keyword.
    /// </summary>
    public class Keyword : EntityBase, ICommandParserEntity
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Gets or sets the response.
        /// </summary>
        public virtual string Response { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether action.
        /// </summary>
        public virtual bool Action { get; set; }

        /// <summary>
        /// The equals.
        /// </summary>
        /// <param name="obj">
        /// The object.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(null, obj))
            {
                return false;
            }

            if (object.ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return this.Equals((Keyword)obj);
        }

        /// <summary>
        /// The get hash code.
        /// </summary>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.Name != null ? this.Name.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (this.Response != null ? this.Response.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ this.Action.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// The equals.
        /// </summary>
        /// <param name="other">
        /// The other.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        protected bool Equals(Keyword other)
        {
            return string.Equals(this.Name, other.Name) && string.Equals(this.Response, other.Response)
                   && this.Action.Equals(other.Action);
        }

        public virtual string CommandKeyword
        {
            get { return this.Name; }
        }

        public virtual string CommandChannel
        {
            get { return null; }
        }
    }
}
