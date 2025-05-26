using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBAccessLibrary.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string UserPassword { get; set; }
        public DateTime DateRegistered { get; set; }
        public DateTime LastSeen { get; set; }
    }
}
