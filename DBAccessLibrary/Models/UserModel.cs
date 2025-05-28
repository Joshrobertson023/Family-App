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
        public string FirstName { get; set; }
        public string FullName
        {
            get 
            {
                return (FirstName.Substring(0, 1).ToUpper() + FirstName.Substring(1) + " " 
                        + LastName.Substring(0, 1).ToUpper() + LastName.Substring(1));
            }
        }
        public string LastName { get; set; }
        public string? Email { get; set; }
        public string UserPassword { get; set; }
        public DateTime DateRegistered { get; set; }
        public DateTime LastSeen { get; set; }
    }
}
