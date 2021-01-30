namespace Helpmebot.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class HelpCategoryAttribute : Attribute
    {
        public string Category { get; private set; }

        public HelpCategoryAttribute(string category)
        {
            this.Category = category;
        }
    }
}