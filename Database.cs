using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace DevBin {
    public class Database {
        public string ConnectionString { get; set; }
        public Database(string connectionString) {
            ConnectionString = connectionString;
        }
        public MySqlConnection GetConnection() {
            return new MySqlConnection(ConnectionString);
        }

#nullable enable
        public Paste? FetchPaste(string id) {
            Paste? paste = null;
            using ( MySqlConnection conn = GetConnection() ) {
                conn.Open();

                MySqlCommand cmd = new MySqlCommand($"SELECT * FROM `pastes` WHERE id = @id;", conn);
                cmd.Parameters.AddWithValue("@id", id);
                using ( var reader = cmd.ExecuteReader() ) {
                    if ( reader.Read() ) {
                        paste = new Paste() {
                            ID = reader.GetString("id"),
                            Title = reader.GetString("title"),
                            Syntax = reader.GetString("syntax"),
                            Exposure = (Paste.PasteExposure)reader.GetByte("exposure"),
                            Date = reader.GetDateTime("timestamp"),
                        };
                    }
                }
            }
            return paste;
        }

        public bool Exists(string id) {
            using ( MySqlConnection conn = GetConnection() ) {
                conn.Open();

                MySqlCommand cmd = new MySqlCommand($"SELECT * FROM `pastes` WHERE id = '{MySqlHelper.EscapeString(id)}';", conn);
                using ( var reader = cmd.ExecuteReader() ) {
                    return reader.HasRows;
                }
            }
        }

        public string Upload(Paste paste) {
            string id = RandomID();
            while ( Exists(id) ) {
                id = RandomID();
            }

            using ( MySqlConnection conn = GetConnection() ) {
                conn.Open();

                MySqlCommand cmd = new MySqlCommand(@"INSERT INTO `pastes` (
                    `id`, `authorId`, `title`, `syntax`, `exposure`
                ) VALUES (
                    @id, @authorId, @title, @syntax, @exposure
                );", conn);

                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@authorId", null);
                cmd.Parameters.AddWithValue("@title", paste.Title);
                cmd.Parameters.AddWithValue("@syntax", paste.Syntax);
                cmd.Parameters.AddWithValue("@exposure", (byte)paste.Exposure);

                int affected = cmd.ExecuteNonQuery();
            }

            return id;
        }

        internal static string RandomID() {
            string code = "";

            string alpha = "abcdefghijklmnopqrstuvwxyz";

            Random random = new Random();

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
