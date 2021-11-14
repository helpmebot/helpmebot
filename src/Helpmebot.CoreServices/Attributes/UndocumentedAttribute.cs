namespace Helpmebot.CoreServices.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Class |  AttributeTargets.Method)]
    public class UndocumentedAttribute : Attribute
    {
    }
}