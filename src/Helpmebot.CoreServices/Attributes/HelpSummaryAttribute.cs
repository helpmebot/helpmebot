namespace Helpmebot.CoreServices.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class HelpSummaryAttribute : Attribute
    {
        public string Description { get; }

        public HelpSummaryAttribute(string description)
        {
            this.Description = description;
        }
    }
}