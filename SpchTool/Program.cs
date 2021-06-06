using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SpchTool
{
    internal static class Program
    {
        private const string DefaultDictionaryFileName = "spch_dictionary.txt";
        // private const string DefaultLabelDictionaryFileName = "spch_label_dictionary.txt";
        // private const string DefaultVoiceTypeDictionaryFileName = "spch_voicetype_dictionary.txt";
        // private const string DefaultAnimDictionaryFileName = "spch_anim_dictionary.txt";
        private const string DefaultHashDumpFileName = "spch_hash_dump_dictionary.txt";

        private static void Main(string[] args)
        {
            var hashManager = new HashManager();

            // Read hash dictionaries
            if (File.Exists(DefaultDictionaryFileName))
            {
                hashManager.StrCode32LookupTable = MakeHashLookupTableFromFile(DefaultDictionaryFileName, FoxHash.Type.StrCode32);
            }

            foreach (var spchPath in args)
            {
                if (File.Exists(spchPath))
                {
                    // Read input file
                    string fileExtension = Path.GetExtension(spchPath);
                    if (fileExtension.Equals(".xml", StringComparison.OrdinalIgnoreCase))
                    {
                        SpchFile spch = ReadFromXml(spchPath);
                        WriteToBinary(spch, Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(spchPath)) + ".spch");
                    }
                    else if (fileExtension.Equals(".spch", StringComparison.OrdinalIgnoreCase))
                    {
                        SpchFile spch = ReadFromBinary(spchPath, hashManager);
                        WriteToXml(spch, Path.GetFileNameWithoutExtension(spchPath) + ".spch.xml");
                    }
                    else
                    {
                        throw new IOException("Unrecognized input type.");
                    }
                }
            }

            // Write hash matches output
            WriteHashMatchesToFile(DefaultHashDumpFileName, hashManager);
        }

        public static void WriteToBinary(SpchFile spch, string path)
        {
            using (BinaryWriter writer = new BinaryWriter(new FileStream(path, FileMode.Create)))
            {
                spch.Write(writer);
            }
        }

        public static SpchFile ReadFromBinary(string path, HashManager hashManager)
        {
            SpchFile spch = new SpchFile();
            using (BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open)))
            {
                spch.Read(reader, hashManager);
            }
            return spch;
        }

        public static void WriteToXml(SpchFile spch, string path)
        {
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings()
            {
                Encoding = Encoding.UTF8,
                Indent = true
            };
            using (var writer = XmlWriter.Create(path, xmlWriterSettings))
            {
                spch.WriteXml(writer);
            }
        }

        public static SpchFile ReadFromXml(string path)
        {
            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings
            {
                IgnoreWhitespace = true
            };

            SpchFile spch = new SpchFile();
            using (var reader = XmlReader.Create(path, xmlReaderSettings))
            {
                spch.ReadXml(reader);
            }
            return spch;
        }

        /// <summary>
        /// Opens a file containing one string per line, hashes each string, and adds each pair to a lookup table.
        /// </summary>
        private static Dictionary<uint, string> MakeHashLookupTableFromFile(string path, FoxHash.Type hashType)
        {
            ConcurrentDictionary<uint, string> table = new ConcurrentDictionary<uint, string>();

            // Read file
            List<string> stringLiterals = new List<string>();
            using (StreamReader file = new StreamReader(path))
            {
                // TODO multi-thread
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    stringLiterals.Add(line);
                }
            }

            // Hash entries
            Parallel.ForEach(stringLiterals, (string entry) =>
            {
                if (hashType == FoxHash.Type.StrCode32)
                {
                    uint hash = HashManager.StrCode32(entry);
                    table.TryAdd(hash, entry);
                }
            });

            return new Dictionary<uint, string>(table);
        }

        /// <summary>
        /// Outputs all hash matched strings to a file.
        /// </summary>
        private static void WriteHashMatchesToFile(string path, HashManager hashManager)
        {
            using (StreamWriter file = new StreamWriter(path))
            {
                foreach (var entry in hashManager.UsedHashes)
                {
                    file.WriteLine(entry.Value);
                }
            }
        }
    }
}
