// /****************************************************************************
//  *   This file is part of Helpmebot.                                        *
//  *                                                                          *
//  *   Helpmebot is free software: you can redistribute it and/or modify      *
//  *   it under the terms of the GNU General Public License as published by   *
//  *   the Free Software Foundation, either version 3 of the License, or      *
//  *   (at your option) any later version.                                    *
//  *                                                                          *
//  *   Helpmebot is distributed in the hope that it will be useful,           *
//  *   but WITHOUT ANY WARRANTY; without even the implied warranty of         *
//  *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the          *
//  *   GNU General Public License for more details.                           *
//  *                                                                          *
//  *   You should have received a copy of the GNU General Public License      *
//  *   along with Helpmebot.  If not, see <http://www.gnu.org/licenses/>.     *
//  ****************************************************************************/
#region Usings

using System;
using System.Reflection;

#endregion

namespace helpmebot6.AI
{
    /// <summary>
    /// 
    /// </summary>
    internal class Intelligence
    {
        /// <summary>
        /// Holds the singleton instance of this class.
        /// </summary>
        private static Intelligence _singleton;

        /// <summary>
        /// Returns the singleton instance of this class.
        /// </summary>
        /// <returns>single instance</returns>
        public static Intelligence singleton()
        {
            return _singleton ?? ( _singleton = new Intelligence( ) );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Intelligence"/> class.
        /// </summary>
        protected Intelligence()
        {
        }

        private DateTime lastAiResponse = DateTime.MinValue;

            /// <summary>
        /// Responds to the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public string respond(string input)
        {
            if(DateTime.Now.AddMinutes(-1  ) > lastAiResponse)
            {
                lastAiResponse = DateTime.Now;
                return getResponse( input );

            }
            return "";
        }

        private string getResponse( string input )
        {
            string[] tokens = input.Split(' ');

            string[] hello = {"hi", "hey", "heya", "hello"};
            const string helloResponses = "cmdSayHi1";
            string[] morning = {"morning", "good morning"};
            const string morningResponses = "cmdSayHiMorning";
            string[] afternoon = {"afternoon", "good afternoon"};
            const string afternoonResponses = "cmdSayHiAfternoon";
            string[] evening = {"evening", "good evening"};
            const string eveningResponses = "cmdSayHiEvening";

            foreach (string word in hello)
            {
                if ( tokens.Length <= 1 ) continue;
                if (tokens[0] == word && tokens[1] == Helpmebot6.irc.ircNickname)
                {
                    return helloResponses;
                }
            }

            foreach (string word in morning)
            {
                if (tokens.Length > 1)
                {
                    if (tokens[0] == word && tokens[1] == Helpmebot6.irc.ircNickname)
                    {
                        return morningResponses;
                    }
                }
            }

            foreach (string word in afternoon)
            {
                if (tokens.Length > 1)
                {
                    if (tokens[0] == word && tokens[1] == Helpmebot6.irc.ircNickname)
                    {
                        return afternoonResponses;
                    }
                }
            }

            foreach (string word in evening)
            {
                if (tokens.Length > 1)
                {
                    if (tokens[0] == word && tokens[1] == Helpmebot6.irc.ircNickname)
                    {
                        return eveningResponses;
                    }
                }
            }

            if (input.Contains("Ayn Rand"))
                return "aynrand";

            return "";
        }
    }
}