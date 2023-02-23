using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication1.Models;

namespace WebApp_ReadXML.Models
{
    public class InvoiceEntity
    {
        public GeneralInformation generalInformation { get; set; }
        public Seller seller { get; set; }
        public Buyer buyer { get; set; }
        public List<ServiceProduct> serviceProducts { get; set; }
        public Pay pay { get; set; }
        public x509Certificate2 Certificate2 { get; set; }
    }
}