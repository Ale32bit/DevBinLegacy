using System.Net.Mail;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace DevBin {
    public class User {
        public int ID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        private readonly string? Password;


        public User(string password) {
            Password = password;
        }

        public User() {
            Password = null;
        }

        public override string ToString() {
            return Username;
        }

        public string GenerateSessionToken() {
            var token = Utils.RandomString(256);

            Database.Instance.InsertSessionToken(ID, token);
            
            return token;
        }

        public bool PasswordMatch(string password) {
            return BCrypt.Net.BCrypt.EnhancedVerify(password, Password);
        }

        /// <summary>
        /// Delete an user account and all its belonging pastes
        /// </summary>
        /// <param name="password">The user password in plain text, for safety measures</param>
        /// <returns>Whether deleted from database</returns>
        public bool DeleteUser(string password) {
            if (!PasswordMatch(password)) return false;
            
            var pastes = Database.Instance.GetUserPastes(this);

            foreach (var paste in pastes) {
                PasteFs.Instance.Delete(paste.ID);
            }

            return Database.Instance.DeleteUser(this);
        }

        public static string Hash(string password) {
            return BCrypt.Net.BCrypt.HashPassword(password, Program.BCryptCost, true);
        }

        public static bool IsUsernameValid(string username) {
            var regex = new Regex(@"^[A-Za-z0-9_]{3,32}$", RegexOptions.Compiled);
            return regex.IsMatch(username);
        }

        public static bool IsEmailValid(string email) {
            try {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch {
                return false;
            }
        }

    }

    public class UserProfile {
        public string Username { get; set; }
        public string Email { get; set; }
        public Paste[] Pastes { get; set; }
    }

    public class UserProfileLimited {
        public string Username { get; set; }
        public Paste[] Pastes { get; set; }
    }
}