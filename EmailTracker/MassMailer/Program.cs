using System;
using System.Net.Mail;
using System.Data.SqlClient;

namespace MassMailer
{
    class Program
    {
        static void Main(string[] args)
        {

            string subject = "Testing Pixel";
            string fromEmail = "{from email}"; //replace with who is sending email
            string toEmail = "{to email}"; //test, this would usually be a list of users
            string campaignId = CreateCampaign(subject); //creates the campaign and returns guid
            string baseUrl = "https://{azure function}/api/EmailTrackingFunction"; //our azure function url
            string imgUrl = $"{baseUrl}?Email={toEmail}&Campaign={campaignId}";// our url for our image

            //connect to gmail
            using(SmtpClient client = new SmtpClient("smtp.gmail.com"))
            {
                client.Credentials = new System.Net.NetworkCredential(fromEmail, "from email password");
                client.Port =587;
                client.EnableSsl = true;

                //create our email
                using(MailMessage msg = new MailMessage(fromEmail, toEmail))
                {
                    msg.Subject = subject;
                    msg.Body = $"Mass mailout<br/><img src='{imgUrl}' alt='' width=1 heigh=1 style='width:1px;height:1px' />";
                    msg.IsBodyHtml = true;

                    client.Send(msg);

            
                    //would log time/date email was sent in a database here
                }
            }

            static string CreateCampaign(string Subject)
            {
                //Replace with your sql connection string
                string cs = "Server=tcp:sqlconnection,1433;Initial Catalog=trackingdata;Persist Security Info=False;User ID={sql user};Password={sql password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
                string campaignId = Guid.NewGuid().ToString();
                using(SqlConnection connection = new SqlConnection(cs))
                {
                    //insert our data into our campaing table to track what campaigns are sent.
                    connection.Open();
                    using(SqlCommand cmd = new SqlCommand("INSERT INTO Campaigns (CampaignId, Subject) Values(@CampaingId, @Subject)", connection))
                    {
                        cmd.Parameters.AddWithValue("@CampaingId", campaignId);
                        cmd.Parameters.AddWithValue("@Subject", Subject);//email that opened link

                        cmd.ExecuteNonQuery();
                    }

                    connection.Close();
                }

                //return our Unique campaign id
                return campaignId;
            }

        }
    }
}
