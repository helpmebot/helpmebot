namespace Helpmebot.CoreServices.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class HelpSummaryMethodAttribute : Attribute
    {
        public string MethodName { get; }

        public HelpSummaryMethodAttribute(string methodName)
        {
            this.MethodName = methodName;
        }
    }
}