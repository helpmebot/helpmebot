namespace Helpmebot.WebUI
{
    using System;
    using System.Collections.Generic;

    public class FlagHelpHelper
    {
        private readonly Dictionary<string, Tuple<string, string>> flagData;

        public FlagHelpHelper(Dictionary<string, Tuple<string, string>> flagData)
        {
            this.flagData = flagData;
        }
        
        public bool IsKnownFlag(string flag)
        {
            return this.flagData.ContainsKey(flag);
        }

        public string GetHelpForFlag(string flag)
        {
            if (this.flagData.ContainsKey(flag))
            {
                return this.flagData[flag].Item1;
            }

            return "Unknown flag";
        }
        public string GetHelpForFlag(char flag)
        {
            return this.GetHelpForFlag(flag + "");
        }
    }
}