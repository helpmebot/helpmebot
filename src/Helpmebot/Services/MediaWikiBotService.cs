namespace Helpmebot.Services
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Xml.XPath;
    using Helpmebot.Configuration;
    using Helpmebot.Services.Interfaces;
    using NHibernate.Impl;

    public class MediaWikiBotService :IMediaWikiBotService
    {
        private readonly MediaWikiDocumentationConfiguration mediaWikiConfig;
        private readonly IWebServiceClient wsClient;

        public MediaWikiBotService(MediaWikiDocumentationConfiguration mediaWikiConfig, IWebServiceClient wsClient)
        {
            this.mediaWikiConfig = mediaWikiConfig;
            this.wsClient = wsClient;
        }
        
        private string GetToken(string type = "csrf")
        {
            var queryparams = new NameValueCollection
            {
                {"action", "query"},
                {"meta", "tokens"},
                {"type", type}
            };

            var apiResult = this.wsClient.DoApiCall(queryparams, this.mediaWikiConfig.ApiBase);
            var nav = new XPathDocument(apiResult).CreateNavigator();

            var token = nav.SelectSingleNode("//tokens/@" + type + "token");

            if (token == null)
            {
                throw new Exception("Error getting token!");
            }

            return token.Value;
        }

        private bool IsLoggedIn()
        {
            var queryparams = new NameValueCollection
            {
                {"action", "query"},
                {"meta", "userinfo"}
            };
            
            var apiResult = this.wsClient.DoApiCall(queryparams, this.mediaWikiConfig.ApiBase);
            var nav = new XPathDocument(apiResult).CreateNavigator();
            return nav.SelectSingleNode("//userinfo/@id").ValueAsInt > 0;
        }
        
        public void Login()
        {
            if (this.IsLoggedIn())
            {
                return;
            }
            
            var token = this.GetToken("login");
            
            var queryparams = new NameValueCollection
            {
                {"action", "login"},
                {"lgname", this.mediaWikiConfig.Username},
                {"lgpassword", this.mediaWikiConfig.Password},
                {"lgtoken", token}
            };

            var apiResult = this.wsClient.DoApiCall(queryparams, this.mediaWikiConfig.ApiBase, true);
         
            var nav = new XPathDocument(apiResult).CreateNavigator();

            var loginResult = nav.SelectSingleNode("//login/@result");

            if (loginResult == null)
            {
                throw new Exception("Error logging in!");
            }

            if (loginResult.Value != "Success")
            {
                throw new Exception("Error logging in, service returned " + loginResult.Value);
            }
        }

        public string GetPage(string pageName, out string timestamp)
        {
            var queryparams = new NameValueCollection
            {
                {"action", "query"},
                {"prop", "info|revisions"},
                // {"meta", "tokens"},
                {"titles", pageName},
                {"rvprop", "timestamp|content"},
               // {"rvslot", "main"}
            };
            
            var apiResult = this.wsClient.DoApiCall(queryparams, this.mediaWikiConfig.ApiBase);
            var nav = new XPathDocument(apiResult).CreateNavigator();

            var missing = nav.SelectSingleNode("//page/@missing");
            if (missing != null)
            {
                timestamp = null;
                return null;
            }
            
            timestamp = nav.SelectSingleNode("//rev/@timestamp").Value;

            var content = nav.SelectSingleNode("//rev");
            return content.Value;
        }

        public bool WritePage(string pageName, string content, string editSummary, string timestamp, bool bot, bool minor)
        {
            var token = this.GetToken();
            
            var queryparams = new NameValueCollection
            {
                {"action", "edit"},
                {"title", pageName},
                {"text", content},
                {"summary", editSummary}
            };

            if (timestamp != null)
            {
                queryparams.Add("basetimestamp", timestamp);
                queryparams.Add("starttimestamp", timestamp);
            }

            if (bot)
            {
                queryparams.Add("bot", null);
            }

            queryparams.Add(minor ? "minor" : "notminor", null);
            
            // must be last
            queryparams.Add("token", token);
            
            var apiResult = this.wsClient.DoApiCall(queryparams, this.mediaWikiConfig.ApiBase, true);
            var nav = new XPathDocument(apiResult).CreateNavigator();
            
            return nav.SelectSingleNode("//edit/@result").Value == "Success";
        }

        public void DeletePage(string pageName, string reason)
        {
            var token = this.GetToken();
            
            var queryparams = new NameValueCollection
            {
                {"action", "delete"},
                {"title", pageName},
                {"reason", reason},
                {"token", token}
            };

            var apiResult = this.wsClient.DoApiCall(queryparams, this.mediaWikiConfig.ApiBase, true);
        }
        
        public IEnumerable<string> PrefixSearch(string prefix)
        {
            bool continuePresent;
            
            var queryparams = new NameValueCollection
            {
                {"action", "query"},
                {"list", "allpages"},
                {"apprefix", prefix},
                {"aplimit", "max"}
            };

            do
            {
                var apiResult = this.wsClient.DoApiCall(queryparams, this.mediaWikiConfig.ApiBase);
                var nav = new XPathDocument(apiResult).CreateNavigator();

                var contNode = nav.SelectSingleNode("//continue");

                if (contNode != null)
                {
                    continuePresent = true;

                    var attrIt = contNode.Select("@*");
                    while (attrIt.MoveNext())
                    {
                        queryparams.Remove(attrIt.Current.Name);
                        queryparams.Add(attrIt.Current.Name, attrIt.Current.Value);
                    }
                }
                else
                {
                    continuePresent = false;
                }

                foreach (var page in nav.Select("//allpages/p/@title"))
                {
                    yield return page.ToString();
                }
                
            } while (continuePresent);
        }
    }
}