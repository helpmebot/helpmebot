#region Usings

using System.Net;
using System.Reflection;
using System.Xml;

#endregion

namespace helpmebot6.Commands
{
    /// <summary>
    ///   Returns the block information of a wikipedian
    /// </summary>
    internal class Blockinfo : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {

            return new CommandResponseHandler(getBlockInformation(string.Join(" ", args), channel).ToString());
        }

        public BlockInformation getBlockInformation(string userName, string channel)
        {

            IPAddress ip;

            string baseWiki = Configuration.singleton().retrieveLocalStringOption("baseWiki", channel);

            DAL.Select q = new DAL.Select("site_api");
            q.setFrom("site");
            q.addWhere(new DAL.WhereConds("site_id", baseWiki));
            string api = DAL.singleton().executeScalarSelect(q);


            string apiParams = "?action=query&list=blocks&bk";
            if (IPAddress.TryParse(userName, out ip))
            {
                apiParams += "ip";
            }
            else
            {
                apiParams += "users";
            }
            apiParams += "=" + userName + "&format=xml";
            XmlTextReader creader = new XmlTextReader(HttpRequest.get(api + apiParams));

            while (creader.Name != "blocks")
            {
                creader.Read();
            }
            creader.Read();

            if ( creader.Name != "block" )
                return new BlockInformation( );
            BlockInformation bi = new BlockInformation
                                      {
                                          id = creader.GetAttribute( "id" ),
                                          target =
                                              creader.GetAttribute( "user" ),
                                          blockedBy =
                                              creader.GetAttribute( "by" ),
                                          start =
                                              creader.GetAttribute(
                                                  "timestamp" ),
                                          expiry =
                                              creader.GetAttribute( "expiry" ),
                                          blockReason =
                                              creader.GetAttribute( "reason" ),
                                          autoblock =
                                              creader.GetAttribute(
                                                  "autoblock" ) == ""
                                                  ? true
                                                  : false,
                                          nocreate =
                                              creader.GetAttribute(
                                                  "nocreate" ) == ""
                                                  ? true
                                                  : false,
                                          noemail =
                                              creader.GetAttribute(
                                                  "noemail" ) == ""
                                                  ? true
                                                  : false,
                                          allowusertalk =
                                              creader.GetAttribute(
                                                  "allowusertalk" ) == ""
                                                  ? true
                                                  : false
                                      };

            return bi;
        }

        public struct BlockInformation
        {
            public string id;
            public string target;
            public string blockedBy;
            public string blockReason;
            public string expiry;
            public string start;
            public bool nocreate;
            public bool autoblock;
            public bool noemail;
            public bool allowusertalk;

            public override string ToString()
            {

                string[] emptyMessageParams = {"", "", "", "", "", "", ""};
                string emptyMessage = Configuration.singleton().getMessage("blockInfoShort", emptyMessageParams);

                string info = "";
                if (nocreate)
                    info += "NOCREATE ";
                if (autoblock)
                    info += "AUTOBLOCK ";
                if (noemail)
                    info += "NOEMAIL ";
                if (allowusertalk)
                    info += "ALLOWUSERTALK ";
                string[] messageParams = {id, target, blockedBy, expiry, start, blockReason, info};
                string message = Configuration.singleton().getMessage("blockInfoShort", messageParams);

                if (message == emptyMessage)
                {
                    message = Configuration.singleton().getMessage("noBlocks");
                }

                return message;
            }
        }
    }
}