using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PwManager
{
    public class User
    {
        public User()
        {
            DefaultPasswordLength = 10;
            CopyTimeout = 30;
            Theme = "Default";
            AutoLogoutTimeInSeconds = 240;
        }
        public int CopyTimeout { get; set; }
        public int TotalAccountGroups { get; set; }
        public ObservableCollection<Account> accounts { get; set; }
        public ObservableCollection<AccountGroup> accountGroups { get; set; }
        public int DefaultPasswordLength { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public int UserId { get; set; }
        public string UserPassword { get; set; }
        public DateTime Created { get; set; }
        public DateTime PasswordLastUpdated { get; set; }
        public byte[] IV { get; set; }
        public string Theme { get; set; }
        public int AutoLogoutTimeInSeconds { get; set; }
        
    }
}
