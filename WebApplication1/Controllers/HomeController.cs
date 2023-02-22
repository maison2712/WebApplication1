using EAGetMail;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;
using Org.BouncyCastle.Crypto.Macs;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using WebApp_ReadXML.Models;
using WebApplication1.Models;
using static System.Net.Mime.MediaTypeNames;


namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        //[HttpPost]
        public async Task<ActionResult> Index(string username, string password)
        {
            //username = "kiemtra11062712@gmail.com";
            //password = "ilgtrxlhaeqzkbbx";
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
                    if (mimeType == "application/pdf" || mimeType == "text/xml" || mimeType == "application/x-zip-compressed" || mimeType == "application/octet-stream")
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
                if (EmailEntity.FileAttactment != "")
                {
                    listEmail.Add(EmailEntity);
                }
            }
            ViewBag.listEmail = listEmail.OrderByDescending(x => x.TimeReceive);
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
        public ActionResult GetFile(string fileName)
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
        public JsonResult validateUser(string username, string password)
        {
            var listUser = new List<UserEntity>();
            var userFind = new UserEntity();
            UserEntity User1 = new UserEntity("maixson.2712@gmail.com", "qpnchazurvybnkxs"); listUser.Add(User1);
            UserEntity User2 = new UserEntity("legolas15397@gmail.com", "yixhptngrwpgnwzb"); listUser.Add(User2);
            UserEntity User3 = new UserEntity("kiemtra11062712@gmail.com", "ilgtrxlhaeqzkbbx"); listUser.Add(User3);
            UserEntity User4 = new UserEntity("pha170320@gmail.com", "ryarooxkojsnqybd"); listUser.Add(User4);

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
            //username = "kiemtra11062712@gmail.com";
            //password = "ilgtrxlhaeqzkbbx";
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
            var dateReceive = getDetailEmail.TimeReceive;
            var stringdate = dateReceive.ToString("yyyyMMdd");
            string localFilePath = AppDomain.CurrentDomain.BaseDirectory + "/Attachment/";

            foreach (MimeEntity attachment in message.Attachments)
            {
                var fileName = attachment.ContentDisposition.FileName ?? attachment.ContentType.Name;
                string mimeType = System.Web.MimeMapping.GetMimeMapping(fileName);
                if (mimeType == "application/pdf" || mimeType == "text/xml" || mimeType == "application/x-zip-compressed" || mimeType == "application/octet-stream")
                {
                    fileAttactment.Add(fileName);
                    Typefilename.Add(mimeType);
                }
                if (mimeType == "application/x-zip-compressed")
                {
                    using (ZipArchive archive = ZipFile.Open(localFilePath + stringdate + "/" + fileName, ZipArchiveMode.Update))
                    {
                        foreach (var file in archive.Entries)
                        {
                            var fileNameinZIP = file.FullName;
                            var duoi = fileNameinZIP.Substring(fileNameinZIP.LastIndexOf(".") + 1);
                            if (System.IO.File.Exists(localFilePath + stringdate + "/" + fileNameinZIP))
                            {
                                System.IO.File.Delete(localFilePath + stringdate + "/" + fileNameinZIP);
                            }
                            if (duoi == "xml" || duoi == "pdf")
                            {
                                fileAttactment.Add(fileNameinZIP);
                                //var NameFile = fileNameinZIP.Substring(0, fileNameinZIP.LastIndexOf('.'));
                            }
                        }
                        archive.ExtractToDirectory(localFilePath + stringdate);
                    }
                }
                if (mimeType == "application/octet-stream")
                {
                    using (var archive = ArchiveFactory.Open(localFilePath + stringdate + "/" + fileName))
                    {
                        foreach (var entry in archive.Entries)
                        {
                            var fileNameinRAR = entry.Key;
                            var duoi = fileNameinRAR.Substring(fileNameinRAR.LastIndexOf(".") + 1);
                            if (!entry.IsDirectory)
                            {
                                entry.WriteToDirectory(localFilePath + stringdate, new ExtractionOptions()
                                {
                                    ExtractFullPath = true,
                                    Overwrite = true
                                });
                            }
                            if (duoi == "xml" || duoi == "pdf")
                            {
                                fileAttactment.Add(fileNameinRAR);
                                //var NameFile = fileNameinRAR.Substring(0, fileNameinRAR.LastIndexOf('.'));
                            }
                        }
                    }
                }
            }
            getDetailEmail.FileAttactment = string.Join(";", fileAttactment);
            getDetailEmail.Typefilename = string.Join("; ", Typefilename);

            return Json(new { data = getDetailEmail }, JsonRequestBehavior.AllowGet);
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
        //-------------------------------đọc file XML---------------------------------------------------
        public JsonResult About(string stringdate, string fileName)
        {
            string filePath = Server.MapPath("~/Attachment/" + stringdate + "/" + fileName); // đường dẫn tệp XML
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(filePath);
            string pathNodeXML = "/TDiep/DLieu/HDon/DLHDon/NDHDon";
            var invoice = new InvoiceEntity();


            SignedXml signedXml = new SignedXml(xmlDocument);
            XmlNodeList nodeList = xmlDocument.GetElementsByTagName("Signature");
            signedXml.LoadXml((XmlElement)nodeList.Item(0));

            var certificate = xmlDocument.GetElementsByTagName("X509Certificate").Item(0).InnerText;
            byte[] tmp;
            tmp = System.Convert.FromBase64String(certificate);
            X509Certificate2 cer = new X509Certificate2(tmp);

            bool verifiedXml = signedXml.CheckSignature(cer.PublicKey.Key);

            if (!verifiedXml)
            {
                return Json(new { status = false, message = "Dữ liệu hóa đơn đã bị thay đổi" });
            }
            else
            {
                //đọc thông tin người bán
                var seller = new Seller();
                seller.Ten = ReadLineXML(xmlDocument, "/NBan/Ten");  
                seller.MST = ReadLineXML(xmlDocument, "/NBan/MST"); 
                seller.DChi = ReadLineXML(xmlDocument, "/NBan/DChi"); 
                seller.DCTDTu = ReadLineXML(xmlDocument, "/NBan/DCTDTu");
                seller.STKNHang = ReadLineXML(xmlDocument, "/NBan/STKNHang");
                seller.TNHang = ReadLineXML(xmlDocument, "/NBan/TNHang");
                invoice.seller = seller;

                //đọc thông tin người mua
                var buyer = new Buyer();
                buyer.Ten = xmlDocument.SelectSingleNode(pathNodeXML + "/NMua/Ten").InnerText;
                buyer.MST = xmlDocument.SelectSingleNode(pathNodeXML + "/NMua/MST").InnerText;
                buyer.DChi = xmlDocument.SelectSingleNode(pathNodeXML + "/NMua/DChi").InnerText;
                invoice.buyer = buyer;

                //đọc thông tin hàng hóa dịch vụ
                XmlNodeList listServiceProductNode = xmlDocument.SelectNodes(pathNodeXML + "/DSHHDVu/HHDVu");
                var listServiceProduct = new List<ServiceProduct>();

                foreach (XmlNode node in listServiceProductNode)
                {
                    var serviceProduct = new ServiceProduct();

                    serviceProduct.TChat = int.Parse(node["TChat"].InnerText);
                    serviceProduct.STT = int.Parse(node["STT"].InnerText);
                    serviceProduct.THHDVu = node["THHDVu"].InnerText;
                    serviceProduct.DVTinh = node["DVTinh"].InnerText;
                    serviceProduct.SLuong = int.Parse(node["SLuong"].InnerText);
                    serviceProduct.DGia = decimal.Parse(node["DGia"].InnerText);
                    serviceProduct.ThTien = decimal.Parse(node["ThTien"].InnerText);
                    serviceProduct.TSuat = node["TSuat"].InnerText;

                    listServiceProduct.Add(serviceProduct);
                }
                invoice.serviceProducts = listServiceProduct;

                //đọc thông tin thanh toán
                var pay = new Pay();
                pay.TSuat = xmlDocument.SelectSingleNode(pathNodeXML + "/TToan/THTTLTSuat/LTSuat/TSuat").InnerText;
                pay.ThTien = decimal.Parse(xmlDocument.SelectSingleNode(pathNodeXML + "/TToan/THTTLTSuat/LTSuat/ThTien").InnerText);
                pay.TThue = decimal.Parse(xmlDocument.SelectSingleNode(pathNodeXML + "/TToan/THTTLTSuat/LTSuat/TThue").InnerText);
                pay.TgTCThue = decimal.Parse(xmlDocument.SelectSingleNode(pathNodeXML + "/TToan/TgTCThue").InnerText);
                pay.TgTThue = decimal.Parse(xmlDocument.SelectSingleNode(pathNodeXML + "/TToan/TgTThue").InnerText);
                pay.TgTTTBSo = decimal.Parse(xmlDocument.SelectSingleNode(pathNodeXML + "/TToan/TgTTTBSo").InnerText);
                pay.TgTTTBChu = xmlDocument.SelectSingleNode(pathNodeXML + "/TToan/TgTTTBChu").InnerText;
                invoice.pay = pay;

                //đọc thông tin chữ ký điện tử
                var Certificate2 = new x509Certificate2();
                Certificate2.Issueser = cer.Issuer;
                Certificate2.Subject = cer.Subject;
                invoice.Certificate2 = Certificate2;
            }
            return Json(new { data = invoice }, JsonRequestBehavior.AllowGet);
        }
        private string ReadLineXML(XmlDocument xmlDocument ,string elementXml)
        {
            string pathNodeXML = "/TDiep/DLieu/HDon/DLHDon/NDHDon";
            var result = xmlDocument.SelectSingleNode(pathNodeXML + elementXml);
            if (result == null)
            {
                return "";
            }
            else
            {
                return result.InnerText;
            }

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