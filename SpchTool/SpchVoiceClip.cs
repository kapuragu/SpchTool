using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Globalization;

namespace SpchTool
{
    public class SpchVoiceClip
    {
        public FoxHash VoiceType { get; set; }
        public FnvHash VoiceId { get; set; }
        public FoxHash AnimationAct { get; set; }
        public float BeforePause { get; set; }
        public float AfterPause { get; set; }
        public void Read(BinaryReader reader, HashManager hashManager, HashIdentifiedDelegate hashIdentifiedCallback)
        {
            VoiceType = new FoxHash();
            VoiceType.Read(reader, hashManager.StrCode32LookupTable, hashIdentifiedCallback);
            VoiceId = new FnvHash();
            VoiceId.Read(reader, hashManager.Fnv1LookupTable, hashIdentifiedCallback);
            AnimationAct = new FoxHash();
            AnimationAct.Read(reader, hashManager.StrCode32LookupTable, hashIdentifiedCallback);
            BeforePause = reader.ReadSingle();
            AfterPause = reader.ReadSingle();

            Console.WriteLine($"    Voice type: {VoiceType.StringLiteral}");
            Console.WriteLine($"    Sbp VoiceClip: {VoiceId.StringLiteral}");
            Console.WriteLine($"    Animation: {AnimationAct.StringLiteral}");
            Console.WriteLine($"    Pause before: {BeforePause}");
            Console.WriteLine($"    Pause after: {AfterPause}");
        }
        public void Write(BinaryWriter writer)
        {
            VoiceType.Write(writer);
            VoiceId.Write(writer);
            AnimationAct.Write(writer);
            writer.Write(BeforePause);
            writer.Write(AfterPause);

            Console.WriteLine($"    Voice type: {VoiceType.StringLiteral}");
            Console.WriteLine($"    Sbp VoiceClip: {VoiceId.StringLiteral}");
            Console.WriteLine($"    Animation: {AnimationAct.StringLiteral}");
            Console.WriteLine($"    Pause before: {BeforePause}");
            Console.WriteLine($"    Pause after: {AfterPause}");
        }
        public void ReadXml(XmlReader reader)
        {
            VoiceType = new FoxHash();
            VoiceType.ReadXml(reader, "voiceType");
            VoiceId = new FnvHash();
            VoiceId.ReadXml(reader, "sbpVoiceClipId"); //todo rename to something more official
            AnimationAct = new FoxHash();
            AnimationAct.ReadXml(reader, "animationAct");
            BeforePause = Extensions.ParseFloatRoundtrip(reader["beforePause"]);
            AfterPause = Extensions.ParseFloatRoundtrip(reader["afterPause"]);
            reader.ReadStartElement("voiceClip");

            Console.WriteLine($"    Voice type: {VoiceType.StringLiteral}");
            Console.WriteLine($"    Sbp VoiceClip: {VoiceId.StringLiteral}");
            Console.WriteLine($"    Animation: {AnimationAct.StringLiteral}");
            Console.WriteLine($"    Pause before: {BeforePause}");
            Console.WriteLine($"    Pause after: {AfterPause}");
        }

        public void WriteXml(XmlWriter writer)
        {
            VoiceType.WriteXml(writer, "voiceType");
            VoiceId.WriteXml(writer, "sbpVoiceClipId");
            AnimationAct.WriteXml(writer, "animationAct");
            writer.WriteAttributeString("beforePause", BeforePause.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("afterPause", AfterPause.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();

            Console.WriteLine($"    Voice type: {VoiceType.StringLiteral}");
            Console.WriteLine($"    Sbp VoiceClip: {VoiceId.StringLiteral}");
            Console.WriteLine($"    Animation: {AnimationAct.StringLiteral}");
            Console.WriteLine($"    Pause before: {BeforePause}");
            Console.WriteLine($"    Pause after: {AfterPause}");
        }

        public XmlSchema GetSchema() {return null;}
    }
}
