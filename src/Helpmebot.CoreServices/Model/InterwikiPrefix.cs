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

    public class InterwikiPrefix : IDatabaseEntity
    {
        public virtual int Id { get; set; }
        public virtual string Prefix { get; set; }

        public virtual string Url { get; set; }
        
        public virtual string ImportedAs { get; set; }

        public virtual bool AbsentFromLastImport { get; set; }
        public virtual bool CreatedSinceLast { get; set; }
    }
}