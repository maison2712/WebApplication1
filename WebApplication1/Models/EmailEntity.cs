using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class EmailEntity
    {
        public string Id { get; set; }
        public string From { get; set; }
        public DateTimeOffset TimeReceive { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string FileAttactment { get; set; }
        public string Typefilename { get; internal set; }
    }
}