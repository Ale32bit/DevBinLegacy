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
            if (!Directory.Exists(dataPath)) {
                Directory.CreateDirectory(dataPath);
            }

            Console.WriteLine("Paste data directory is " + DataPath);
        }

        public void Write(string id, string content) {
            if (UseCompression) {
                byte[] byteArray = Encoding.ASCII.GetBytes(content);
                using MemoryStream originalStream = new MemoryStream(byteArray);
                using FileStream compressedFileStream = File.Create(Path.Combine(DataPath, id));
                using GZipStream gZipStream = new GZipStream(compressedFileStream, CompressionMode.Compress);
                originalStream.CopyTo(gZipStream);
                File.WriteAllText(Path.Combine(DataPath, id), content);
            }
            else {
                File.WriteAllText(Path.Combine(DataPath, id), content);
            }
        }

        public string Read(string id) {
            if (File.Exists(Path.Combine(DataPath, id))) {
                if (UseCompression) {
                    try {
                        using FileStream originalFileStream = File.OpenRead(Path.Combine(DataPath, id));
                        using MemoryStream decompressedFileStream = new MemoryStream();
                        using GZipStream gZipStream = new GZipStream(originalFileStream, CompressionMode.Decompress);
                        gZipStream.CopyTo(decompressedFileStream);

                        return Encoding.ASCII.GetString(decompressedFileStream.ToArray());
                    }
                    catch (InvalidDataException) {
                        return File.ReadAllText(Path.Combine(DataPath, id));
                    }
                }
                else {
                    return File.ReadAllText(Path.Combine(DataPath, id));
                }
            }

            return string.Empty;
        }
    }
}