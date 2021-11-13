namespace Helpmebot.WebUI
{
    using System.Linq;
    using Helpmebot.CoreServices.Attributes;
    using Helpmebot.CoreServices.Model;

    public static class FlagHelpHelper
    {
        public static string GetHelpForFlag(string flag)
        {
            var fieldInfos = typeof(Flags).GetFields().Where(x => x.IsLiteral && x.FieldType == typeof(string));
            
            foreach (var fieldInfo in fieldInfos)
            {
                if ((string)fieldInfo.GetRawConstantValue() == flag)
                {
                    var flagHelpAttr = fieldInfo.GetCustomAttributes(typeof(FlagHelpAttribute), false).Cast<FlagHelpAttribute>().FirstOrDefault();
                    if (flagHelpAttr != null)
                    {
                        return flagHelpAttr.QuickHelpText;
                    }
                }
            }

            return "Unknown or undocumented flag";
        }
    }
}