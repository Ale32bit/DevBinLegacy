using System.Net.Mail;
using System.Text.RegularExpressions;

namespace DevBin {
    public class User {
        public int ID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        private string Password;

        public User(string password) {
            Password = password;
        }

        public bool PasswordMatch(string password) {
            return BCrypt.Net.BCrypt.EnhancedVerify(password, Password);
        }

        public static string Hash(string password) {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11, true);
        }

        public static bool IsUsernameValid(string username) {
            var regex = new Regex(@"^[A-Za-z0-9_]{3,32}$", RegexOptions.Compiled);
            return regex.IsMatch(username);
        }

        public static bool isEmailValid(string email) {
            try {
                var addr = new MailAddress(email);
                return addr.Address == email;
            } catch {
                return false;
            }
        }
    }
}
