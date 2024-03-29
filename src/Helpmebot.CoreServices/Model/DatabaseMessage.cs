namespace Helpmebot.CoreServices.Model
{
    using System;
    using Helpmebot.Persistence;

    public class DatabaseMessage : EntityBase
    {
        public virtual string ContextType { get; set; }
        public virtual string Context { get; set; }
        public virtual string MessageKey { get; set; }
        public virtual string Value { get; set; }
        
        /// <summary>
        /// Value format; futureproofing 
        /// </summary>
        public virtual int Format { get; set; }
        public virtual DateTime LastUpdated { get; set; }
    }
}