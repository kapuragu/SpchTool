using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Globalization;

namespace SpchTool
{
    public class SpchLabel
    {
        public FoxHash LabelName { get; set; }
        public uint SbpListId { get; set; }
        public uint VoiceClipCount { get; set; }
        public List<SpchVoiceClip> VoiceClips = new List<SpchVoiceClip>();
        public void Read(BinaryReader reader, Dictionary<uint, string> nameLookupTable, HashIdentifiedDelegate hashIdentifiedCallback)
        {
            LabelName = new FoxHash(FoxHash.Type.StrCode32);
            LabelName.Read(reader, nameLookupTable, hashIdentifiedCallback);
            SbpListId = reader.ReadUInt32();
            VoiceClipCount = reader.ReadUInt32();

            Console.WriteLine($"Label name: {LabelName.StringLiteral}");
            Console.WriteLine($"Sbp list Id: {SbpListId}");
            Console.WriteLine($"Voice clip count Id: {VoiceClipCount}");
            for (int i = 0; i < VoiceClipCount; i++)
            {
                SpchVoiceClip voiceClip = new SpchVoiceClip();
                voiceClip.Read(reader, nameLookupTable, hashIdentifiedCallback);
                VoiceClips.Add(voiceClip);
            }
        }
        public void Write(BinaryWriter writer)
        {
            LabelName.Write(writer);
            writer.Write(SbpListId);
            writer.Write(VoiceClips.Count);
            foreach (var voiceClip in VoiceClips)
            {
                voiceClip.Write(writer);
            }
        }
        public void ReadXml(XmlReader reader)
        {
            LabelName = new FoxHash(FoxHash.Type.StrCode32);
            LabelName.ReadXml(reader, "labelName");
            SbpListId = uint.Parse(reader["sbpListId"]);
            reader.ReadStartElement("label");

            Console.WriteLine($"Label name: {LabelName.StringLiteral}");
            Console.WriteLine($"Sbp list Id: {SbpListId}");
            while (2 > 1)
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        SpchVoiceClip voiceClip = new SpchVoiceClip();
                        voiceClip.ReadXml(reader);
                        VoiceClips.Add(voiceClip);
                        continue;
                    case XmlNodeType.EndElement:
                        reader.Read();
                        return;
                }
            }
        }
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("label");
            LabelName.WriteXml(writer, "labelName");
            writer.WriteAttributeString("sbpListId", SbpListId.ToString(CultureInfo.InvariantCulture));

            Console.WriteLine($"Label name: {LabelName.StringLiteral}");
            Console.WriteLine($"Sbp list Id: {SbpListId}");
            foreach (SpchVoiceClip voiceClip in VoiceClips)
            {
                writer.WriteStartElement("voiceClip");
                voiceClip.WriteXml(writer);
            }
            writer.WriteEndElement();
        }
        public XmlSchema GetSchema() { return null; }
    }
}
