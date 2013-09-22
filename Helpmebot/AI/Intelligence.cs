// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Intelligence.cs" company="Helpmebot Development Team">
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
//   Defines the Intelligence type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.AI
{
    using System;

    using Helpmebot;

    /// <summary>
    /// The intelligence.
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
        public static Intelligence Singleton()
        {
            return _singleton ?? (_singleton = new Intelligence());
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="Intelligence"/> class.
        /// </summary>
        protected Intelligence()
        {
        }

        /// <summary>
        /// The last ai response.
        /// </summary>
        private DateTime lastAiResponse = DateTime.MinValue;

        /// <summary>
        /// The respond.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string Respond(string input)
        {
            if (DateTime.Now.AddMinutes(-1) > this.lastAiResponse)
            {
                this.lastAiResponse = DateTime.Now;
                return this.getSpecialResponse(input);
            }

            return this.getStandardResponse(input);
        }

        /// <summary>
        /// The get standard response.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string getStandardResponse(string input)
        {
            string[] tokens = input.Split(' ');

            string[] hello = { "hi", "hey", "heya", "hello" };
            const string helloResponses = "cmdSayHi1";
            string[] morning = { "morning", "good morning" };
            const string morningResponses = "cmdSayHiMorning";
            string[] afternoon = { "afternoon", "good afternoon" };
            const string afternoonResponses = "cmdSayHiAfternoon";
            string[] evening = { "evening", "good evening" };
            const string eveningResponses = "cmdSayHiEvening";

            foreach (string word in hello)
            {
                if (tokens.Length <= 1)
                {
                    continue;
                }

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

            return string.Empty;
        }

        /// <summary>
        /// The get special response.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string getSpecialResponse(string input)
        {
            if (input.Contains("Ayn Rand"))
            {
                return "aynrand";
            }

            return string.Empty;
        }
    }
}