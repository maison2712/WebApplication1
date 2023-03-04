using EAGetMail;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using Microsoft.Ajax.Utilities;
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
using System.Web.Configuration;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using WebApp_ReadXML.Models;
using WebApplication1.Models;
using static System.Net.Mime.MediaTypeNames;
using HtmlAgilityPack;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Windows.Forms;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Security.Policy;


namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private string _emailUsername; //khai báo tên email
        private string _emailPassword; //Khai báo mật khẩu email

        //Hàm hiện thị ở trang Index
        public async Task<ActionResult> Index()
        {
            var listEmail = new List<EmailEntity>();
            var mailClient = new ImapClient();

            //đọc user từ file web.config;
            _emailUsername = WebConfigurationManager.AppSettings["email_username"];
            _emailPassword = WebConfigurationManager.AppSettings["email_password"];

            mailClient.Connect("imap.gmail.com", 993);
            mailClient.Authenticate(_emailUsername, _emailPassword);
            //Truy xuất vào Inbox của email và đọc thư nhận
            var folder = await mailClient.GetFolderAsync("Inbox");
            await folder.OpenAsync(FolderAccess.ReadWrite);
            //lấy ra danh dách uId của tất cả các mail nhận
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
                var mailMessageEML = new mailMessageEML();
                var dateReceive = EmailEntity.TimeReceive;
                var stringdate = dateReceive.ToString("yyyyMMdd");
                string localFilePath = AppDomain.CurrentDomain.BaseDirectory + "/Attachment/" + stringdate;
                string pattern = @"https://tracuuhoadon\.fpt\.com\.vn/<([^>]+)>";
                Regex regex = new Regex(pattern);

                MatchCollection matches = regex.Matches(EmailEntity.Body);

                foreach (Match match in matches)
                {
                    string link = match.Groups[1].Value;
                    Uri uri = new Uri(link);
                    string mst = HttpUtility.ParseQueryString(uri.Query).Get("mst");
                    string sec = HttpUtility.ParseQueryString(uri.Query).Get("sec");
                    string fileNameXML = mst + sec + ".xml";
                    fileAttactment.Add(fileNameXML);
                    ChromeOptions options = new ChromeOptions();
                    options.AddArgument("--headless"); // chạy Chrome ở chế độ ẩn
                    options.AddArgument("--disable-gpu"); // tắt GPU để tăng tốc độ xử lý
                    options.AddArgument("--no-sandbox"); // tắt sandbox để tăng tốc độ xử lý
                    options.AddArgument("--disable-dev-shm-usage"); // tắt sử dụng bộ nhớ chia sẻ để tăng tốc độ xử lý
                    options.AddArgument("--disable-extensions"); // tắt các extension không cần thiết để tăng tốc độ xử lý
                    IWebDriver driver = new ChromeDriver(options);
                    driver.Navigate().GoToUrl(link);
                    System.Threading.Thread.Sleep(3000);
                    // Thực thi đoạn mã JavaScript để tải file XML về
                    IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                    string stringXML = (string)js.ExecuteScript("return xml");
                    if (stringXML != null)
                    {
                        XmlDocument xmlDocument = new XmlDocument();
                        xmlDocument.LoadXml(stringXML);
                        xmlDocument.Save(localFilePath + "/" + fileNameXML);
                    }
                }
                foreach (MimeEntity attachment in message.Attachments)
                {
                    if (attachment.ContentType.ToString() == "Content-Type: message/rfc822")
                    {
                        var rfc822 = (MessagePart)attachment;
                        var emlMessage = rfc822.Message;
                        fileAttactment.Add(mailMessageEML.Subject);
                    }
                    else
                    {
                        var fileName = attachment.ContentDisposition.FileName ?? attachment.ContentType.Name;//lấy ra tên của từng file
                        string mimeType = System.Web.MimeMapping.GetMimeMapping(fileName); //lấy kiểu file của từng file
                        if (mimeType == "application/pdf" || mimeType == "text/xml" || mimeType == "application/x-zip-compressed" || mimeType == "application/octet-stream")
                        {
                            fileAttactment.Add(fileName);
                            Typefilename.Add(mimeType);
                            if (!Directory.Exists(localFilePath))
                            {
                                Directory.CreateDirectory(localFilePath);
                            }
                            using (var stream = System.IO.File.Create(localFilePath + "/" + fileName))
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
                        //Tách file PDF vào folder riêng
                        if (mimeType == "application/pdf")
                        {
                            if (!Directory.Exists(localFilePath + "/PDF/"))
                            {
                                Directory.CreateDirectory(localFilePath + "/PDF/");
                            }
                            using (var stream = System.IO.File.Create(localFilePath + "/PDF/" + fileName))
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
                        //Tách file xml vào foler riêng
                        if (mimeType == "text/xml")
                        {
                            if (!Directory.Exists(localFilePath + "/XML/"))
                            {
                                Directory.CreateDirectory(localFilePath + "/XML/");
                            }
                            using (var stream = System.IO.File.Create(localFilePath + "/XML/" + fileName))
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

        //Hàm tải file với
        //Input là tên file
        //Output là 1 File có tên truyền vào
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

        //Hàm kiểm tra tài khoản người dùng
        //[HttpGet]
        //public JsonResult validateUser(string username, string password)
        //{
        //    var listUser = new List<UserEntity>();
        //    var userFind = new UserEntity();
        //    UserEntity User1 = new UserEntity("maixson.2712@gmail.com", "qpnchazurvybnkxs"); listUser.Add(User1);
        //    UserEntity User2 = new UserEntity("legolas15397@gmail.com", "yixhptngrwpgnwzb"); listUser.Add(User2);
        //    UserEntity User3 = new UserEntity("kiemtra11062712@gmail.com", "ilgtrxlhaeqzkbbx"); listUser.Add(User3);
        //    UserEntity User4 = new UserEntity("pha170320@gmail.com", "ryarooxkojsnqybd"); listUser.Add(User4);

        //    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        //    {
        //        return Json(new { error = "Please enter username and password." }, JsonRequestBehavior.AllowGet);
        //    }

        //    foreach (UserEntity item in listUser)
        //    {
        //        if (item.user == username && item.password == password)
        //        {
        //            userFind = item;
        //            return Json(new { data = userFind }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    return Json(new { error = "Incorrect username or password" }, JsonRequestBehavior.AllowGet);
        //}

        //Hàm lấy nội dung chi tiết mail theo Id
        //Input là id của email
        //Output là đối tượng Email có id truyền vào
        [HttpGet]
        public async Task<ActionResult> getEachEmail(string Id_mail/*, string username, string password*/)
        {
            var getDetailEmail = new EmailEntity();
            var mailClient = new ImapClient();

            //đọc user từ file web.config;
            _emailUsername = WebConfigurationManager.AppSettings["email_username"];
            _emailPassword = WebConfigurationManager.AppSettings["email_password"];

            mailClient.Connect("imap.gmail.com", 993);
            mailClient.Authenticate(_emailUsername, _emailPassword);
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
            var messageEML = new List<mailMessageEML>();
            var mailMessageEML = new mailMessageEML();
            var dateReceive = getDetailEmail.TimeReceive;
            var stringdate = dateReceive.ToString("yyyyMMdd");
            string localFilePath = AppDomain.CurrentDomain.BaseDirectory + "/Attachment/" + stringdate;

            string patternFPT = @"https://tracuuhoadon\.fpt\.com\.vn/<([^>]+)>";
            Regex regexFPT = new Regex(patternFPT);

            MatchCollection matchesFPT = regexFPT.Matches(getDetailEmail.Body);

            foreach (Match match in matchesFPT)
            {
                string link = match.Groups[1].Value;
                Uri uri = new Uri(link);
                string mst = HttpUtility.ParseQueryString(uri.Query).Get("mst");
                string sec = HttpUtility.ParseQueryString(uri.Query).Get("sec");
                string fileNameXML = mst + sec + ".xml";
                fileAttactment.Add(fileNameXML);
            }

                foreach (MimeEntity attachment in message.Attachments)
            {
                if (attachment.ContentType.ToString() == "Content-Type: message/rfc822")
                {
                    var rfc822 = (MessagePart)attachment;
                    var emlMessage = rfc822.Message;

                    mailMessageEML.Id = emlMessage.MessageId;
                    mailMessageEML.From = emlMessage.From.ToString();
                    mailMessageEML.To = emlMessage.To.ToString();
                    mailMessageEML.TimeReceive = emlMessage.Date;
                    mailMessageEML.Subject = emlMessage.Subject;
                    mailMessageEML.Body = emlMessage.TextBody;
                    string pattern = @"tracuu\.ehoadon\.vn<([^>]+)>";

                    Regex regex = new Regex(pattern);

                    MatchCollection matches = regex.Matches(mailMessageEML.Body);

                    foreach (Match match in matches)
                    {
                        string link = match.Groups[1].Value;
                        //Process.Start(link);
                        IWebDriver driver = new ChromeDriver();

                        // Truy cập vào trang web cần tải
                        driver.Navigate().GoToUrl(link);

                        // Thực thi lệnh JavaScript:__doPostBack('LinkDownXML','')
                        IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                        js.ExecuteScript("javascript:__doPostBack('LinkDownXML','')");

                        // Đợi một khoảng thời gian để trang web hoàn tất việc tải xuống
                        System.Threading.Thread.Sleep(5000);

                        // Đóng trình duyệt
                        
                    }

                    string fileNameEML = mailMessageEML.Subject + ".eml";
                    fileAttactment.Add(fileNameEML);
                    messageEML.Add(mailMessageEML);
                }
                else
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
                        using (ZipArchive archive = ZipFile.Open(localFilePath + "/" + fileName, ZipArchiveMode.Update))
                        {
                            foreach (var file in archive.Entries)
                            {
                                var fileNameinZIP = file.FullName;
                                var duoi = fileNameinZIP.Substring(fileNameinZIP.LastIndexOf(".") + 1);
                                if (System.IO.File.Exists(localFilePath + "/" + fileNameinZIP))
                                {
                                    System.IO.File.Delete(localFilePath + "/" + fileNameinZIP);
                                }
                                if (duoi == "xml" || duoi == "pdf")
                                {
                                    fileAttactment.Add(fileNameinZIP);
                                    //var NameFile = fileNameinZIP.Substring(0, fileNameinZIP.LastIndexOf('.'));
                                }
                            }
                            archive.ExtractToDirectory(localFilePath);
                        }
                    }
                    if (mimeType == "application/octet-stream")
                    {
                        using (var archive = ArchiveFactory.Open(localFilePath + "/" + fileName))
                        {
                            foreach (var entry in archive.Entries)
                            {
                                var fileNameinRAR = entry.Key;
                                var duoi = fileNameinRAR.Substring(fileNameinRAR.LastIndexOf(".") + 1);
                                if (!entry.IsDirectory)
                                {
                                    entry.WriteToDirectory(localFilePath, new ExtractionOptions()
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
            }
            getDetailEmail.FileAttactment = string.Join(";", fileAttactment);
            getDetailEmail.Typefilename = string.Join("; ", Typefilename);

            return Json(new { data = getDetailEmail }, JsonRequestBehavior.AllowGet);
        }

        //Hàm lấy theo khoảng ngày
        public IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return day;
        }

        //lấy đường dẫn tới file theo 10 ngày ( từ ngày hiện tại về 10 ngày trước)
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

        //----------Đọc file XML---------------------
        //Input là ngày nhận thư (stringdate) và tên file(fileName)
        //Output là đối tượng Hóa đơn(Invoice) ở dạng Json
        public JsonResult ReadXML(string stringdate, string fileName)
        {
            string filePath = Server.MapPath("~/Attachment/" + stringdate + "/" + fileName); // đường dẫn tệp XML
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(filePath);
            var invoice = new InvoiceEntity(); //Khai báo đối tượng hóa đơn

            SignedXml signedXml = new SignedXml(xmlDocument);
            XmlNodeList nodeList = xmlDocument.GetElementsByTagName("Signature"); //Truy vấn đến Tag Signature
            signedXml.LoadXml((XmlElement)nodeList.Item(0));
            
            //đọc thông tin Chứng nhận
            var certificate = xmlDocument.GetElementsByTagName("X509Certificate").Item(0).InnerText; 
            byte[] tmp;
            tmp = Convert.FromBase64String(certificate);
            X509Certificate2 cer = new X509Certificate2(tmp);

            bool verifiedXml = signedXml.CheckSignature(cer.PublicKey.Key); //kiểm tra chữ ký số
            //truy vấn node mã của cơ quan thuế
            var nodeMCQT = xmlDocument.SelectSingleNode("/TDiep/DLieu/HDon/MCCQT"); //Truy vấn đến thẻ Mã Cơ quan thuế
            //truy vấn node ngày lập hóa đơn
            var nodeNgayLap = xmlDocument.SelectSingleNode("/TDiep/DLieu/HDon/DLHDon/TTChung/NLap");
            //truy vấn node Mgafy Ký của người bán
            var nodeNgayKy = xmlDocument.GetElementsByTagName("SigningTime").Item(0);
            DateTime NKy = DateTime.Parse(nodeNgayKy.InnerText).Date; //Định dạng ngày đăng ký

            if (!verifiedXml)
            {
                return Json(new { status = false, message = "Dữ liệu hóa đơn đã bị thay đổi" });

            }
            else if (nodeMCQT == null)
            {
                return Json(new { error = "Thiếu mã cơ quan thuế" }, JsonRequestBehavior.AllowGet);

            }
            else if (nodeNgayKy == null || nodeNgayLap == null)
            {
                return Json(new { error = "Thiếu ngày lập hoặc ngày ký" }, JsonRequestBehavior.AllowGet);

            }
            else if (DateTime.Parse(nodeNgayLap.InnerText).Date != DateTime.Parse(nodeNgayKy.InnerText).Date)
            {
                return Json(new { error = "Ngày ký và ngày lập không hợp lệ" }, JsonRequestBehavior.AllowGet);

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
                buyer.NKy = NKy.ToString("yyyy-MM-dd");
                invoice.buyer = buyer;

                //Truy vấn vào node child trong danh sách hàng hóa dịch vụ
                XmlNodeList listServiceProductNode = xmlDocument.SelectNodes("/TDiep/DLieu/HDon/DLHDon/NDHDon/DSHHDVu/HHDVu");
                var listServiceProduct = new List<ServiceProduct>();

                foreach (XmlNode node in listServiceProductNode)
                {
                    var serviceProduct = new ServiceProduct();
                    //đọc thông tin từng node hàng hóa dịch vụ
                    serviceProduct.TChat = ReadNodeList(node, "TChat");
                    serviceProduct.STT = ReadNodeList(node, "STT");
                    serviceProduct.MHHDVu = ReadNodeList(node, "MHHDVu");
                    serviceProduct.THHDVu = ReadNodeList(node, "THHDVu");
                    serviceProduct.DVTinh = ReadNodeList(node, "DVTinh");
                    serviceProduct.SLuong = ReadNodeList(node, "SLuong");
                    serviceProduct.DGia = ReadNodeList(node, "DGia");
                    serviceProduct.TLCKhau = ReadNodeList(node, "TLCKhau");
                    serviceProduct.ThTien = ReadNodeList(node, "ThTien");
                    serviceProduct.TSuat = ReadNodeList(node, "TSuat");

                    listServiceProduct.Add(serviceProduct);//add từng node child vào danh sách
                }
                invoice.serviceProducts = listServiceProduct;

                //đọc thông tin thanh toán
                var pay = new Pay();
                pay.TSuat = ReadLineXML(xmlDocument, "/NDHDon/TToan/THTTLTSuat/LTSuat/TSuat");
                pay.ThTien = ReadLineXML(xmlDocument, "/NDHDon/TToan/THTTLTSuat/LTSuat/ThTien");
                pay.TThue = ReadLineXML(xmlDocument, "/NDHDon/TToan/THTTLTSuat/LTSuat/TThue");
                pay.TgTCThue = ReadLineXML(xmlDocument, "/NDHDon/TToan/TgTCThue");
                pay.TgTThue = ReadLineXML(xmlDocument, "/NDHDon/TToan/TgTThue");
                pay.TTCKTMai = ReadLineXML(xmlDocument, "/NDHDon/TToan/TTCKTMai");
                pay.TgTTTBSo = ReadLineXML(xmlDocument, "/NDHDon/TToan/TgTTTBSo");
                pay.TgTTTBChu = ReadLineXML(xmlDocument, "/NDHDon/TToan/TgTTTBChu");
                invoice.pay = pay;

                //Truy vấn vào node chile trong danh sách loại Phí
                XmlNodeList listFeeTypeNode = xmlDocument.SelectNodes("/TDiep/DLieu/HDon/DLHDon/NDHDon/TToan/DSLPhi/LPhi");
                var listFeeType = new List<FeeType>();
                foreach(XmlNode node in listFeeTypeNode)
                {
                    var feeType = new FeeType();
                    //đọc thông tin từng node Loại Phí
                    feeType.TLPhi = ReadNodeList(node, "TLPhi");
                    feeType.TPhi = ReadNodeList(node, "TPhi");
                    listFeeType.Add(feeType);
                }
                invoice.feeType = listFeeType;

                //đọc thông tin chữ ký điện tử
                var Certificate2 = new x509Certificate2();
                Certificate2.Issueser = cer.Issuer;
                Certificate2.Subject = cer.Subject;
                invoice.Certificate2 = Certificate2;

                //đọc thông tin mã của cơ quan thuế
                invoice.MCCQT = nodeMCQT.InnerText;

            }
            return Json(new { data = invoice }, JsonRequestBehavior.AllowGet);
        }

        //kiểm tra node đơn với
        //Input là object XmlDocument và tên node đơn
        //Output là chuỗi giá trị của node trong xml nếu node tồn tại, ngược lại thì trả về rỗng
        private string ReadLineXML(XmlDocument xmlDocument, string elementXml)
        {
            string pathNodeXML = "/TDiep/DLieu/HDon/DLHDon";
            var result = xmlDocument.SelectSingleNode(pathNodeXML + elementXml);//truy vấn vào node đơn
            if (result == null)
            {
                return "";
            }
            else
            {
                return result.InnerText;
            }
        }

        //kiểm tra các node trong List
        //Input là object XmlNode và tên node con
        //Output là chuỗi giá trị của node con trong xml nếu node tồn tại, ngược lại thì trả về rỗng
        private string ReadNodeList(XmlNode node, string elementXml)
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
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        //Hàm Đăng nhập
        //    [HttpPost]
        public ActionResult Login()
        {
            return View();
        }

        //Hàm đăng xuất
        public ActionResult Logout()
        {
            Session["keySession"] = null;
            return RedirectToAction("Index", "Home");
        }
    }
}