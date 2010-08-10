<<<<<<< .working
// /****************************************************************************
//  *   This file is part of Helpmebot.                                        *
//  *                                                                          *
//  *   Helpmebot is free software: you can redistribute it and/or modify      *
//  *   it under the terms of the GNU General Public License as published by   *
//  *   the Free Software Foundation, either version 3 of the License, or      *
//  *   (at your option) any later version.                                    *
//  *                                                                          *
//  *   Helpmebot is distributed in the hope that it will be useful,           *
//  *   but WITHOUT ANY WARRANTY; without even the implied warranty of         *
//  *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the          *
//  *   GNU General Public License for more details.                           *
//  *                                                                          *
//  *   You should have received a copy of the GNU General Public License      *
//  *   along with Helpmebot.  If not, see <http://www.gnu.org/licenses/>.     *
//  ****************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace helpmebot6.Commands
{
    /// <summary>
    /// Returns the number of articles currently waiting at Articles for Creation
    /// </summary>
    internal class Afccount : GenericCommand
    {
        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <param name="source">The user who triggered the command.</param>
        /// <param name="channel">The channel the command was triggered in.</param>
        /// <param name="args">The arguments to the command.</param>
        /// <returns></returns>
        protected override CommandResponseHandler execute( User source, string channel, string[ ] args )
        {
            //  api + ?format=xml&action=query&prop=categoryinfo&titles=Category:Pending%20AfC%20submissions

            string baseWiki = Configuration.singleton().retrieveLocalStringOption("baseWiki", channel);

           // DAL.Select q = new DAL.Select("site_api");
           // q.setFrom("site");
           // q.addWhere(new DAL.WhereConds("site_id", baseWiki));
           // string api = DAL.singleton().executeScalarSelect(q);

           // XPathDocument d =
           //     new XPathDocument(
           //         HttpRequest.get( api +
           //                          "?format=xml&action=query&prop=categoryinfo&titles=Category:Pending%20AfC%20submissions" ) );
            

           // XPathNavigator nav = d.CreateNavigator( );
           // XPathNodeIterator xpni = nav.Select( "//categoryinfo@pages" );

            ;

            return null;

        }
    }
=======
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

           // DAL.Select q = new DAL.Select("site_api");
           // q.setFrom("site");
           // q.addWhere(new DAL.WhereConds("site_id", baseWiki));
           // string api = DAL.singleton().executeScalarSelect(q);

           // XPathDocument d =
           //     new XPathDocument(
           //         HttpRequest.get( api +
           //                          "?format=xml&action=query&prop=categoryinfo&titles=Category:Pending%20AfC%20submissions" ) );
            

           // XPathNavigator nav = d.CreateNavigator( );
           // XPathNodeIterator xpni = nav.Select( "//categoryinfo@pages" );

            ;

            return null;

        }
    }
>>>>>>> .merge-right.r448
}