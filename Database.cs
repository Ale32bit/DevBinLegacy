using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;

namespace DevBin {
    public class Database {
        public static Database Instance;

        public const string SqlCreateScript =
            @"CREATE TABLE IF NOT EXISTS `users` (
  `userId` int(11) NOT NULL AUTO_INCREMENT,
  `username` varchar(64) NOT NULL,
  `email` varchar(255) NOT NULL,
  `password` varchar(255) NOT NULL,
  PRIMARY KEY (`userId`),
  UNIQUE KEY `Unique` (`username`,`email`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `pastes` (
  `id` varchar(8) NOT NULL,
  `authorId` int(11) DEFAULT NULL,
  `title` varchar(255) NOT NULL DEFAULT 'Unnamed paste',
  `syntax` varchar(255) NOT NULL DEFAULT 'plaintext',
  `exposure` tinyint(3) unsigned NOT NULL DEFAULT 0,
  `timestamp` timestamp NOT NULL DEFAULT current_timestamp(),
  `updateTimestamp` timestamp NULL DEFAULT NULL,
  `views` int(10) unsigned NOT NULL DEFAULT 0,
  `contentCache` varchar(255) DEFAULT '',
  PRIMARY KEY (`id`),
  KEY `Author` (`authorId`),
  CONSTRAINT `Author` FOREIGN KEY (`authorId`) REFERENCES `users` (`userId`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `session_tokens` (
	`token` VARCHAR(256) NOT NULL COLLATE 'utf8mb4_general_ci',
	`userId` INT(11) NOT NULL DEFAULT '0',
	`timestamp` TIMESTAMP NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
	PRIMARY KEY (`token`) USING BTREE,
	INDEX `UserID` (`userId`) USING BTREE,
	CONSTRAINT `UserID` FOREIGN KEY (`userId`) REFERENCES `users` (`userId`) ON UPDATE NO ACTION ON DELETE CASCADE
)
COLLATE='utf8_general_ci'
ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS `api_tokens` (
	`userId` INT(11) NULL DEFAULT NULL,
	`token` VARCHAR(256) NOT NULL COLLATE 'utf8mb4_general_ci',
	PRIMARY KEY (`token`) USING BTREE,
	INDEX `user` (`userId`) USING BTREE,
	CONSTRAINT `user` FOREIGN KEY (`userId`) REFERENCES `users` (`userId`) ON UPDATE NO ACTION ON DELETE CASCADE 
)
COLLATE='utf8mb4_general_ci'
ENGINE=InnoDB
;

";

        public string ConnectionString { get; set; }

        public Database(string connectionString) {
            ConnectionString = connectionString;

            // Create tables if not exist
            using var conn = GetConnection();
            conn.Open();
            MySqlCommand cmd = new(SqlCreateScript, conn);
            cmd.ExecuteNonQuery();

            TryCreateEvent();

            Instance = this;
        }

        public MySqlConnection GetConnection() {
            return new(ConnectionString);
        }

        private bool TryCreateEvent() {
            string sql = @"CREATE DEFINER=`devbin`@`localhost` EVENT `clear_expired_sessions`
	ON SCHEDULE
		EVERY 15 MINUTE
	ON COMPLETION PRESERVE
	ENABLE
	DO DELETE FROM session_tokens WHERE timestamp < (NOW() - INTERVAL 30 DAY)";

            var conn = GetConnection();

            conn.Open();

            using MySqlCommand cmd = new(sql, conn);
            try {
                cmd.ExecuteNonQuery();
            }
            catch {
                return false;
            }

            return true;
        }

#nullable enable
        // This method also increases views field!
        public Paste? FetchPaste(string id, MySqlConnection conn, bool updateViews = false) {
            Paste? paste = null;
            if (conn.State != ConnectionState.Open) conn.Open();

            int? authorId = null;
            MySqlCommand cmd =
                new(
                    (updateViews ? @"UPDATE `pastes` SET views = views+1 WHERE id = @id;" : "") +
                    @"SELECT * from pastes WHERE pastes.id = @id;"
                    , conn);
            cmd.Parameters.AddWithValue("@id", id);
            using (var reader = cmd.ExecuteReader()) {
                if (reader.Read()) {
                    paste = new Paste {
                        ID = reader.GetString("id"),
                        Title = reader.GetString("title"),
                        Syntax = reader.GetString("syntax"),
                        Exposure = (Paste.Exposures) reader.GetByte("exposure"),
                        Date = reader.GetDateTime("timestamp"),
                        Views = reader.GetUInt32("views"),
                        ContentCache = reader.GetString("contentCache") ?? ""
                    };

                    if (!reader.IsDBNull(reader.GetOrdinal("authorId"))) authorId = reader.GetInt32("authorId");
                }
            }

            if (authorId != null && paste != null) {
                cmd = new MySqlCommand(@"SELECT username FROM `users` WHERE userId = @userId;", conn);
                cmd.Parameters.AddWithValue("@userId", authorId);
                using var reader = cmd.ExecuteReader();
                if (reader.Read()) {
                    paste.Author = reader.GetString("username");
                    paste.AuthorID = authorId;
                }
            }

            conn.Close();

            return paste;
        }

        public Paste? FetchPaste(string id, bool updateViews = false) {
            using MySqlConnection conn = GetConnection();
            return FetchPaste(id, conn, updateViews);
        }

        public Paste[] GetLatest(int n = 30) {
            List<Paste> pastes = new();
            using MySqlConnection conn = GetConnection();

            conn.Open();

            MySqlCommand cmd =
                new(@"SELECT * FROM `pastes` WHERE exposure = 0 ORDER BY timestamp DESC LIMIT @n;", conn);
            cmd.Parameters.AddWithValue("@n", n);
            using (var reader = cmd.ExecuteReader()) {
                while (reader.Read()) {
                    int? authorId = null;
                    var paste = new Paste {
                        ID = reader.GetString("id"),
                        Title = reader.GetString("title"),
                        Syntax = reader.GetString("syntax"),
                        Exposure = (Paste.Exposures) reader.GetByte("exposure"),
                        Date = reader.GetDateTime("timestamp"),
                        Views = reader.GetUInt32("views"),
                        ContentCache = reader.GetString("contentCache") ?? ""
                    };

                    if (!reader.IsDBNull(reader.GetOrdinal("authorId"))) authorId = reader.GetInt32("authorId");
                    if (authorId != null) {
                        using var uconn = GetConnection();
                        uconn.Open();
                        var ucmd = new MySqlCommand(@"SELECT username FROM `users` WHERE userId = @userId;", uconn);
                        ucmd.Parameters.AddWithValue("@userId", authorId);
                        using var ureader = ucmd.ExecuteReader();
                        if (ureader.Read()) {
                            paste.Author = ureader.GetString("username");
                            paste.AuthorID = authorId;
                        }

                    }

                    pastes.Add(paste);
                }
            }

            return pastes.ToArray();
        }

        public bool Exists(string id, MySqlConnection conn) {
            if (conn.State != ConnectionState.Open) conn.Open();

            MySqlCommand cmd = new(@"SELECT * FROM `pastes` WHERE id = @id;", conn);
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            return reader.HasRows;
        }

        public bool Exists(string id) {
            using MySqlConnection conn = GetConnection();
            return Exists(id, conn);
        }

        // TODO: Set author ID
        public string Upload(Paste paste, User? author = null) {
            string id;

            paste.Title = paste.Title[..Math.Min(255, paste.Title.Length)];
            paste.Syntax = paste.Syntax[..Math.Min(255, paste.Syntax.Length)];

            using MySqlConnection conn = GetConnection();
            conn.Open();

            do {
                id = RandomId();
            } while (Exists(id, conn));

            MySqlCommand cmd = new(@"INSERT INTO `pastes` (
                    `id`, `authorId`, `title`, `syntax`, `exposure`, `contentCache`
                ) VALUES (
                    @id, @authorId, @title, @syntax, @exposure, @contentCache
                );", conn);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@authorId", author?.ID);
            cmd.Parameters.AddWithValue("@title", paste.Title);
            cmd.Parameters.AddWithValue("@syntax", paste.Syntax);
            cmd.Parameters.AddWithValue("@exposure", (byte) paste.Exposure);
            cmd.Parameters.AddWithValue("@contentCache", paste.ContentCache);

            cmd.ExecuteNonQuery();
            conn.Close();

            return id;
        }

        public bool Update(Paste paste, MySqlConnection conn) {
            paste.Title = paste.Title[..Math.Min(255, paste.Title.Length)];
            paste.Syntax = paste.Syntax[..Math.Min(255, paste.Syntax.Length)];

            if (conn.State != ConnectionState.Open) conn.Open();

            MySqlCommand cmd = new(@"UPDATE pastes
                                    SET title = @title, syntax = @syntax, exposure = @exposure, contentCache = @contentCache
                                    WHERE id = @id;", conn);

            cmd.Parameters.AddWithValue("@id", paste.ID);
            cmd.Parameters.AddWithValue("@title", paste.Title);
            cmd.Parameters.AddWithValue("@syntax", paste.Syntax);
            cmd.Parameters.AddWithValue("@exposure", (byte) paste.Exposure);
            cmd.Parameters.AddWithValue("@contentCache", paste.ContentCache);

            return cmd.ExecuteNonQuery() == 1;
        }

        public bool Update(Paste paste) {
            using MySqlConnection conn = GetConnection();
            return Update(paste, conn);
        }

        public bool Delete(Paste paste, MySqlConnection conn) {
            if (conn.State != ConnectionState.Open) conn.Open();

            MySqlCommand cmd = new(@"DELETE FROM pastes WHERE id = @id;", conn);

            cmd.Parameters.AddWithValue("@id", paste.ID);

            return cmd.ExecuteNonQuery() == 1;
        }

        public bool Delete(Paste paste) {
            using MySqlConnection conn = GetConnection();
            return Delete(paste, conn);
        }

        public User CreateUser(string name, string email, string password) {
            string hash = BCrypt.Net.BCrypt.EnhancedHashPassword(password);
            User user = new(hash);

            return user;
        }

        public User? FetchUser(string loginDetail) {
            using MySqlConnection conn = GetConnection();
            conn.Open();

            MySqlCommand cmd = new(@"SELECT * FROM users WHERE username = @loginDetail OR email = @loginDetail;", conn);

            cmd.Parameters.AddWithValue("@loginDetail", loginDetail);

            using var reader = cmd.ExecuteReader();
            if (reader.Read()) {
                User user = new(reader.GetString("password")) {
                    ID = reader.GetInt32("userId"),
                    Username = reader.GetString("username"),
                    Email = reader.GetString("email")
                };

                return user;
            }

            return null;
        }

        public User? FetchUser(int id) {
            using MySqlConnection conn = GetConnection();
            conn.Open();

            MySqlCommand cmd = new(@"SELECT * FROM users WHERE userId = @userId;", conn);

            cmd.Parameters.AddWithValue("@userId", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read()) {
                User user = new(reader.GetString("password")) {
                    ID = reader.GetInt32("userId"),
                    Username = reader.GetString("username"),
                    Email = reader.GetString("email")
                };

                return user;
            }

            return null;
        }

        public User? CreateUser(User user, string password) {
            using MySqlConnection conn = GetConnection();
            conn.Open();

            MySqlCommand cmd =
                new(@"INSERT INTO `users` (username, email, password) VALUES (@username, @email, @password);", conn);

            cmd.Parameters.AddWithValue("@username", user.Username);
            cmd.Parameters.AddWithValue("@email", user.Email);
            cmd.Parameters.AddWithValue("@password", password);

            
            var newUser = cmd.ExecuteNonQuery() > 1 ? FetchUser(user.Email) : null;

            return newUser;
        }

        public void InsertSessionToken(int userId, string token) {
            using MySqlConnection conn = GetConnection();
            conn.Open();

            MySqlCommand cmd = new(@"INSERT INTO session_tokens (token, userId) VALUES (@token, @userId)", conn);
            cmd.Parameters.AddWithValue("@token", token);
            cmd.Parameters.AddWithValue("@userId", userId);

            cmd.ExecuteNonQuery();
        }

        public void InvalidateSessionToken(string token) {
            using MySqlConnection conn = GetConnection();
            conn.Open();

            MySqlCommand cmd = new(@"DELETE FROM session_tokens WHERE token = @token;", conn);
            cmd.Parameters.AddWithValue("@token", token);

            cmd.ExecuteNonQuery();
        }

        public User? ResolveSessionToken(string token, bool update = true) {
            using MySqlConnection conn = GetConnection();
            conn.Open();

            // The UPDATE part is just to trigger the timestamp update in the SQL server
            MySqlCommand cmd = new((update ? @"UPDATE session_tokens SET token = token WHERE token = @token;" : "") +
                                   @"SELECT * FROM session_tokens WHERE token = @token;", conn);
            cmd.Parameters.AddWithValue("@token", token);

            using var reader = cmd.ExecuteReader();

            User? user = null;
            
            if (reader.Read()) {
                var userId = reader.GetInt32("userId");
                user = FetchUser(userId);
            }

            return user;
        }

        public Paste[] GetUserPastes(User user) {
            List<Paste> pastes = new();
            using MySqlConnection conn = GetConnection();

            conn.Open();

            MySqlCommand cmd =
                new(@"SELECT * FROM `pastes` WHERE authorId = @userId ORDER BY timestamp;", conn);
            cmd.Parameters.AddWithValue("@userId", user.ID);
            using (var reader = cmd.ExecuteReader()) {
                while (reader.Read()) {
                    var paste = new Paste {
                        ID = reader.GetString("id"),
                        Title = reader.GetString("title"),
                        Syntax = reader.GetString("syntax"),
                        Exposure = (Paste.Exposures) reader.GetByte("exposure"),
                        Date = reader.GetDateTime("timestamp"),
                        Views = reader.GetUInt32("views"),
                        ContentCache = reader.GetString("contentCache") ?? "",
                        Author = user.Username,
                        AuthorID = user.ID,
                    };
                    pastes.Add(paste);
                }
            }

            return pastes.ToArray();
        }

        public bool UpdateUserEmail(User user, string newEmail) {
            using MySqlConnection conn = GetConnection();

            conn.Open();

            MySqlCommand cmd =
                new(@"UPDATE users SET email = @email WHERE userId = @userId;", conn);
            cmd.Parameters.AddWithValue("@email", newEmail);
            cmd.Parameters.AddWithValue("@userId", user.ID);


            var changed = cmd.ExecuteNonQuery() == 1;


            return changed;
        }

        public bool UpdateUserPassword(User user, string newPassword) {
            using MySqlConnection conn = GetConnection();

            conn.Open();

            MySqlCommand cmd =
                new(@"UPDATE users SET password = @password WHERE userId = @userId;", conn);
            cmd.Parameters.AddWithValue("@password", newPassword);
            cmd.Parameters.AddWithValue("@userId", user.ID);


            var changed = cmd.ExecuteNonQuery() == 1;

            return changed;
        }

        public bool DeleteUser(User user) {
            using MySqlConnection conn = GetConnection();

            conn.Open();

            MySqlCommand cmd =
                new(@"DELETE FROM users WHERE userId = @userId;", conn);
            cmd.Parameters.AddWithValue("@userId", user.ID);

            var changed = cmd.ExecuteNonQuery() != 0;

            return changed;
        }

        public User? ResolveApiToken(string token) {
            using MySqlConnection conn = GetConnection();
            conn.Open();

            MySqlCommand cmd =
                new(@"SELECT * FROM api_tokens WHERE token = @token;", conn);
            cmd.Parameters.AddWithValue("@token", token);

            using var reader = cmd.ExecuteReader();

            User? user = null;
            if (reader.Read()) {
                user = FetchUser(reader.GetInt32("userId"));
            }

            conn.Close();

            return user;
        }

        public string? GetUserApiToken(User user) {
            using MySqlConnection conn = GetConnection();
            conn.Open();

            MySqlCommand cmd =
                new(@"SELECT * FROM api_tokens WHERE userId = @userId;", conn);
            cmd.Parameters.AddWithValue("@userId", user.ID);

            using var reader = cmd.ExecuteReader();

            string? token = null;
            if (reader.Read()) {
                token = reader.GetString("token");
            }

            return token;
        }

        public string GenerateToken(User user) {
            string token;
            do {
                token = Utils.RandomString(64);
            } while (ResolveApiToken(token) != null);

            var tokenExists = GetUserApiToken(user) != null;

            using MySqlConnection conn = GetConnection();
            conn.Open();

            MySqlCommand cmd = tokenExists
                ? new MySqlCommand(@"UPDATE api_tokens SET token = @token WHERE userId = @userId;", conn)
                : new MySqlCommand(@"INSERT INTO api_tokens (userId, token) VALUES (@userId, @token);", conn);

            cmd.Parameters.AddWithValue("@userId", user.ID);
            cmd.Parameters.AddWithValue("@token", token);

            cmd.ExecuteNonQuery();

            return token;
        }

        public bool DeleteToken(User user) {
            using MySqlConnection conn = GetConnection();
            conn.Open();

            MySqlCommand cmd = new(@"DELETE FROM api_tokens WHERE userId = @userId;", conn);
            cmd.Parameters.AddWithValue("@userId", user.ID);

            var changed = cmd.ExecuteNonQuery() == 1;

            return changed;
        }
        
        private static string RandomId() {
            string code = "";

            string alpha = "abcdefghijklmnopqrstuvwxyz";

            Random random = new();

            for (var i = 0; i < 8; i++) {
                string ch = alpha[random.Next(0, alpha.Length)].ToString();
                if (random.Next(0, 2) > 0) ch = ch.ToUpper();
                code += ch;
            }

            return code;
        }
    }
}