using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Globalization;

namespace SpchTool
{
    public class SpchVoiceClip
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
}
