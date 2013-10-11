// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Logger.cs" company="Helpmebot Development Team">
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
//   Logger
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot
{
    using System;

    /// <summary>
    /// Logger
    /// </summary>
    internal class Logger
    {
        private static Logger _instance;

        protected Logger()
        {
            const string init = "Welcome to Helpmebot v6.";

            this.addToLog(init, LogTypes.General);
        }

        public static Logger instance()
        {
            return _instance ?? ( _instance = new Logger( ) );
        }

        /// <summary>
        /// Log types
        /// </summary>
        public enum LogTypes
        {
            /// <summary>
            /// dotnetwikibot, GREY, no choice
            /// </summary>
            DNWB,

            /// <summary>
            /// Database stuff, MAGENTA
            /// </summary>
            DAL, 

            /// <summary>
            /// IRC stuff YELLOW
            /// </summary>
            IAL,

            /// <summary>
            /// command log events, BLUE
            /// </summary>
            Command,

            /// <summary>
            /// general log events, WHITE
            /// </summary>
            General,

            /// <summary>
            /// error events, RED
            /// </summary>
            Error, 

            /// <summary>
            /// raw IRC events,
            /// </summary>
            IRC, 

            /// <summary>
            /// ?
            /// </summary>
            DalLock
        }

        // DATE: GREEN

        /// <summary>
        /// Adds to log.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="type">The type.</param>
        public void addToLog(string message, LogTypes type)
        {
            lock (this)
            {
                Console.ResetColor();

                string dateString = "[ " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() +
                                    " ] ";

                switch (type)
                {
                    case LogTypes.DNWB:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(dateString);
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("A " + message);
                        break;
                    case LogTypes.DalLock:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(dateString);
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.WriteLine("DL " + message);
                        break;
                    case LogTypes.DAL:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(dateString);
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.WriteLine("D " + message); 
                        break;
                    case LogTypes.IAL:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(dateString);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("I " + message);
                        break;
                    case LogTypes.Command:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(dateString);
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("C " + message);
                        break;
                    case LogTypes.General:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(dateString);
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("G " + message);
                        break;
                    case LogTypes.Error:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(dateString);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("E " + message);
                        break;
                    case LogTypes.IRC:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(dateString);
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.BackgroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("R " + message);
                        break;
                }

                Console.ResetColor();
            }
        }
    }
}