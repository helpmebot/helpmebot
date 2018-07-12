namespace Helpmebot.Exceptions
{
    using System;

    public class MediawikiApiException : Exception
    {
        public MediawikiApiException(string message) : base (message)
        {
        }
        public MediawikiApiException(string message, Exception ex) : base (message, ex)
        {
        }   
    }
}