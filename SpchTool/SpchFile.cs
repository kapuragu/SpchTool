using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace SpchTool
{
    public class SpchFile : IXmlSerializable
    {
        public List<SpchLabel> Labels = new List<SpchLabel>();
        public List<SpchVoiceClip> VoiceClips = new List<SpchVoiceClip>();
        public void Read(BinaryReader reader, HashManager hashManager)
        {
            // Read header
            uint signature = reader.ReadUInt32();
            if (signature!=1751347315) //'spch'
            {
                Console.WriteLine("Wrong signature!!! Not a SPCH file?");
                throw new ArgumentOutOfRangeException();
            };
            reader.BaseStream.Position += 2; //ushort endianness
            ushort labelCount = reader.ReadUInt16();
            reader.BaseStream.Position += 8 * labelCount; // Padding of 8 bytes per label

            Console.WriteLine($"Labels count: {labelCount}");

            for (int i = 0; i < labelCount; i++)
            {
                SpchLabel label = new SpchLabel();
                label.Read(reader, hashManager, hashManager.OnHashIdentified);
                Labels.Add(label);
            }
        }
        public void Write(BinaryWriter writer)
        {
            // Write header
            writer.Write('s'); writer.Write('p'); writer.Write('c'); writer.Write('h');
            writer.Write((ushort)0);
            writer.Write((ushort)Labels.Count);
            for (int i = 0; i < Labels.Count; i++)
            {
                writer.Write(0);
                writer.Write(0);
            }
            foreach (var label in Labels)
            {
                label.Write(writer);
            }
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement("spch");
            while (2 > 1)
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        SpchLabel label = new SpchLabel();
                        label.ReadXml(reader);
                        Labels.Add(label);
                        continue;
                    case XmlNodeType.EndElement:
                        return;
                }
            }
        }
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("spch");
            foreach (SpchLabel label in Labels)
            {
                label.WriteXml(writer);
            }
            writer.WriteEndDocument();
        }
        public XmlSchema GetSchema() {return null;}
    }
}
