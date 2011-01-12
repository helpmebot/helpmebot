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

            RemoteDataParcel parcel = new RemoteDataParcel()
                                          {
                                              colourIndex = int.Parse(args[0]),
                                              colourParameter = int.Parse(args[1]),
                                              motionIndex = int.Parse(args[2]),
                                              motionParameter = int.Parse(args[3]),
                                              timerInterval = int.Parse(args[4])
                                          };

            snsClient.Publish(new PublishRequest { Message = RemoteDataParcel.Serialize(parcel), Subject = source.ToString(), TopicArn = Configuration.singleton()["awsSnsTopicArn"] });

            return null;
        }
    }
}
