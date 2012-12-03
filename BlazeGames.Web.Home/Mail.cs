using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using BlazeGames.Web.Core;
using MySql.Data.MySqlClient;
using OpenPop.Mime;
using OpenPop.Pop3;

namespace BlazeGames.Web
{
    public class DynamicPage : CodePage
    {
        public override void onPageInitialize()
        {
            // TODO: Page Initialize Code Here
        }

        public override void onPageLoad()
        {
            Pop3Client client = new Pop3Client();
            client.Connect("pop3.live.com", 995, true);
            client.Authenticate("ashton@blzgms.co", "sedona72894");

            // Get the number of messages in the inbox
            int messageCount = client.GetMessageCount();

            // We want to download all messages
            List<Message> allMessages = new List<Message>(messageCount);

            // Messages are numbered in the interval: [1, messageCount]
            // Ergo: message numbers are 1-based.
            // Most servers give the latest message the highest number
            for (int i = messageCount; i > 0; i--)
            {
                if(i == messageCount - 10)
                    break;

                echo(client.GetMessage(i).ToMailMessage().Subject + "<br />");
            }
        }
    }
}