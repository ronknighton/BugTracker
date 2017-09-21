﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace BugTracker.Helpers
{
   
    public class NotifcationMessage
    {
        //public string CustomSource { get; set; }
        //public string SourceName { get; set; }
        //public string SourceId { get; set; }

        [DataType(DataType.EmailAddress)]
        public string RecipientEmail { get; set; }

        //public string Subject { get; set; }

        [AllowHtml]
        public string Body { get; set; }
    }

    public class EmailHelper
    {
        

        public static async Task SendNotificationEmailAsync(NotifcationMessage message)
        {
           

            //Steps for composing the CustomSource 
            //message.SourceName =  userHelper.GetUserNameFromId(message.RecipientId);
            //message.CustomSource = message.SourceName + WebConfigurationManager.AppSettings["emailsource"];

            //Get data from our private.Config for usage within the SMTP object
            var gmailUsername = WebConfigurationManager.AppSettings["username"];
            var gmailPassword = WebConfigurationManager.AppSettings["password"];
            var gmailHost = WebConfigurationManager.AppSettings["host"];
            var gmailPort = Convert.ToInt32(WebConfigurationManager.AppSettings["port"]);

            using (var smtp = new SmtpClient()
            {
                Host = gmailHost,
                Port = gmailPort,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(gmailUsername, gmailPassword)
            })

            try
            {
                await smtp.SendMailAsync("EMS<rhksoftdev@gmail.com>", message.RecipientEmail, "A message from EMS", message.Body);
                  
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                await Task.FromResult(0);
            }
        }
    }
}