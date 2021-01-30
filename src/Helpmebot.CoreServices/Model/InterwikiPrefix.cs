// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InterwikiPrefix.cs" company="Helpmebot Development Team">
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
    using Helpmebot.Persistence.Interfaces;

    /// <summary>
    ///     The inter-wiki link prefix.
    /// </summary>
    public class InterwikiPrefix : IDatabaseEntity
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets a value indicating whether scary transclusion is allowed.
        /// </summary>
        public virtual bool AllowTransclusion { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is local.
        /// </summary>
        public virtual bool IsLocal { get; set; }

        /// <summary>
        ///     Gets or sets the prefix.
        /// </summary>
        public virtual string Prefix { get; set; }

        /// <summary>
        ///     Gets or sets the url.
        /// </summary>
        public virtual byte[] Url { get; set; }

        #endregion
    }
}