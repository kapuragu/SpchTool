using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Reflection;

namespace SpchTool
{
    internal static class Program
    {
        private const string DefaultHashDumpFileName = "spch_hash_dump_dictionary.txt";

        private static void Main(string[] args)
        {
            var hashManager = new HashManager();

            // Multi-Dictionary Reading!!
            List<string> dictionaryNames = new List<string>
            {
                "spch_dictionary.txt",
                "spch_label_dictionary.txt",
                "spch_voicetype_dictionary.txt",
                "spch_anim_dictionary.txt",
                "spch_user_dictionary.txt",
            };

            List<string> dictionaries = new List<string>();

            foreach (var dictionaryPath in dictionaryNames)
                if (File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/" + dictionaryPath))
                    dictionaries.Add(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/" + dictionaryPath);

			hashManager.StrCode32LookupTable = MakeHashLookupTableFromFiles(dictionaries, FoxHash.Type.StrCode32);

            List<string> UserStrings = new List<string>();

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
                        CollectUserStrings(spch, hashManager, UserStrings);
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
            WriteUserStringsToFile(UserStrings);
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
        /// Opens a file containing one string per line from the input table of files, hashes each string, and adds each pair to a lookup table.
        /// </summary>
        private static Dictionary<uint, string> MakeHashLookupTableFromFiles(List<string> paths, FoxHash.Type hashType)
        {
            ConcurrentDictionary<uint, string> table = new ConcurrentDictionary<uint, string>();

            // Read file
            List<string> stringLiterals = new List<string>();
            foreach (var dictionary in paths)
            {
                using (StreamReader file = new StreamReader(dictionary))
                {
                    // TODO multi-thread
                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        stringLiterals.Add(line);
                    }
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
        public static void CollectUserStrings(SpchFile spch, HashManager hashManager, List<string> UserStrings)
        {
            foreach (var label in spch.Labels) // Analyze hashes
            {
                if (UserStringCheck(label.LabelName.StringLiteral, UserStrings, hashManager))
                    UserStrings.Add(label.LabelName.StringLiteral);
                foreach (var voiceClip in label.VoiceClips)
                {
                    if (UserStringCheck(voiceClip.VoiceType.StringLiteral, UserStrings, hashManager))
                        UserStrings.Add(voiceClip.VoiceType.StringLiteral);
                    if (UserStringCheck(voiceClip.AnimationAct.StringLiteral, UserStrings, hashManager))
                        UserStrings.Add(voiceClip.AnimationAct.StringLiteral);
                }
            }
        }
        public static bool UserStringCheck(string userString, List<string> list, HashManager hashManager)
        {
            if (!hashManager.StrCode32LookupTable.ContainsValue(userString) && !list.Contains(userString))
                return true;
            else
                return false;
        }
        public static void WriteUserStringsToFile(List<string> UserStrings)
        {
            UserStrings.Sort(); //Sort alphabetically for neatness
            var UserDictionary = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/" + "spch_user_dictionary.txt";
            foreach (var userString in UserStrings)
                using (StreamWriter file = new StreamWriter(UserDictionary, append: true))
                    file.WriteLine(userString); //Write them into the user dictionary
        }
    }
}
