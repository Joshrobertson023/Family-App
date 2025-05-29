using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBAccessLibrary.Models
{
    public class SecurityQuestionModel
    {
        public int UserId { get; set; }
        public int Type { get; set; }
        public string Answer { get; set; }
    }
}
