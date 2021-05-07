using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace DevBin {
    public class PasteFs {
        public string DataPath { get; set; }
        public bool UseCompression { get; set; }

        public PasteFs(string dataPath, bool useCompression = false) {
            DataPath = Path.Combine(Environment.CurrentDirectory, dataPath);
            UseCompression = useCompression;
            if ( !Directory.Exists(dataPath) ) {
                Directory.CreateDirectory(dataPath);
            }

            Console.WriteLine("Paste data directory is " + DataPath);
        }

        public void Write(string id, string content) {
            if ( UseCompression ) {
                byte[] byteArray = Encoding.ASCII.GetBytes(content);
                MemoryStream stream = new(byteArray);

                using var writeStream = File.OpenWrite(Path.Combine(DataPath, id));
                using var brotli = new BrotliStream(writeStream, CompressionMode.Compress);
                stream.CopyTo(brotli);
            } else {
                File.WriteAllText(Path.Combine(DataPath, id), content);
            }

        }

        public string Read(string id) {
            if ( File.Exists(Path.Combine(DataPath, id)) ) {
                if ( UseCompression ) {
                    using FileStream inputStream = File.OpenRead(Path.Combine(DataPath, id)); 
                    using Stream outputStream = new MemoryStream();
                    using BrotliStream brotli = new(inputStream, CompressionMode.Decompress); brotli.CopyTo(outputStream);
                    var reader = new StreamReader(outputStream);

                    Console.WriteLine("TEST: " + reader.ReadToEnd());
                    return "x";
                } else {
                    return File.ReadAllText(Path.Combine(DataPath, id));
                }
            }

            return "";
        }
    }
}
