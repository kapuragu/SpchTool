using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace SpchTool
{
    public class SpchLabel
    {
        public FoxHash LabelName { get; set; }
        public FnvHash VoiceEvent { get; set; }
        public uint VoiceClipCount { get; set; }
        public List<SpchVoiceClip> VoiceClips = new List<SpchVoiceClip>();
        public void Read(BinaryReader reader, HashManager hashManager, HashIdentifiedDelegate hashIdentifiedCallback)
        {
            LabelName = new FoxHash();
            LabelName.Read(reader, hashManager.StrCode32LookupTable, hashIdentifiedCallback);
            VoiceEvent = new FnvHash();
            VoiceEvent.Read(reader, hashManager.Fnv1LookupTable, hashIdentifiedCallback);
            VoiceClipCount = reader.ReadUInt32();

            Console.WriteLine($"Label name: {LabelName.StringLiteral}");
            Console.WriteLine($"Sbp list Id: {VoiceEvent.StringLiteral}");
            Console.WriteLine($"Voice clip count Id: {VoiceClipCount}");
            for (int i = 0; i < VoiceClipCount; i++)
            {
                SpchVoiceClip voiceClip = new SpchVoiceClip();
                voiceClip.Read(reader, hashManager, hashIdentifiedCallback);
                VoiceClips.Add(voiceClip);
            }
        }
        public void Write(BinaryWriter writer)
        {
            LabelName.Write(writer);
            VoiceEvent.Write(writer);
            writer.Write(VoiceClips.Count);
            foreach (var voiceClip in VoiceClips)
            {
                voiceClip.Write(writer);
            }
        }
        public void ReadXml(XmlReader reader)
        {
            LabelName = new FoxHash();
            LabelName.ReadXml(reader, "labelName");
            VoiceEvent = new FnvHash();
            VoiceEvent.ReadXml(reader, "sbpListId"); //todo rename to something more official
            reader.ReadStartElement("label");

            Console.WriteLine($"Label name: {LabelName.StringLiteral}");
            Console.WriteLine($"Sbp list Id: {VoiceEvent.StringLiteral}");
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
            VoiceEvent.WriteXml(writer, "sbpListId");

            Console.WriteLine($"Label name: {LabelName.StringLiteral}");
            Console.WriteLine($"Sbp list Id: {VoiceEvent.StringLiteral}");
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
