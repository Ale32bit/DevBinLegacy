using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;

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
    }
}
