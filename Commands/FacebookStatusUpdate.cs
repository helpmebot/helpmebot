using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    class Facebookstatusupdate : GenericCommand
    {
        protected override CommandResponseHandler execute( User source, string channel, string[ ] args )
        {
            string fbmail = Configuration.Singleton( ).retrieveGlobalStringOption( "fbEmail" );
            System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage( "helpmebot@helpmebot.org.uk", fbmail );
            string data = string.Join( " ", args );
            message.Subject = data;
            message.Body = data;
            message.BodyEncoding = Encoding.UTF8;
            message.Sender = new System.Net.Mail.MailAddress("stwalkerster@willow.toolserver.org");
            System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient( );
            client.Send( message );
            return null;
        }
    }
}
