// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GitRevisionAttribute.cs" company="Helpmebot Development Team">
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
//   The git revision attribute.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Attributes
{
    using System;

    /// <summary>
    /// The git revision attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class GitRevisionAttribute : Attribute
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="GitRevisionAttribute"/> class.
        /// </summary>
        /// <param name="revisionInfo">
        /// The revision info.
        /// </param>
        public GitRevisionAttribute(string revisionInfo)
        {
            this.RevisionInfo = revisionInfo;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The revision info.
        /// </summary>
        public string RevisionInfo { get; private set; }

        #endregion
    }
}