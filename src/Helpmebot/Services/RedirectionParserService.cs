using System.Collections.Generic;
using System.Linq;
using FluentNHibernate.Conventions;
using Helpmebot.Services.Interfaces;
using NHibernate.Cfg.ConfigurationSchema;

namespace Helpmebot.Services
{
    public class RedirectionParserService : IRedirectionParserService
    {
        public RedirectionParserResult Parse(IEnumerable<string> messageData)
        {
            var originalMessage = messageData.ToList();
            var newMessage = new List<string>();
            var redirection = new List<string>();

            bool isRedirecting = false;

            foreach (var section in originalMessage)
            {
                // firstly, check if we're currently in a redirection context.
                if (isRedirecting)
                {
                    // OK, handle the redirection
                    redirection.Add(section);
                    isRedirecting = false;
                    continue;
                }

                // are we about to go into a redirection context?
                if (section == ">")
                {
                    isRedirecting = true;
                    continue;
                }

                // are we going old-style and not including the space?
                if (section.StartsWith(">") && !section.StartsWith(">>"))
                {
                    redirection.Add(section.Substring(1));
                    continue;
                }

                // not in any form of redirection context, so skip for now.
                newMessage.Add(section);
            }

            // We're in a redirection context, but now at the end of the message.
            // This is almost certainly an errorneous input, but handle it sanely anyway
            if (isRedirecting)
            {
                newMessage.Add(">");
            }

            return new RedirectionParserResult(newMessage, string.Join(", ", redirection));
        }

        public class RedirectionParserResult
        {
            public IEnumerable<string> Message { get; private set; }
            public string Destination { get; private set; }

            public RedirectionParserResult(IEnumerable<string> message, string destination)
            {
                this.Message = message;
                this.Destination = destination;
            }
        }
    }
}