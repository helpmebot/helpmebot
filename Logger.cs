#region Usings

using System;
using System.IO;

#endregion

namespace helpmebot6
{
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
        }

        public static Logger instance()
        {
            return _instance ?? ( _instance = new Logger( ) );
        }

        private readonly StreamWriter _dalLogger;
        private readonly StreamWriter _ialLogger;
        private readonly StreamWriter _ircLogger;
        private readonly StreamWriter _errorLogger;

        public bool logDAL { get; set; }

        public bool logIrc { get; set; }

        public bool logDalLock { get; set; }

        public enum LogTypes
        {
            DNWB, // dotnetwikibot, GREY, no choice
            DAL, // Database stuff, MAGENTA
            IAL, // IRC stuff YELLOW
            Command, // command log events, BLUE
            General, // general log events, WHITE
            Error, // error events, RED
            IRC, // raw IRC events, 
            DalLock
        }

        // DATE: GREEN

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
                        if (this.logDalLock)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write(dateString);
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            Console.WriteLine("DL " + message);
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
                        }
                        this._dalLogger.WriteLine(dateString + message);
                        break;
                    case LogTypes.IAL:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(dateString);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        this._ialLogger.WriteLine(dateString + message);
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
                        this._errorLogger.WriteLine(dateString + message);
                        Console.WriteLine("E " + message);
                        break;
                    case LogTypes.IRC:
                        if (this.logIrc)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write(dateString);
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.BackgroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("R " + message);
                        }
                        this._ircLogger.WriteLine(dateString + message);

                        break;
                    default:
                        break;
                }
                Console.ResetColor();
            }
        }
    }
}