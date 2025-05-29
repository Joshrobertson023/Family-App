using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBAccessLibrary.Models
{
    public class VerseModel
    {
        public int Id { get; set; }
        public string Reference { get; set; }
        public int UsersSaved { get; set; }
        public int UsersHighlighted { get; set; }
    }
}
