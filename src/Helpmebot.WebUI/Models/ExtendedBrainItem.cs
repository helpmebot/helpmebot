namespace Helpmebot.WebUI.Models
{
    using System.Collections.Generic;
    using System.Text;
    using System.Web;
    using Helpmebot.WebApi.TransportModels;

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
            var colourLookup = new Dictionary<int, string>
            {
                { 0, "white" },
                { 1, "black" },
                { 2, "blue" },
                { 3, "green" },
                { 4, "light-red" },
                { 5, "brown" },
                { 6, "purple" },
                { 7, "orange" },
                { 8, "yellow" },
                { 9, "light-green" },
                { 10, "cyan" },
                { 11, "light-cyan" },
                { 12, "light-blue" },
                { 13, "pink" },
                { 14, "grey" },
                { 15, "light-grey" },
            };
            
            var builder = new StringBuilder();

            const char BoldMarker = (char)0x02;
            var boldActive = false;
            
            const char ItalicMarker = (char)0x1d;
            var italicActive = false;
            
            const char UnderlineMarker = (char)0x1f;
            var underlineActive = false;
            
            const char StrikethroughMarker = (char)0x1e;
            var strikethroughActive = false;
            
            const char MonospaceMarker = (char)0x11;
            var monospaceActive = false;
            
            const char ColourMarker = (char)0x03;
            string colorsActive = null;
            
            const char ClearMarker = (char)0x0f;

            void CloseTag()
            {
                if (boldActive || italicActive || underlineActive || strikethroughActive || monospaceActive || !string.IsNullOrWhiteSpace(colorsActive))
                {
                    builder.Append("</span>");
                }
            }
            void OpenTag()
            {
                if (boldActive || italicActive || underlineActive || strikethroughActive || monospaceActive || !string.IsNullOrWhiteSpace(colorsActive))
                {
                    var classes = new List<string>(4);

                    if (boldActive)
                    {
                        classes.Add("irc-bold");
                    }

                    if (italicActive)
                    {
                        classes.Add("irc-italic");
                    }

                    if (underlineActive)
                    {
                        classes.Add("irc-underline");
                    }

                    if (strikethroughActive)
                    {
                        classes.Add("irc-strikethrough");
                    }

                    if (monospaceActive)
                    {
                        classes.Add("irc-monospace");
                    }

                    if (!string.IsNullOrWhiteSpace(colorsActive))
                    {
                        classes.Add(colorsActive);
                    }

                    builder.Append("<span class=\"");
                    builder.Append(string.Join(' ', classes));
                    builder.Append("\">");
                }
            }
            bool IsDigit(char c)
            {
                return c is >= '0' and <= '9';
            }
            bool IsComma(char c)
            {
                return c == ',';
            }

            var messageChars = HttpUtility.HtmlEncode(this.Response).ToCharArray();
            for (var i = 0; i < messageChars.Length; i++)
            {
                switch (messageChars[i])
                {
                    case BoldMarker:
                        this.HasFormatting = true;
                        CloseTag();
                        boldActive = !boldActive;
                        OpenTag();
                        break;
                    case ItalicMarker:
                        this.HasFormatting = true;
                        CloseTag();
                        italicActive = !italicActive;
                        OpenTag();
                        break;
                    case UnderlineMarker:
                        this.HasFormatting = true;
                        CloseTag();
                        underlineActive = !underlineActive;
                        OpenTag();
                        break;
                    case StrikethroughMarker:
                        this.HasFormatting = true;
                        CloseTag();
                        strikethroughActive = !strikethroughActive;
                        OpenTag();
                        break;
                    case MonospaceMarker:
                        this.HasFormatting = true;
                        CloseTag();
                        monospaceActive = !monospaceActive;
                        OpenTag();
                        break;
                    case ColourMarker:
                        this.HasFormatting = true;
                        CloseTag();
                        if (!IsDigit(messageChars[i + 1]) && !IsComma(messageChars[i + 1]))
                        {
                            colorsActive = null;
                            OpenTag();
                            break;
                        }

                        string foreground = "", background = "";
                        
                        if (IsDigit(messageChars[i + 1]))
                        {
                            if (IsDigit(messageChars[i + 2]))
                            {
                                // two-digit colour
                                if (colourLookup.TryGetValue((messageChars[i + 1] - '0') * 10 + (messageChars[i + 2] - '0'), out var fgKey))
                                {
                                    foreground = "irc-fg-" + fgKey;
                                }

                                i += 2;
                            }
                            else
                            {
                                // two-digit colour
                                if (colourLookup.TryGetValue((messageChars[i + 1] - '0'), out var fgKey))
                                {
                                    foreground = "irc-fg-" + fgKey;
                                }

                                i++;
                            }
                        }

                        if (IsComma(messageChars[i + 1]))
                        {
                            // bump the comma;
                            i++;
                            
                            if (IsDigit(messageChars[i + 2]))
                            {
                                // two-digit colour
                                if (colourLookup.TryGetValue((messageChars[i + 1] - '0') * 10 + (messageChars[i + 2] - '0'), out var bgKey))
                                {
                                    background = "irc-bg-" + bgKey;
                                }

                                i += 2;
                            }
                            else
                            {
                                // two-digit colour
                                if (colourLookup.TryGetValue((messageChars[i + 1] - '0'), out var bgKey))
                                {
                                    background = "irc-bg-" + bgKey;
                                }

                                i++;
                            }
                        }

                        colorsActive = (foreground + " " + background).Trim();
                        
                        OpenTag();
                        break;
                    case ClearMarker:
                        this.HasFormatting = true;
                        CloseTag();
                        boldActive = false;
                        italicActive = false;
                        underlineActive = false;
                        strikethroughActive = false;
                        monospaceActive = false;
                        colorsActive = null;
                        break;
                    default:
                        builder.Append(messageChars[i]);
                        break;
                }
            }
            
            this.HasFormatting = this.HasFormatting.GetValueOrDefault(false);
            this.HtmlFormatted = builder.ToString();
        }
        
        public bool? HasFormatting { get; set; }
        public string HtmlFormatted { get; set; }
    }
}