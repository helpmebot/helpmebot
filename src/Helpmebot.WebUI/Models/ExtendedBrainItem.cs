namespace Helpmebot.WebUI.Models
{
    using System.Collections.Generic;
    using System.Text;
    using System.Web;
    using Helpmebot.WebApi.TransportModels;
    using Helpmebot.WebUI.ExtensionMethods;

    public class ExtendedBrainItem : BrainItem
    {
        public ExtendedBrainItem(BrainItem item)
        {
            this.Keyword = item.Keyword;
            this.Response = item.Response;
            this.IsAction = item.IsAction;
        }

        public void Parse()
        {
            var inputText = this.Response;

            this.HtmlFormatted = inputText.ConvertIrcFormattingToWeb(out var hasFormatting);
            this.HasFormatting = hasFormatting;
        }
        
        public bool? HasFormatting { get; set; }
        public string HtmlFormatted { get; set; }
    }
}