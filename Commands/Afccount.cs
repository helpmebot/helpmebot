using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace helpmebot6.Commands
{
    class Afccount : GenericCommand
    {
        protected override CommandResponseHandler execute( User source, string channel, string[ ] args )
        {
            //  api + ?format=xml&action=query&prop=categoryinfo&titles=Category:Pending%20AfC%20submissions

            string baseWiki = Configuration.singleton().retrieveLocalStringOption("baseWiki", channel);

            DAL.Select q = new DAL.Select("site_api");
            q.setFrom("site");
            q.addWhere(new DAL.WhereConds("site_id", baseWiki));
            string api = DAL.singleton().executeScalarSelect(q);

            XPathDocument d =
                new XPathDocument(
                    HttpRequest.get( api +
                                     "?format=xml&action=query&prop=categoryinfo&titles=Category:Pending%20AfC%20submissions" ) );
            

            XPathNavigator nav = d.CreateNavigator( );
            XPathNodeIterator xpni = nav.Select( "//categoryinfo@pages" );

            ;

            return null;

        }
    }
}