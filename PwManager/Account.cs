using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PwManager
{
    public class Account
    {
        public Account()
        {
            DisplayName = "";
            Password = "";
            UserName = "";
            Notes = "";
            AssociatedEmail = "";
            URL = "";
            Created = DateTime.Now;
            LastUpdated = DateTime.Now;
            SpecialCharacters = "";
            passwordLength = 0;
        }

        public string DisplayName { get; set; }
        public string SpecialCharacters { get; set; }
        public int passwordLength { get; set; }
        public string Password { get; set; }
        public string UserName { get; set; }
        public string Notes { get; set; }
        public string AssociatedEmail { get; set; }
        public string URL { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Group { get; set; }







    }
}
