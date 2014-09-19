using System.Net.Mail;
using System.Configuration;
using System.IO;
using System;
using System.Windows.Forms;
using System.Threading;

namespace Logger
{
    public class SendEmail
    {
        
        private static void clearOldLogs(string process, string filename){

            string[] fileEntries = Directory.GetFiles(Path.GetDirectoryName(filename));
            foreach (string fileName in fileEntries)
            {
                if (fileName.Contains(process) && Path.GetExtension(fileName)==".txt")
                {
                   // MessageBox.Show(fileName);
                    File.Delete(fileName);
                }
            }
        }
        public static void SendMessage(string Subject, string Message, string process, TimeSpan time)
        {
            
            string filename= createFile(process, time);
            SendMessage(Subject, Message + "\nLink:\t" +filename, Properties.Settings.Default.SendFromEmail, Properties.Settings.Default.SendToEmail, Properties.Settings.Default.CCEmail, filename);
            Thread.Sleep(2000);            
            
        }
        private static string createFile(string process, TimeSpan time)
        {
            clsDatabase d= new clsDatabase();
            string filename;
            if (Properties.Settings.Default.FilePath.Length > 0)
                filename = Properties.Settings.Default.FilePath;
            else
                filename = Path.GetFullPath(Application.ExecutablePath);
                 
            if (!filename.EndsWith("\\") || !filename.EndsWith("/"))
            {
                filename = filename + "\\";                    
            }
            filename= filename + process + DateTime.Now.ToString("MM-dd-yy_hhmmss") + ".txt";


            clearOldLogs(process, filename);
            StreamWriter sw = new StreamWriter(filename);
            foreach (ProcessLogs pl in d.printLogs(process, time)){
                sw.WriteLine(string.Format("{0} ({1}) {2}.{3}\t {4}", pl.LogDate, pl.LogCode.Trim(), pl.ProcessName, pl.ProcessPhase, pl.LogMessage)); 
            }               
            sw.Close();
            return Path.GetFullPath(filename);
        }
        public static void SendMessage(string subject, string messageBody, string fromAddress, string toAddress, string ccAddress, string attachmentLoc)
        {
            MailMessage message = new MailMessage();
            SmtpClient client = new SmtpClient();

            // Set the sender's address
            message.From = new MailAddress(fromAddress);

            // Allow multiple "To" addresses to be separated by a semi-colon
            if (toAddress.Trim().Length > 0)
            {
                foreach (string addr in toAddress.Split(';'))
                {
                    message.To.Add(new MailAddress(addr));
                }
            }

            // Allow multiple "Cc" addresses to be separated by a semi-colon
            if (ccAddress.Trim().Length > 0)
            {
                foreach (string addr in ccAddress.Split(';'))
                {
                    message.CC.Add(new MailAddress(addr));
                }
            }
            // Allow multiple attachments to be added to the email.
            if (attachmentLoc.Trim().Length > 0)
            {
                foreach (string attachFile in attachmentLoc.Split(';'))
                {                    
                    message.Attachments.Add(new Attachment(attachFile));
                }
            }

            // Set the subject and message body text
            message.Subject = subject;
            message.Body = messageBody;

            // TODO: *** Modify for your SMTP server ***
            // Set the SMTP server to be used to send the message
            client.Host = "mail.usu.edu";

            // Send the e-mail message
            client.Send(message);
        }
    }
}
