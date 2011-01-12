using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace helpmebot6.Commands
{
    class Rama : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            string uKey = Configuration.singleton()["awsUserKey"];
            string sKey = Configuration.singleton()["awsSecretKey"];

            AmazonSimpleNotificationService snsClient = AWSClientFactory.CreateAmazonSNSClient(uKey, sKey);
            snsClient.Publish(new PublishRequest {Message = string.Join(" ", args), Subject = "test", TopicArn = Configuration.singleton()["awsSnsTopicArn"]});

            return null;
        }
    }
}
