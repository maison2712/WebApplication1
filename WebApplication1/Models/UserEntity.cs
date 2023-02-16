using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Xml.Linq;

namespace WebApplication1.Models
{
    public class UserEntity
    {
        public string user { get; set; }
        public string password { get; set; }
        public UserEntity(string User, string Password)
        {
            user = User;
            password = Password;    
        }
        public UserEntity()
        {
           
        }
    }

}