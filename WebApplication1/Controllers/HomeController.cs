using EAGetMail;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using WebApplication1.Models;
using static System.Net.Mime.MediaTypeNames;


namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        //[HttpPost]
        public async Task<ActionResult> Index(string username, string password)
        {
            var listEmail = new List<EmailEntity>();
            var mailClient = new ImapClient();
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return View("Login");
            }
            else
            {
                mailClient.Connect("imap.gmail.com", 993);
                mailClient.Authenticate(username, password);
            }
                
            //mailClient.Authenticate("pha170320@gmail.com", "ryarooxkojsnqybd");
            var folder = await mailClient.GetFolderAsync("Inbox");
            await folder.OpenAsync(FolderAccess.ReadWrite);

            IList<UniqueId> uids = folder.Search(SearchQuery.All);
            foreach (UniqueId uid in uids)
            {
                MimeMessage message = folder.GetMessage(uid);
                var EmailEntity = new EmailEntity();
                EmailEntity.Id = uid.ToString();
                EmailEntity.From = message.From.ToString();
                EmailEntity.To = message.To.ToString();
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
                    var fileName = attachment.ContentDisposition.FileName ?? attachment.ContentType.Name; 
                    string mimeType = System.Web.MimeMapping.GetMimeMapping(fileName); 
                    if ((mimeType == "application/pdf" || mimeType == "text/xml"))
                    {
                        fileAttactment.Add(fileName);
                        Typefilename.Add(mimeType);
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
                }
                EmailEntity.FileAttactment = string.Join(";", fileAttactment);
                EmailEntity.Typefilename = string.Join("; ", Typefilename);
                if(EmailEntity.FileAttactment != "")
                {
                    listEmail.Add(EmailEntity);
                }
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
            foreach (string path in getLocalpath())
            {
                string localFilePath = path + "/" + fileName;
                if (System.IO.File.Exists(localFilePath))
                {
                    return File(System.IO.File.OpenRead(localFilePath), "application/octet-stream", Path.GetFileName(localFilePath));
                }
            }
            return null;
        }
        [HttpGet]
        public async Task<JsonResult> validateUser(string username, string password)
        {
            var listUser = new List<UserEntity>();
            var userFind = new UserEntity();
            UserEntity User1 = new UserEntity("maixson.2712@gmail.com", "qpnchazurvybnkxs"); listUser.Add(User1);
            UserEntity User2 = new UserEntity("legolas15397@gmail.com", "yixhptngrwpgnwzb"); listUser.Add(User2);
            UserEntity User3 = new UserEntity("kiemtra11062712@gmail.com", "ilgtrxlhaeqzkbbx"); listUser.Add(User3);
            UserEntity User4 = new UserEntity("pha170320@gmail.com", "ryarooxkojsnqybd"); listUser.Add(User4);

            foreach (var user in listUser)
            {
                if (username == user.user && password == user.password)
                {
                    Session.Add("keySession", user);
                }
            }
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return Json(new { error = "Please enter username and password." }, JsonRequestBehavior.AllowGet);
            }

            foreach (UserEntity item in listUser)
            {
                if (item.user == username && item.password == password)
                {
                    userFind = item;
                    return Json(new { data = userFind }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { error = "Incorrect username or password" }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<ActionResult> getEachEmail(string Id_mail, string username, string password)
        {
            var getDetailEmail = new EmailEntity();
            var mailClient = new ImapClient();
            mailClient.Connect("imap.gmail.com", 993);
            mailClient.Authenticate(username, password);
            var folder = await mailClient.GetFolderAsync("Inbox");
            await folder.OpenAsync(FolderAccess.ReadWrite);

            MimeMessage message = folder.GetMessage(UniqueId.Parse(Id_mail));
            getDetailEmail.Id = Id_mail.ToString();
            getDetailEmail.From = message.From.ToString();
            getDetailEmail.To = message.To.ToString();
            getDetailEmail.TimeReceive = message.Date;
            getDetailEmail.Subject = message.Subject;
            getDetailEmail.Body = message.TextBody;
            var fileAttactment = new List<string>();
            var Typefilename = new List<string>();

            foreach (MimeEntity attachment in message.Attachments)
            {
                var fileName = attachment.ContentDisposition.FileName ?? attachment.ContentType.Name; fileAttactment.Add(fileName);
                string mimeType = System.Web.MimeMapping.GetMimeMapping(fileName); Typefilename.Add(mimeType);
            }
                getDetailEmail.FileAttactment = string.Join(";", fileAttactment);
                getDetailEmail.Typefilename = string.Join("; ", Typefilename);

            return Json(new {data = getDetailEmail}, JsonRequestBehavior.AllowGet);
        }
        public IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return day;
        }
        private List<string> getLocalpath()
        {
            List<string> localFilePath = new List<string>();
            foreach (DateTime day in EachDay(DateTime.Now.AddDays(-10), DateTime.Now))
            {
                var stringdate = day.ToString("yyyyMMdd");
                string Path = AppDomain.CurrentDomain.BaseDirectory + "/Attachment/" + stringdate;
                localFilePath.Add(Path);
            }
            return localFilePath;
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

        //    [HttpPost]
        public ActionResult Login()
        {
            return View();
        }
        public ActionResult Logout()
        {
            Session["keySession"] = null;
            return RedirectToAction("Index", "Home");
        }
    }
}