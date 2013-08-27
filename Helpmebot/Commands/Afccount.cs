// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Afccount.cs" company="Helpmebot Development Team">
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
//   Returns the number of articles currently waiting at Articles for Creation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    /// <summary>
    /// Returns the number of articles currently waiting at Articles for Creation
    /// </summary>
    internal class Afccount : Categorysize
    {
        protected override CommandResponseHandler ExecuteCommand(User source, string channel, string[] args)
        {
            return getResultOfCommand("Pending AfC submissions", channel);
        }
    }
}