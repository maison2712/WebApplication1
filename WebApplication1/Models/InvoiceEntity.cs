using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp_ReadXML.Models
{
    public class InvoiceEntity
    {
        public Seller seller { get; set; }
        public Buyer buyer { get; set; }
        public List<ServiceProduct> serviceProducts { get; set; }
        public Pay pay { get; set; }

    }
}