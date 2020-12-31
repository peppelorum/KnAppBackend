
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace KnApp.Services
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string txt, string html);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendAsync(string _to, string subject, string txt, string html)
        {
            var apiKey = _configuration.GetSection("SENDGRID_API_KEY").Value;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("knapp@appokalyps.se", "KnApp");
            var to = new EmailAddress(_to, _to);


            Console.WriteLine(apiKey);

            // var subject = "Hello world email from Sendgrid ";
            // var htmlContent = "<strong>Hello world with HTML content</strong>";
            // var displayRecipients = false; // set this to true if you want recipients to see each others mail id 
            var msg = MailHelper.CreateSingleEmail(from, to, subject, txt, html);
            var response = await client.SendEmailAsync(msg);
        }
    }
}