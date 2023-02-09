namespace Helpmebot.CoreServices.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class ForceDocumentedAttribute : Attribute
    {
        public bool PromoteAliases { get; }

        public ForceDocumentedAttribute(bool promoteAliases = false)
        {
            this.PromoteAliases = promoteAliases;
        }
    }
}