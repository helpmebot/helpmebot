namespace Helpmebot.Brain.Services
{
    using System;
    using System.Text.RegularExpressions;
    using Helpmebot.Brain.Services.Interfaces;
    using Microsoft.CSharp.RuntimeBinder;

    public class SedExpressionService  : ISedExpressionService
    {
        private Regex overallMatch = new Regex("^(?<mode>.)(?<delim>.)(?<expr>(?:\\\\\\k<delim>|(?!\\k<delim>).)*)\\k<delim>(?<repl>(?:\\\\\\k<delim>|(?!\\k<delim>).)*)\\k<delim>(?<flags>[gi]*)");
        
        public string Apply(string input, string expression)
        {
            var (searchExpression, replacement, flags) = this.ParseExpression(expression);

            if (searchExpression == null || replacement == null)
            {
                return input;
            }

            var r = new Regex(searchExpression, flags.Contains("i") ? RegexOptions.IgnoreCase : RegexOptions.None);

            if (flags.Contains("g"))
            {
                return r.Replace(input, replacement);
            }
            else
            {
                return r.Replace(input, replacement, 1);
            }
        }

        private (string, string, string) ParseExpression(string expression)
        {
            var match = this.overallMatch.Match(expression);
            if (!match.Success)
            {
                return (null, null, null);
            }
            
            if (match.Groups["mode"].Value != "s")
            {
                throw new NotImplementedException($"Sed mode {expression[0]} is not available.");
            }

            return (match.Groups["expr"].Value, match.Groups["repl"].Value, match.Groups["flags"].Value);
        }
    }
}