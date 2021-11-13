namespace Helpmebot.CoreServices.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Field)]
    public class FlagHelpAttribute : Attribute
    {
        public string QuickHelpText { get; }
        public string DetailedHelp { get; }

        public FlagHelpAttribute(string quickHelpText, string detailedHelp = null)
        {
            this.QuickHelpText = quickHelpText;
            this.DetailedHelp = detailedHelp;
        }
    }
}