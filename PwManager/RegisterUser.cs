using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

namespace PwManager
{
    public class RegisterUser
    {

        public bool UserExists(string userName)
        {
            if (File.Exists(@$"Users/{userName}.dbxt"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void CreateUserFile(string userName)
        {
            //Delete File if it already exists
            File.Delete(@$"Users/{userName}.dbxt");
            //Create the User Database File
            using (var myFile = File.CreateText(@$"Users/{userName}.dbxt"))
            {
                // interact with myFile here, it will be disposed automatically
            }
        }
    }
}
