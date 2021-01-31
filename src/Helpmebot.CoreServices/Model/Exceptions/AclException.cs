namespace Helpmebot.Exceptions
{
    using System;

    public class AclException : Exception
    {
        public AclException(string message) : base(message)
        {
        }
    }
}