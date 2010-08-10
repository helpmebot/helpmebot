#region Usings

using System;
using System.Reflection;

#endregion

namespace helpmebot6.Commands
{
    /// <summary>
    ///   Talks to the Nubio(squared) API to retrive FAQ information
    /// </summary>
    internal class Faq : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            string command = GlobalFunctions.popFromFront(ref args).ToLower();
            CommandResponseHandler crh = new CommandResponseHandler();

            NubioApi faqRepo = new NubioApi(new Uri(Configuration.singleton().retrieveGlobalStringOption("faqApiUri")));
            string result;
            switch (command)
            {
                case "search":
                    result = faqRepo.searchFaq(string.Join(" ", args));
                    if (result != null)
                    {
                        crh.respond(result);
                    }
                    break;
                case "fetch":
                    result = faqRepo.fetchFaqText(int.Parse(args[0]));
                    if (result != null)
                    {
                        crh.respond(result);
                    }
                    break;
                case "link":
                    result = faqRepo.viewLink(int.Parse(args[0]));
                    if (result != null)
                    {
                        crh.respond(result);
                    }
                    break;
                default:
                    break;
            }

            return crh;
        }
    }
}
