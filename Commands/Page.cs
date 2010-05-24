using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.IO;
using System.Xml;

namespace helpmebot6.Commands
{
    class Page:GenericCommand
    {
        protected override CommandResponseHandler execute( User source, string channel, string[ ] args )
        {
            Stream rawDataStream = HttpRequest.get( "http://en.wikipedia.org/w/api.php?action=query&prop=revisions|info&rvprop=user|comment&redirects&inprop=protection&format=xml&titles=" + string.Join( " ", args ) );

            XmlTextReader xtr = new XmlTextReader( rawDataStream );

            CommandResponseHandler crh = new CommandResponseHandler();

            string redirects = null;
            ArrayList protection = new ArrayList( );
            string user, title, size, comment;
            DateTime touched = DateTime.MinValue;
            user = title = comment = size = null;


            while( !xtr.EOF )
            {
                xtr.Read( );
                if( xtr.IsStartElement( ) )
                    switch( xtr.Name )
                    {
                        case "r":
                            // redirect!
                            // <r from="Sausages" to="Sausage" />
                            redirects = xtr.GetAttribute( "from" );
                            break;
                        case "page":
                            if( xtr.GetAttribute( "missing" ) != null )
                            {
                                return new CommandResponseHandler( Configuration.Singleton( ).GetMessage( "pageMissing" ) );
                            }
                            // title, touched
                            // <page pageid="78056" ns="0" title="Sausage" touched="2010-05-23T17:46:16Z" lastrevid="363765722" counter="252" length="43232">
                            title = xtr.GetAttribute( "title" );
                            touched = DateTime.Parse( xtr.GetAttribute( "touched" ) );

                            break;
                        case "rev":
                            // user, comment
                            // <rev user="RjwilmsiBot" comment="..." />
                            user = xtr.GetAttribute( "user" );
                            comment = xtr.GetAttribute( "comment" );
                            break;
                        case "pr":
                            // protections  
                            // <pr type="edit" level="autoconfirmed" expiry="2010-06-30T18:36:52Z" />
                            string time = xtr.GetAttribute( "expiry" );
                            protection.Add( new PageProtection( xtr.GetAttribute( "type" ), xtr.GetAttribute( "level" ), time == "infinity" ? DateTime.MaxValue : DateTime.Parse( time ) ) );
                            break;
                        default:
                            break;
                    }
            }



            if( redirects != null )
            {
                string[ ] redirArgs = { redirects,title };
                crh.respond( Configuration.Singleton( ).GetMessage( "pageRedirect", redirArgs ) );
            }

            string[ ] margs = { title, user, touched.ToString( ), comment, size };
            crh.respond( Configuration.Singleton( ).GetMessage( "pageMainResponse", margs ) );

            foreach( PageProtection p in protection )
            {
                string[ ] pargs = { title, p.type,p.level,p.expiry == DateTime.MaxValue ? "infinity" : p.expiry.ToString() };
                crh.respond( Configuration.Singleton( ).GetMessage( "pageProtected", pargs ) );
            }

            return crh;
        }

        private struct PageProtection
        {
            public PageProtection(string type, string level, DateTime expiry )
            {
                this.type = type;
                this.level = level;
                this.expiry = expiry;
            }
            public string type, level;
            public DateTime expiry;
        }
    }
}
