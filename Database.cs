using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace DevBin {
    public class Database {
        public const string CreateSQL = @"
/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

CREATE TABLE IF NOT EXISTS `pastes` (
  `id` varchar(8) NOT NULL,
  `authorId` int(11) DEFAULT NULL,
  `title` varchar(255) NOT NULL DEFAULT 'Unnamed paste',
  `syntax` varchar(255) NOT NULL DEFAULT 'plaintext',
  `exposure` tinyint(3) unsigned NOT NULL DEFAULT 0,
  `timestamp` timestamp NOT NULL DEFAULT current_timestamp(),
  `updateTimestamp` timestamp NULL DEFAULT NULL,
  `views` int(10) unsigned NOT NULL DEFAULT 0,
  `contentCache` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `Author` (`authorId`),
  CONSTRAINT `Author` FOREIGN KEY (`authorId`) REFERENCES `users` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `users` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `username` varchar(63) NOT NULL,
  `email` varchar(255) NOT NULL,
  `password` varchar(255) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `Unique` (`username`,`email`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;";
        public string ConnectionString { get; set; }
        public Database(string connectionString) {
            ConnectionString = connectionString;
        }
        public MySqlConnection GetConnection() {
            return new MySqlConnection(ConnectionString);
        }

#nullable enable
        // This method also increases views field!
        public Paste? FetchPaste(string id, MySqlConnection conn) {
            Paste? paste = null;
            if ( conn.State != System.Data.ConnectionState.Open ) {
                conn.Open();
            }

            MySqlCommand cmd = new($"UPDATE `pastes` SET views = views+1 WHERE id = @id; SELECT * FROM `pastes` WHERE id = @id;", conn);
            cmd.Parameters.AddWithValue("@id", id);
            using ( var reader = cmd.ExecuteReader() ) {
                if ( reader.Read() ) {
                    paste = new Paste() {
                        ID = reader.GetString("id"),
                        Title = reader.GetString("title"),
                        Syntax = reader.GetString("syntax"),
                        Exposure = (Paste.PasteExposure)reader.GetByte("exposure"),
                        Date = reader.GetDateTime("timestamp"),
                        Views = reader.GetUInt32("views"),
                        ContentCache = reader.GetString("contentCache") ?? "",
                    };
                }
            }

            conn.Close();

            return paste;
        }
        public Paste? FetchPaste(string id) {
            MySqlConnection conn = GetConnection();
            return FetchPaste(id, conn);
        }

        public Paste[] GetLatest(int n = 30) {
            List<Paste> pastes = new();
            MySqlConnection conn = GetConnection();

            conn.Open();

            MySqlCommand cmd = new($"SELECT * FROM `pastes` WHERE exposure = 0 ORDER BY timestamp DESC LIMIT {n};", conn);
            using ( var reader = cmd.ExecuteReader() ) {
                while ( reader.Read() ) {
                    pastes.Add(new Paste() {
                        ID = reader.GetString("id"),
                        Title = reader.GetString("title"),
                        Syntax = reader.GetString("syntax"),
                        Exposure = (Paste.PasteExposure)reader.GetByte("exposure"),
                        Date = reader.GetDateTime("timestamp"),
                        Views = reader.GetUInt32("views"),
                        ContentCache = reader.GetString("contentCache") ?? "",
                    });
                }
            }

            conn.Close();
            return pastes.ToArray();
        }

        public bool Exists(string id, MySqlConnection conn) {
            if ( conn.State != System.Data.ConnectionState.Open ) {
                conn.Open();
            }

            MySqlCommand cmd = new($"SELECT * FROM `pastes` WHERE id = @id;", conn);
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader(); return reader.HasRows;
        }
        public bool Exists(string id) {
            MySqlConnection conn = GetConnection();
            return Exists(id, conn);
        }

        public string Upload(Paste paste) {
            string id;

            paste.Title = paste.Title.Substring(0, Math.Min(255, paste.Title.Length));
            paste.Syntax = paste.Syntax.Substring(0, Math.Min(255, paste.Syntax.Length));

            using ( MySqlConnection conn = GetConnection() ) {
                conn.Open();

                do {
                    id = RandomID();
                } while ( Exists(id, conn) );

                MySqlCommand cmd = new(@"INSERT INTO `pastes` (
                    `id`, `authorId`, `title`, `syntax`, `exposure`, `contentCache`
                ) VALUES (
                    @id, @authorId, @title, @syntax, @exposure, @contentCache
                );", conn);

                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@authorId", null);
                cmd.Parameters.AddWithValue("@title", paste.Title);
                cmd.Parameters.AddWithValue("@syntax", paste.Syntax);
                cmd.Parameters.AddWithValue("@exposure", (byte)paste.Exposure);
                cmd.Parameters.AddWithValue("@contentCache", paste.ContentCache);

                int affected = cmd.ExecuteNonQuery();
                conn.Close();
            }

            return id;
        }

        public bool Update(Paste paste, MySqlConnection conn) {
            paste.Title = paste.Title.Substring(0, Math.Min(255, paste.Title.Length));
            paste.Syntax = paste.Syntax.Substring(0, Math.Min(255, paste.Syntax.Length));

            if ( conn.State != System.Data.ConnectionState.Open ) {
                conn.Open();
            }

            MySqlCommand cmd = new(@"UPDATE pastes
                                    SET (title = @title, syntax = @syntax, exposure = @exposure, contentCache = @contentCache)
                                    WHERE id = @id;");

            cmd.Parameters.AddWithValue("@id", paste.ID);
            cmd.Parameters.AddWithValue("@title", paste.Title);
            cmd.Parameters.AddWithValue("@syntax", paste.Syntax);
            cmd.Parameters.AddWithValue("@exposure", (byte)paste.Exposure);
            cmd.Parameters.AddWithValue("@contentCache", paste.ContentCache);

            return cmd.ExecuteNonQuery() == 1;
        }

        public bool Update(Paste paste) {
            MySqlConnection conn = GetConnection();
            return Update(paste, conn);
        }

        public bool Delete(Paste paste, MySqlConnection conn) {
            if ( conn.State != System.Data.ConnectionState.Open ) {
                conn.Open();
            }

            MySqlCommand cmd = new(@"DELETE FROM pastes WHERE id = @id;");

            cmd.Parameters.AddWithValue("@id", paste.ID);

            return cmd.ExecuteNonQuery() == 1;
        }

        public bool Delete(Paste paste) {
            MySqlConnection conn = GetConnection();
            return Delete(paste, conn);
        }

        /*public User CreateUser(string name, string email, string password) {
            string hash = BCrypt.Net.BCrypt.EnhancedHashPassword(password);
            User user = new User(hash) {

            };
        }*/

        public User? FetchUser(int loginDetail) {
            MySqlConnection conn = GetConnection();
            conn.Open();

            User user;

            MySqlCommand cmd = new(@"SELECT * FROM users WHERE username = @loginDetail OR email = @loginDetail;");

            cmd.Parameters.AddWithValue("@loginDetail", loginDetail);

            using ( var reader = cmd.ExecuteReader() ) {
                if ( reader.Read() ) {
                    user = new User(reader.GetString("password")) {
                        ID = reader.GetInt32("id"),
                        Username = reader.GetString("username"),
                        Email = reader.GetString("email"),
                    };

                    return user;
                }
            }

            return null;
        }


        internal static string RandomID() {
            string code = "";

            string alpha = "abcdefghijklmnopqrstuvwxyz";

            Random random = new();

            for ( int i = 0; i < 8; i++ ) {
                string ch = alpha[random.Next(0, alpha.Length)].ToString();
                if ( random.Next(0, 2) > 0 ) {
                    ch = ch.ToUpper();
                }
                code += ch;
            }

            return code;
        }
    }
}
