using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PwManager
{
    public class TipsAndQuotes
    {
        List<String> TipsAndQuotesBank = new List<string>();

        public TipsAndQuotes()
        {
            TipsAndQuotesBank.Add("Tip:" + Environment.NewLine + Environment.NewLine + "Never use personal information such as your name, birthday, user name, or email address.");
            TipsAndQuotesBank.Add("Tip:" + Environment.NewLine + Environment.NewLine + "Avoid using words that can be found in the dictionary. For example, swimming1 would be a weak password.");
            TipsAndQuotesBank.Add("Tip:" + Environment.NewLine + Environment.NewLine + "If you have a password that is 8 characters long or less. Change it Immediately.");
            TipsAndQuotesBank.Add("Tip:" + Environment.NewLine + Environment.NewLine + "We recommend that you use multi-factor authentication for accounts where applicable.");
            TipsAndQuotesBank.Add("Tip:" + Environment.NewLine + Environment.NewLine + "Stop using the same password for various accounts!");
            TipsAndQuotesBank.Add("Tip:" + Environment.NewLine + Environment.NewLine + "Using real words is discouraged but if you must, use phrases for passwords to make them longer and more secure while allowing them to be more easily remembered. E.g. I-Love-Going-For-Walks-At-The-Park");
        }

        public string getRandomQuote()
        {
            int randomQuoteIndex = RandomNumberGenerator.GetInt32(0, TipsAndQuotesBank.Count);
            return TipsAndQuotesBank[randomQuoteIndex];
        }
    }
}
