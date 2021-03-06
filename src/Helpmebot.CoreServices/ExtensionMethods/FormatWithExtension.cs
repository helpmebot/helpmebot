﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FormatWithExtension.cs" company="Helpmebot Development Team">
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
//   Defines the FormatWithExtension type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.CoreServices.ExtensionMethods
{
    using System;
    using System.Collections.Generic;
    using System.Web;

    public static class FormatWithExtension
    {
        public static string FormatWith(this string format, IDictionary<string, object> source)
        {
            if (format == null)
            {
                throw new ArgumentNullException("format");
            }

            string result = format;

            foreach (KeyValuePair<string, object> o in source)
            {
                var value = o.Value.ToString();
                    
                // plain text
                result = result.Replace("{" + o.Key + "}", value);
                
                // url encoding
                value = HttpUtility.UrlEncode(value);
                result = result.Replace("{" + o.Key + ":url}", value);
                
                // backtrack a bit for MW title encoding
                value = value
                    .Replace("+", "_")
                    .Replace("%2f", "/")
                    .Replace("%3a", ":");
                result = result.Replace("{" + o.Key + ":title}", value);
            }

            return result;
        }
    }
}
