#region Usings

using System;
using System.Reflection;
using System.Xml;

#endregion

namespace helpmebot6.Commands
{
    /// <summary>
    ///   Returns the user rights of a wikipedian
    /// </summary>
    internal class Rights : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            CommandResponseHandler crh = new CommandResponseHandler();
            if (args.Length > 0)
            {
                string username = string.Join(" ", args);
                string rights = getRights(username, channel);


                string message;
                if (rights != "")
                {
                    string[] messageParameters = {username, rights};
                    message = Configuration.singleton().getMessage("cmdRightsList", messageParameters);
                }
                else
                {
                    string[] messageParameters = {username};
                    message = Configuration.singleton().getMessage("cmdRightsNone", messageParameters);
                }

                crh.respond(message);
            }
            else
            {
                string[] messageParameters = {"rights", "1", args.Length.ToString()};

                Helpmebot6.irc.ircNotice(source.nickname,
                                         Configuration.singleton().getMessage("notEnoughParameters", messageParameters));
            }
            return crh;
        }


        public string getRights(string username, string channel)
        {
            if (username == string.Empty)
            {
                throw new ArgumentNullException();
            }
            string baseWiki = Configuration.singleton().retrieveLocalStringOption("baseWiki", channel);

            DAL.Select q = new DAL.Select("site_api");
            q.setFrom("site");
            q.addWhere(new DAL.WhereConds("site_id", baseWiki));
            string api = DAL.singleton().executeScalarSelect(q);

            string returnStr = "";
            int rightsCount = 0;
            XmlTextReader creader =
                new XmlTextReader(
                    HttpRequest.get(api + "?action=query&list=users&usprop=groups&format=xml&ususers=" + username));
            do
                creader.Read(); while (creader.Name != "user");
            creader.Read();
            if (creader.Name == "groups") //the start of the group list
            {
                do
                {
                    creader.Read();
                    string rightsList = (creader.ReadString());
                    if (rightsList != "")
                        returnStr = returnStr + rightsList + ", ";
                    rightsCount = rightsCount + 1;
                } while (creader.Name == "g"); //each group should be added
            }
            returnStr = rightsCount == 0 ? "" : returnStr.Remove(returnStr.Length - 2);


            return returnStr;
        }
    }
}