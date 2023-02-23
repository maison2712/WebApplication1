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
            var invoice = new InvoiceEntity();

            SignedXml signedXml = new SignedXml(xmlDocument);
            XmlNodeList nodeList = xmlDocument.GetElementsByTagName("Signature");
            signedXml.LoadXml((XmlElement)nodeList.Item(0));

            var certificate = xmlDocument.GetElementsByTagName("X509Certificate").Item(0).InnerText;
            byte[] tmp;
            tmp = Convert.FromBase64String(certificate);
            X509Certificate2 cer = new X509Certificate2(tmp);

            bool verifiedXml = signedXml.CheckSignature(cer.PublicKey.Key);
            //truy vấn node mã của cơ quan thuế
            var nodeMCQT = xmlDocument.SelectSingleNode("/TDiep/DLieu/HDon/MCCQT");
            //truy vấn ngày lập hóa đơn và ngày ký
            var nodeNgayLap = xmlDocument.SelectSingleNode("/TDiep/DLieu/HDon/MCCQT");
            var nodeNgayKy = xmlDocument.SelectSingleNode("/TDiep/DLieu/HDon/DSCKS/CQT/Signature/Object/SignatureProperties/SignatureProperty/SigningTime");

            if (!verifiedXml)
            {
                return Json(new { status = false, message = "Dữ liệu hóa đơn đã bị thay đổi" });

            }
            else if(nodeMCQT == null ){
                return Json(new { status = false, message = "Thiếu mã cơ quan thuế" });

            }
            else if (nodeNgayKy == null || nodeNgayLap == null)
            {
                return Json(new { status = false, message = "Ngày ký và ngày lập không hợp lệ" });

            }
            else if(DateTime.Parse(nodeNgayLap.InnerText).Date != DateTime.Parse(nodeNgayKy.InnerText).Date)
            {
                return Json(new { status = false, message = "Ngày ký và ngày lập không hợp lệ" });

            }
            else
            {
                //đọc thông tin chung
                var generalIn4 = new GeneralInformation();
                generalIn4.PBan = ReadLineXML(xmlDocument, "/TTChung/PBan");
                generalIn4.TenHD = ReadLineXML(xmlDocument, "/TTChung/TenHD");
                generalIn4.KHMSHDon = ReadLineXML(xmlDocument, "/TTChung/KHMSHDon");
                generalIn4.KHHDon = ReadLineXML(xmlDocument, "/TTChung/KHHDon");
                generalIn4.SHDon = ReadLineXML(xmlDocument, "/TTChung/SHDon");
                generalIn4.MHSo = ReadLineXML(xmlDocument, "/TTChung/MHSo");
                generalIn4.NLap = ReadLineXML(xmlDocument, "/TTChung/NLap");
                generalIn4.SBKe = ReadLineXML(xmlDocument, "/TTChung/SBKe");
                generalIn4.DVTTe = ReadLineXML(xmlDocument, "/TTChung/DVTTe");
                generalIn4.TGia = ReadLineXML(xmlDocument, "/TTChung/TGia");
                generalIn4.HTTToan = ReadLineXML(xmlDocument, "/TTChung/HTTToan");
                generalIn4.MSTTCGP = ReadLineXML(xmlDocument, "/TTChung/MSTTCGP");
                generalIn4.MSTDVNUNLHDon = ReadLineXML(xmlDocument, "/TTChung/MSTDVNUNLHDon");
                generalIn4.TDVNUNLHDon = ReadLineXML(xmlDocument, "/TTChung/TDVNUNLHDon");
                generalIn4.DCDVNUNLDon = ReadLineXML(xmlDocument, "/TTChung/DCDVNUNLDon");
                invoice.generalInformation = generalIn4;

                //đọc thông tin người bán
                var seller = new Seller();
                seller.Ten = ReadLineXML(xmlDocument, "/NDHDon/NBan/Ten");
                seller.MST = ReadLineXML(xmlDocument, "/NDHDon/NBan/MST");
                seller.DChi = ReadLineXML(xmlDocument, "/NDHDon/NBan/DChi");
                seller.SDThoai = ReadLineXML(xmlDocument, "/NDHDon/NBan/SDThoai");
                seller.DCTDTu = ReadLineXML(xmlDocument, "/NDHDon/NBan/DCTDTu");
                seller.STKNHang = ReadLineXML(xmlDocument, "/NDHDon/NBan/STKNHang");
                seller.TNHang = ReadLineXML(xmlDocument, "/NDHDon/NBan/TNHang");
                seller.Fax = ReadLineXML(xmlDocument, "/NDHDon/NBan/Fax");
                seller.Website = ReadLineXML(xmlDocument, "/NDHDon/NBan/Website");
                invoice.seller = seller;

                //đọc thông tin người mua
                var buyer = new Buyer();
                buyer.Ten = ReadLineXML(xmlDocument, "/NDHDon/NMua/Ten");
                buyer.MST = ReadLineXML(xmlDocument, "/NDHDon/NMua/MST");
                buyer.DChi = ReadLineXML(xmlDocument, "/NDHDon/NMua/DChi");
                buyer.MKHang = ReadLineXML(xmlDocument, "/NDHDon/NMua/MKHang");
                buyer.SDThoai = ReadLineXML(xmlDocument, "/NDHDon/NMua/SDThoai");
                buyer.DCTDTu = ReadLineXML(xmlDocument, "/NDHDon/NMua/DCTDTu");
                buyer.HVTNMHang = ReadLineXML(xmlDocument, "/NDHDon/NMua/HVTNMHang");
                buyer.STKNHang = ReadLineXML(xmlDocument, "/NDHDon/NMua/STKNHang");
                buyer.TNHang = ReadLineXML(xmlDocument, "/NDHDon/NMua/TNHang");
                invoice.buyer = buyer;

                //đọc thông tin hàng hóa dịch vụ
                XmlNodeList listServiceProductNode = xmlDocument.SelectNodes("/TDiep/DLieu/HDon/DLHDon/NDHDon/DSHHDVu/HHDVu");
                var listServiceProduct = new List<ServiceProduct>();

                foreach (XmlNode node in listServiceProductNode)
                {
                    var serviceProduct = new ServiceProduct();

                    serviceProduct.TChat = int.Parse(ReadNodeList(xmlDocument, "TChat"));
                    serviceProduct.STT = int.Parse(ReadNodeList(xmlDocument, "STT"));
                    serviceProduct.MHHDVu = ReadNodeList(xmlDocument, "MHHDVu");
                    serviceProduct.THHDVu = ReadNodeList(xmlDocument, "THHDVu");
                    serviceProduct.DVTinh = ReadNodeList(xmlDocument, "DVTinh");
                    serviceProduct.SLuong = int.Parse(ReadNodeList(xmlDocument, "SLuong"));
                    serviceProduct.DGia = decimal.Parse(ReadNodeList(xmlDocument, "DGia"));
                    serviceProduct.TLCKhau = decimal.Parse(ReadNodeList(xmlDocument, "TLCKhau"));
                    serviceProduct.ThTien = decimal.Parse(ReadNodeList(xmlDocument, "ThTien"));
                    serviceProduct.TSuat = ReadNodeList(xmlDocument, "TSuat");

                    listServiceProduct.Add(serviceProduct);
                }
                invoice.serviceProducts = listServiceProduct;

                //đọc thông tin thanh toán
                var pay = new Pay();
                pay.TSuat = ReadLineXML(xmlDocument, "/NDHDon/TToan/THTTLTSuat/LTSuat/TSuat");
                pay.ThTien = decimal.Parse(ReadLineXML(xmlDocument, "/NDHDon/TToan/THTTLTSuat/LTSuat/ThTien"));
                pay.TThue = decimal.Parse(ReadLineXML(xmlDocument, "/NDHDon/TToan/THTTLTSuat/LTSuat/TThue"));
                pay.TgTCThue = decimal.Parse(ReadLineXML(xmlDocument, "/NDHDon/TToan/TgTCThue"));
                pay.TgTThue = decimal.Parse(ReadLineXML(xmlDocument, "/NDHDon/TToan/TgTThue"));
                pay.TLPhi = ReadLineXML(xmlDocument, "/NDHDon/TToan/TLPhi");
                pay.TPhi = decimal.Parse(ReadLineXML(xmlDocument, "/NDHDon/TToan/TPhi"));
                pay.TTCKTMai = decimal.Parse(ReadLineXML(xmlDocument, "/NDHDon/TToan/TTCKTMai"));
                pay.TgTTTBSo = decimal.Parse(ReadLineXML(xmlDocument, "/NDHDon/TToan/TgTTTBSo"));
                pay.TgTTTBChu = ReadLineXML(xmlDocument, "/NDHDon/TToan/TgTTTBChu");
                invoice.pay = pay;

                //đọc thông tin chữ ký điện tử
                var Certificate2 = new x509Certificate2();
                Certificate2.Issueser = cer.Issuer;
                Certificate2.Subject = cer.Subject;
                invoice.Certificate2 = Certificate2;
            }
            return Json(new { data = invoice }, JsonRequestBehavior.AllowGet);
        }
        private string ReadLineXML(XmlDocument xmlDocument, string elementXml)
        {
            string pathNodeXML = "/TDiep/DLieu/HDon/DLHDon";
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

        private string ReadNodeList(XmlDocument xmlDocument, string elementXml)
        {
            string pathNode = "/TDiep/DLieu/HDon/DLHDon/NDHDon/DSHHDVu/HHDVu";
            XmlNodeList listNode = xmlDocument.SelectNodes(pathNode);
            foreach (XmlNode node in listNode)
            {
                var result = node[elementXml];
                if (result == null)
                {
                    return "";
                }
                else
                {
                    return result.InnerText;
                }
            }
            return null;
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