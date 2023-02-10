using EAGetMail;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;
using static System.Net.Mime.MediaTypeNames;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var listEmail = new List<EmailEntity>();
            var mailClient = new ImapClient();
            mailClient.Connect("imap.gmail.com", 993);
            //mailClient.Authenticate("maixson.2712@gmail.com", "qpnchazurvybnkxs");
            //mailClient.Authenticate("legolas15397@gmail.com", "yixhptngrwpgnwzb");
            mailClient.Authenticate("kiemtra11062712@gmail.com", "sibdnslycluebzun");
            var folder = await mailClient.GetFolderAsync("Inbox");
            await folder.OpenAsync(FolderAccess.ReadWrite);

            IList<UniqueId> uids = folder.Search(SearchQuery.All);
            foreach (UniqueId uid in uids)
            {
                MimeMessage message = folder.GetMessage(uid);
                var EmailEntity = new EmailEntity();
                EmailEntity.Id = uid.ToString();
                EmailEntity.From = message.From.ToString();
                EmailEntity.TimeReceive = message.Date;
                EmailEntity.Subject = message.Subject;
                EmailEntity.Body = message.TextBody;
                var fileAttactment = new List<string>();
                var Typefilename = new List<string>();
                var dateReceive = EmailEntity.TimeReceive;
                var stringdate = dateReceive.ToString("yyyyMMdd");
                string localFilePath = AppDomain.CurrentDomain.BaseDirectory + "/Attachment/";

                foreach (MimeEntity attachment in message.Attachments)
                {
                    var fileName = attachment.ContentDisposition.FileName ?? attachment.ContentType.Name; fileAttactment.Add(fileName);
                    string mimeType = System.Web.MimeMapping.GetMimeMapping(fileName); Typefilename.Add(mimeType);
                    if ((mimeType == "application/pdf" || mimeType == "text/xml"))
                    {
                        if (!Directory.Exists(localFilePath + "/File_Pdf_and_XML"))
                        {
                            Directory.CreateDirectory(localFilePath + "/File_Pdf_and_XML");
                        }
                        using (var str = System.IO.File.Create(localFilePath + "/File_Pdf_and_XML" + "/" + fileName))
                        {
                            if (attachment is MessagePart)
                            {
                                var rfc822 = (MessagePart)attachment;

                                rfc822.Message.WriteTo(str);
                            }
                            else
                            {
                                var part = (MimePart)attachment;

                                part.Content.DecodeTo(str);
                            }
                        }
                    }

                    if (!Directory.Exists(localFilePath + stringdate))
                    {
                        Directory.CreateDirectory(localFilePath + stringdate);
                    }
                    using (var stream = System.IO.File.Create(localFilePath + stringdate + "/" + fileName))
                    {
                        if (attachment is MessagePart)
                        {
                            var rfc822 = (MessagePart)attachment;

                            rfc822.Message.WriteTo(stream);
                        }
                        else
                        {
                            var part = (MimePart)attachment;

                            part.Content.DecodeTo(stream);
                        }
                    }
                }
                EmailEntity.FileAttactment = string.Join(";", fileAttactment);
                EmailEntity.Typefilename = string.Join("; ", Typefilename);
                listEmail.Add(EmailEntity);

            }
            ViewBag.listEmail = listEmail;
            return View();
        }

        static Imap4Folder FindFolder(string folderPath, Imap4Folder[] folders)
        {
            int count = folders.Length;
            for (int i = 0; i < count; i++)
            {
                Imap4Folder folder = folders[i];
                if (string.Compare(folder.LocalPath, folderPath, true) == 0)
                {
                    return folder;
                }
                folder = FindFolder(folderPath, folder.SubFolders);
                if (folder != null)
                {
                    return folder;
                }
            }
            // No folder found
            return null;
        }
        [HttpGet]
        public async Task<ActionResult> GetFile(string fileName)
        {
            foreach (DateTime day in EachDay(DateTime.Now.AddDays(-10), DateTime.Now))
            {
                var stringdate = day.ToString("yyyyMMdd");
                string localFilePath = AppDomain.CurrentDomain.BaseDirectory + "/Attachment/" + stringdate + "/" + fileName;
                if (System.IO.File.Exists(localFilePath))
                {
                    return File(System.IO.File.OpenRead(localFilePath), "application/octet-stream", Path.GetFileName(localFilePath));
                }
            }
            return null;
        }
        public IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return day;
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}