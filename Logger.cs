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
using System.IO;
using System.Net.Sockets;
using System.Text;

#endregion

namespace helpmebot6
{
    /// <summary>
    /// Logger
    /// </summary>
    internal class Logger
    {
        private static Logger _instance;

        protected Logger()
        {
            this._dalLogger = new StreamWriter("dal.log");
            this._ircLogger = new StreamWriter("irc.log");
            this._ialLogger = new StreamWriter("ial.log");
            this._errorLogger = new StreamWriter("error.log");


            this._dalLogger.AutoFlush = true;
            this._ircLogger.AutoFlush = true;
            this._ialLogger.AutoFlush = true;
            this._errorLogger.AutoFlush = true;

            const string init = "Welcome to Helpmebot v6.";
            this._dalLogger.WriteLine(init);
            this._ircLogger.WriteLine(init);
            this._ialLogger.WriteLine(init);
            this._errorLogger.WriteLine(init);

            addToLog(init, LogTypes.General);

            this.copyToUdp = int.Parse(Configuration.singleton()["udpLogPort"]);
        }

        public static Logger instance()
        {
            return _instance ?? ( _instance = new Logger( ) );
        }

        private readonly StreamWriter _dalLogger;
        private readonly StreamWriter _ialLogger;
        private readonly StreamWriter _ircLogger;
        private readonly StreamWriter _errorLogger;

        /// <summary>
        /// Gets or sets a value indicating whether [log DAL].
        /// </summary>
        /// <value><c>true</c> if [log DAL]; otherwise, <c>false</c>.</value>
        public bool logDAL { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [log irc].
        /// </summary>
        /// <value><c>true</c> if [log irc]; otherwise, <c>false</c>.</value>
        public bool logIrc { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [log dal lock].
        /// </summary>
        /// <value><c>true</c> if [log dal lock]; otherwise, <c>false</c>.</value>
        public bool logDalLock { get; set; }

        public int copyToUdp { get; set; }

        /// <summary>
        /// Log types
        /// </summary>
        public enum LogTypes
        {
            /// <summary>
            /// dotnetwikibot, GREY, no choice
            /// </summary>
            DNWB, // 
            /// <summary>
            /// Database stuff, MAGENTA
            /// </summary>
            DAL, // 
            /// <summary>
            /// IRC stuff YELLOW
            /// </summary>
            IAL, // 
            /// <summary>
            /// command log events, BLUE
            /// </summary>
            Command, // 
            /// <summary>
            /// general log events, WHITE
            /// </summary>
            General, // 
            /// <summary>
            /// error events, RED
            /// </summary>
            Error, // 
            /// <summary>
            /// raw IRC events,
            /// </summary>
            IRC, //  
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
                        udpLog(dateString + "A " + message);
                        break;
                    case LogTypes.DalLock:
                        if (this.logDalLock)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write(dateString);
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            Console.WriteLine("DL " + message);
                            udpLog(dateString + "DL " + message);
                        }
                        this._dalLogger.WriteLine(dateString + message);
                        break;
                    case LogTypes.DAL:
                        if (this.logDAL)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write(dateString);
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            Console.WriteLine("D " + message);
                            udpLog(dateString + "D " + message);
                        }
                        this._dalLogger.WriteLine(dateString + message);
                        break;
                    case LogTypes.IAL:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(dateString);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        this._ialLogger.WriteLine(dateString + message);
                        Console.WriteLine("I " + message);
                        udpLog(dateString + "I " + message);
                        break;
                    case LogTypes.Command:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(dateString);
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("C " + message);
                        udpLog(dateString + "C " + message);
                        break;
                    case LogTypes.General:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(dateString);
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("G " + message);
                        udpLog(dateString + "G " + message);
                        break;
                    case LogTypes.Error:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(dateString);
                        Console.ForegroundColor = ConsoleColor.Red;
                        this._errorLogger.WriteLine(dateString + message);
                        Console.WriteLine("E " + message);
                        udpLog(dateString + "E " + message);
                        break;
                    case LogTypes.IRC:
                        if (this.logIrc)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write(dateString);
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.BackgroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("R " + message);
                            udpLog(dateString + "R " + message);
                        }
                        this._ircLogger.WriteLine(dateString + message);

                        break;
                    default:
                        break;
                }
                Console.ResetColor();
            }
        }

        private void udpLog(string message)
        {
            if( 1025 <= copyToUdp && copyToUdp <= 65535 )
            {
                UdpClient udp = new UdpClient("helpmebot.org.uk", copyToUdp);
                byte[] messageBytes = Encoding.ASCII.GetBytes(message);
                udp.Send(messageBytes, messageBytes.Length);
            }
        }
    }
}