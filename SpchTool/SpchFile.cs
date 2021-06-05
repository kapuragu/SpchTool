using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Globalization;

namespace SpchTool
{
    public interface ILabels : IXmlSerializable
    {
        void Read(BinaryReader reader, Dictionary<uint, string> hashLookupTable, HashIdentifiedDelegate hashIdentifiedCallback);
        void Write(BinaryWriter writer);
    }
    public interface IVoiceClips : IXmlSerializable
    {
        void Read(BinaryReader reader, Dictionary<uint, string> hashLookupTable, HashIdentifiedDelegate hashIdentifiedCallback);
        void Write(BinaryWriter writer);
    }
    public class LabelEntry : ILabels
    {
        public FoxHash LabelName { get; set; }
        public uint SbpListId { get; set; }
        public uint VoiceClipCount { get; set; }
        public List<IVoiceClips> VoiceClips = new List<IVoiceClips>();
        public void Read(BinaryReader reader, Dictionary<uint, string> nameLookupTable, HashIdentifiedDelegate hashIdentifiedCallback)
        {
            LabelName = new FoxHash(FoxHash.Type.StrCode32);
            LabelName.Read(reader, nameLookupTable, hashIdentifiedCallback);
            SbpListId = reader.ReadUInt32();
            VoiceClipCount = reader.ReadUInt32();
            Console.WriteLine($"Label name: {LabelName}");
            Console.WriteLine($"Sbp list Id: {SbpListId}");
            Console.WriteLine($"Voice clip count Id: {VoiceClipCount}");
            for (int i = 0; i < VoiceClipCount; i++)
            {
                IVoiceClips voiceClip = new VoiceClip();
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
            Console.WriteLine($"Label name: {LabelName}");
            Console.WriteLine($"Sbp list Id: {SbpListId}");
            reader.ReadStartElement("label");
            while (2 > 1)
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        IVoiceClips voiceClip = new VoiceClip();
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
            foreach (IVoiceClips voiceClip in VoiceClips)
            {
                writer.WriteStartElement("voiceClip");
                voiceClip.WriteXml(writer);
            }
            writer.WriteEndElement();
        }
        public XmlSchema GetSchema() { return null; }
    }
    public class VoiceClip : IVoiceClips
    {
        public FoxHash VoiceType { get; set; }
        public uint SbpVoiceClip { get; set; }
        public FoxHash AnimationAct { get; set; }
        public float BeforePause { get; set; }
        public float AfterPause { get; set; }
        public void Read(BinaryReader reader, Dictionary<uint, string> nameLookupTable, HashIdentifiedDelegate hashIdentifiedCallback)
        {
            VoiceType = new FoxHash(FoxHash.Type.StrCode32);
            VoiceType.Read(reader, nameLookupTable, hashIdentifiedCallback);
            SbpVoiceClip = reader.ReadUInt32();
            AnimationAct = new FoxHash(FoxHash.Type.StrCode32);
            AnimationAct.Read(reader, nameLookupTable, hashIdentifiedCallback);
            BeforePause = reader.ReadSingle();
            AfterPause = reader.ReadSingle();

            Console.WriteLine($"    Voice type: {VoiceType}");
            Console.WriteLine($"    Sbp VoiceClip: {SbpVoiceClip}");
            Console.WriteLine($"    Animation: {AnimationAct}");
            Console.WriteLine($"    Pause before: {BeforePause}");
            Console.WriteLine($"    Pause after: {AfterPause}");
        }
        public void Write(BinaryWriter writer)
        {
            VoiceType.Write(writer);
            writer.Write(SbpVoiceClip);
            AnimationAct.Write(writer);
            writer.Write(BeforePause);
            writer.Write(AfterPause);

            Console.WriteLine($"    Voice type: {VoiceType}");
            Console.WriteLine($"    Sbp VoiceClip: {SbpVoiceClip}");
            Console.WriteLine($"    Animation: {AnimationAct}");
            Console.WriteLine($"    Pause before: {BeforePause}");
            Console.WriteLine($"    Pause after: {AfterPause}");
        }
        public void ReadXml(XmlReader reader)
        {
            VoiceType = new FoxHash(FoxHash.Type.StrCode32);
            VoiceType.ReadXml(reader, "voiceType");

            SbpVoiceClip = uint.Parse(reader["sbpVoiceClipId"]);

            AnimationAct = new FoxHash(FoxHash.Type.StrCode32);
            AnimationAct.ReadXml(reader, "animationAct");

            BeforePause = Extensions.ParseFloatRoundtrip(reader["beforePause"]);

            AfterPause = Extensions.ParseFloatRoundtrip(reader["afterPause"]);

            reader.ReadStartElement("voiceClip");

            Console.WriteLine($"    Voice type: {VoiceType}");
            Console.WriteLine($"    Sbp VoiceClip: {SbpVoiceClip}");
            Console.WriteLine($"    Animation: {AnimationAct}");
            Console.WriteLine($"    Pause before: {BeforePause}");
            Console.WriteLine($"    Pause after: {AfterPause}");
        }

        public void WriteXml(XmlWriter writer)
        {
            VoiceType.WriteXml(writer, "voiceType");
            writer.WriteAttributeString("sbpVoiceClipId", SbpVoiceClip.ToString(CultureInfo.InvariantCulture));
            AnimationAct.WriteXml(writer, "animationAct");
            writer.WriteAttributeString("beforePause", BeforePause.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("afterPause", AfterPause.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
        }

        public XmlSchema GetSchema() {return null;}
    }
    public class SpchFile : IXmlSerializable
    {
        public List<ILabels> Labels = new List<ILabels>();
        public List<IVoiceClips> VoiceClips = new List<IVoiceClips>();
        public void Read(BinaryReader reader, HashManager hashManager)
        {
            // Read header
            uint signature = reader.ReadUInt32();
            if (signature!=1751347315) //'SPCH'
            {
                Console.WriteLine("Wrong signature!!! Not a SPCH file?");
                throw new ArgumentOutOfRangeException();
            };
            reader.BaseStream.Position += 2; //ushort endianness
            ushort labelCount = reader.ReadUInt16();
            Console.WriteLine($"Labels count: {labelCount}");
            reader.BaseStream.Position += 8*labelCount; // Padding of 8 bytes per label
            for (int i = 0; i < labelCount; i++)
            {
                ILabels label = new LabelEntry();
                label.Read(reader, hashManager.StrCode32LookupTable, hashManager.OnHashIdentified);
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
                        ILabels label = new LabelEntry();
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
            foreach (ILabels label in Labels)
            {
                label.WriteXml(writer);
            }
            writer.WriteEndDocument();
        }
        public XmlSchema GetSchema() {return null;}
    }
}
