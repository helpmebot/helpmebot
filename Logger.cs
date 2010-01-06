using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace helpmebot6
{
    class Logger
    {
        private static Logger _instance;
        protected Logger( )
        {
            DalLogger = new StreamWriter( "dal.log" );
            IRClogger = new StreamWriter( "irc.log" );
            IalLogger = new StreamWriter( "ial.log" );
            ErrorLogger = new StreamWriter( "error.log" );

            DalLogger.AutoFlush = true;
            IRClogger.AutoFlush = true;
            IalLogger.AutoFlush = true;
            ErrorLogger.AutoFlush = true;

            string init = "Welcome to Helpmebot v6.";
            DalLogger.WriteLine( init );
            IRClogger.WriteLine( init );
            IalLogger.WriteLine( init );
            ErrorLogger.WriteLine( init );

            addToLog( init , LogTypes.GENERAL );
        }
        public static Logger Instance( )
        {
            if( _instance == null )
                _instance = new Logger( );
            return _instance;
        }

        private StreamWriter DalLogger;
        private StreamWriter IalLogger;
        private StreamWriter IRClogger;
        private StreamWriter ErrorLogger;

        private bool logDal;
        private bool logIRC;

        public bool LogDAL
        {
            get
            {
                return logDal;
            }
            set
            {
                logDal = value;
            }
        }
        public bool LogIRC
        {
            get
            {
                return logIRC;
            }
            set
            {
                logIRC = value;
            }
        }

        public enum LogTypes
        {
            DNWB ,// dotnetwikibot, GREY, no choice
            DAL , // Database stuff, MAGENTA
            IAL , // IRC stuff YELLOW
            COMMAND , // command log events, BLUE
            GENERAL , // general log events, WHITE
            ERROR, // error events, RED
            IRC // raw IRC events, 
        } // DATE: GREEN

        public void addToLog( string message , LogTypes type )
        {
            lock( this )
            {
                Console.ResetColor( );

                string dateString = "[ " + DateTime.Now.ToShortDateString( ) + " " + DateTime.Now.ToLongTimeString( ) + " ] ";

                switch( type )
                {
                    case LogTypes.DNWB:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write( dateString );
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine( "A " + message );
                        break;
                    case LogTypes.DAL:
                        if( logDal )
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write( dateString );
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            Console.WriteLine( "D " + message );
                        }
                        DalLogger.WriteLine( dateString + message );
                        break;
                    case LogTypes.IAL:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write( dateString );
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        IalLogger.WriteLine( dateString + message );
                        Console.WriteLine( "I " + message );
                        break;
                    case LogTypes.COMMAND:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write( dateString );
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine( "C " + message );
                        break;
                    case LogTypes.GENERAL:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write( dateString );
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine( "G " + message );
                        break;
                    case LogTypes.ERROR:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write( dateString );
                        Console.ForegroundColor = ConsoleColor.Red;
                        ErrorLogger.WriteLine( dateString + message );
                        Console.WriteLine( "E " + message );
                        break;
                    case LogTypes.IRC:
                        if( logIRC )
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write( dateString );
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.BackgroundColor = ConsoleColor.Yellow;
                            Console.WriteLine( "R " + message );
                        }
                        IRClogger.WriteLine( dateString + message );

                        break;
                    default:
                        break;
                }
                Console.ResetColor( );
            }
        }


    }
}
