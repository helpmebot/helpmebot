// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlagGroupUser.cs" company="Helpmebot Development Team">
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
//   The flag group user.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Model
{
    using System;
    using Helpmebot.Persistence;

    /// <summary>
    /// The flag group user.
    /// </summary>
    public class FlagGroupUser : EntityBase
    {
        public virtual User User { get; set; }

        public virtual FlagGroup FlagGroup { get; set; }

        public virtual DateTime LastModified { get; set; }
    }
}
