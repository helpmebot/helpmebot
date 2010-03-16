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
            message.Subject = string.Join( " ", args );
            System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient( "localhost" );
            client.Send( message );
            return;
        }
    }
}
