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
            List<string> fnvDictionaryNames = new List<string>
            {
                "spch_fnv_voiceevent_dictionary.txt",
                "spch_fnv_voiceid_dictionary.txt",
                "spch_user_dictionary.txt",
            };

            List<string> strCodeDictionaries = new List<string>();
            List<string> fnvDictionaries = new List<string>();

            foreach (var dictionaryPath in dictionaryNames)
                if (File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/" + dictionaryPath))
                    strCodeDictionaries.Add(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/" + dictionaryPath);

            foreach (var dictionaryPath in fnvDictionaryNames)
                if (File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/" + dictionaryPath))
                    fnvDictionaries.Add(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/" + dictionaryPath);

            hashManager.StrCode32LookupTable = MakeStrCode32HashLookupTableFromFiles(strCodeDictionaries);
            hashManager.Fnv1LookupTable = MakeFnv1HashLookupTableFromFiles(fnvDictionaries);

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
        private static Dictionary<uint, string> MakeStrCode32HashLookupTableFromFiles(List<string> paths)
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
                uint hash = HashManager.StrCode32(entry);
                table.TryAdd(hash, entry);
            });

            return new Dictionary<uint, string>(table);
        }
        private static Dictionary<uint, string> MakeFnv1HashLookupTableFromFiles(List<string> paths)
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
                uint hash = HashManager.FNV1Hash32Str(entry);
                table.TryAdd(hash, entry);
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
                if (IsUserString(label.LabelName.StringLiteral, UserStrings, hashManager.StrCode32LookupTable))
                    UserStrings.Add(label.LabelName.StringLiteral);
                if (IsUserString(label.VoiceEvent.StringLiteral, UserStrings, hashManager.Fnv1LookupTable))
                    UserStrings.Add(label.VoiceEvent.StringLiteral);
                foreach (var voiceClip in label.VoiceClips)
                {
                    if (IsUserString(voiceClip.VoiceType.StringLiteral, UserStrings, hashManager.StrCode32LookupTable))
                        UserStrings.Add(voiceClip.VoiceType.StringLiteral);
                    if (IsUserString(voiceClip.VoiceId.StringLiteral, UserStrings, hashManager.Fnv1LookupTable))
                        UserStrings.Add(voiceClip.VoiceId.StringLiteral);
                    if (IsUserString(voiceClip.AnimationAct.StringLiteral, UserStrings, hashManager.StrCode32LookupTable))
                        UserStrings.Add(voiceClip.AnimationAct.StringLiteral);
                }
            }
        }
        public static bool IsUserString(string userString, List<string> list, Dictionary<uint,string> dictionaryTable)
        {
            if (!dictionaryTable.ContainsValue(userString) && !list.Contains(userString))
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
