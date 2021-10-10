using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Xml.Linq;
using static MudBlazor.Colors;

namespace ExpressionMapEditor
{
    public class ArticulationDocument
    {
        public static int NextId = 800000000;

        public static int GetNextId() // REVIEW - how to do this?
        {
            NextId += 200;
            return NextId;
        }
        //public ArticulationDocument(XDocument doc, string name)
        //{

        //}

        public ArticulationDocument(XElement XElement)
        {
            this.XElement = XElement;
            action = XElement.Elements().Where(e => e.Attribute("class") != null && e.Attribute("class").Value == "PSlotMidiAction").FirstOrDefault();
            if (action != null)
            {
                messages = action.Elements().Where(e => e.Attribute("name").Value == "midiMessages").FirstOrDefault();
                messagesList = messages.Element("list");
            }



        }
        public XElement XElement { get; }
        public XElement action { get; }
        public XElement messages { get; }
        public XElement messagesList { get; }

        public IEnumerable<XElement> Notes
        {
            get
            {
                foreach (var message in messagesList.Elements())
                {
                    yield return message;
                }
            }
        }

        public Note XElementToNote(XElement message)
        {
            var noteValueStr = message.Elements().Where(m => m.Attribute("name").Value == "data1").FirstOrDefault().Attribute("value").Value;
            return new Note(Convert.ToInt32(noteValueStr));
        }

        public void RemoveNote(Note note)
        {
            if (messagesList == null) return;

            foreach (var noteElement in Notes.ToArray())
            {
                if (XElementToNote(noteElement).Value == note.Value)
                {
                    noteElement.Remove();
                    break;
                }
            }
        }
    }


    public class Articulation
    {
        public Articulation()
        {
            while (Visuals.Count < 4) { Visuals.Add(new Visual()); }

        }

        static Random r = new Random();

        public static Articulation CloneFrom(Articulation other, XDocument doc, string name)
        {

            var notePrototype = @"   <obj class=""POutputEvent"" ID=""%%ID%%"">
                        <int name=""status"" value=""144""/>
                        <int name=""data1"" value=""99999""/>
                        <int name=""data2"" value=""120""/>
                     </obj>";
            var groupPrototype = @"<obj class=""USlotVisuals"" ID=""%%ID%%"">
                     <int name=""displaytype"" value=""1""/>
                     <int name=""articulationtype"" value=""1""/>
                     <int name=""symbol"" value=""73""/>
                     <string name=""text"" value=""%%TEXT%%"" wide=""true""/>
                     <string name=""description"" value=""%%DESCRIPTION%%"" wide=""true""/>
                     <int name=""group"" value=""%%GROUP%%""/>
                  </obj>";

            var sbNotes = new StringBuilder();
            foreach (var note in other.Notes)
            {
                sbNotes.Append(notePrototype
                    .Replace("99999", note.Value.ToString())
                    //.Replace("%%ID%%", r.Next(999999999).ToString())
                    .Replace("%%ID%%", 10.ToString())
                    );
            }

            var sbGroups = new StringBuilder();
            int groupIndex = 0;
            foreach (var visual in other.Visuals)
            {
                string text = visual.Text;
                string description = visual.Description;
                if (text == "legato") { text = "slur"; }
                if (description == "legato") { description = "slur"; }
                sbGroups.Append(groupPrototype
                    .Replace("%%ID%%", r.Next(999999999).ToString())
                    .Replace("%%TEXT%%", text)
                    .Replace("%%DESCRIPTION%%", description)
                    .Replace("%%GROUP%%", groupIndex++.ToString())

                    )
                    ;
            }

            var prototype = XDocument.Parse(@$"<obj class=""PSoundSlot"" ID=""643498448"">
            <obj class=""PSlotThruTrigger"" name=""remote"" ID=""0"">
               <int name=""status"" value=""144""/>
               <int name=""data1"" value=""-1""/>
            </obj>
            <obj class=""PSlotMidiAction"" name=""action"" ID=""0"">
               <int name=""version"" value=""600""/>
               <member name=""noteChanger"">
                  <int name=""ownership"" value=""1""/>
                  <list name=""obj"" type=""obj"">
                     <obj class=""PSlotNoteChanger"" ID=""0"">
                        <int name=""channel"" value=""-1""/>
                        <float name=""velocityFact"" value=""1""/>
                        <float name=""lengthFact"" value=""1""/>
                        <int name=""minVelocity"" value=""0""/>
                        <int name=""maxVelocity"" value=""127""/>
                        <int name=""transpose"" value=""0""/>
                        <int name=""minPitch"" value=""0""/>
                        <int name=""maxPitch"" value=""127""/>
                     </obj>
                  </list>
               </member>
               <member name=""midiMessages"">
                  <int name=""ownership"" value=""1""/>
                  <list name=""obj"" type=""obj"">{sbNotes.ToString()}
                     <!--
                      <obj class=""POutputEvent"" ID=""0"">
                        <int name=""status"" value=""144""/>
                        <int name=""data1"" value=""24""/>
                        <int name=""data2"" value=""120""/>
                     </obj>
-->
                  </list>
               </member>
               <int name=""channel"" value=""-1""/>
               <float name=""velocityFact"" value=""1""/>
               <float name=""lengthFact"" value=""1""/>
               <int name=""minVelocity"" value=""0""/>
               <int name=""maxVelocity"" value=""127""/>
               <int name=""transpose"" value=""0""/>
               <int name=""maxPitch"" value=""127""/>
               <int name=""minPitch"" value=""0""/>
               <int name=""key"" value=""24""/>
               <int name=""key2"" value=""36""/>
            </obj>
            <member name=""sv"">
               <int name=""ownership"" value=""2""/>
               <list name=""obj"" type=""obj""> {sbGroups.ToString()}
<!--
                  <obj class=""USlotVisuals"" ID=""0"">
                     <int name=""displaytype"" value=""1""/>
                     <int name=""articulationtype"" value=""1""/>
                     <int name=""symbol"" value=""73""/>
                     <string name=""text"" value=""staccato short"" wide=""true""/>
                     <string name=""description"" value=""staccato short"" wide=""true""/>
                     <int name=""group"" value=""0""/>
                  </obj>
                  <obj class=""USlotVisuals"" ID=""0"">
                     <int name=""displaytype"" value=""1""/>
                     <int name=""articulationtype"" value=""1""/>
                     <int name=""symbol"" value=""73""/>
                     <string name=""text"" value=""bold"" wide=""true""/>
                     <string name=""description"" value=""bold"" wide=""true""/>
                     <int name=""group"" value=""1""/>
                  </obj>
-->
               </list>
            </member>
            <member name=""name"">
               <string name=""s"" value=""${name}"" wide=""true""/>
            </member>
            <int name=""color"" value=""1""/>
         </obj>".Replace("%%ID%%", (1000000 +  r.Next(999999)).ToString()));

            //result.xdoc = prototype;
            //list.Add(prototype.Root);


            var result = new Articulation()
            {
                Document = new ArticulationDocument(prototype.Root),
                Name = name,
            };

            var articulationsListWrapper =
                          from lv1 in (doc ?? throw new ArgumentNullException(nameof(doc))).Descendants("member")
                          where lv1.Attribute("name").Value == "slots"
                          select lv1;

            var list =
                (from e in articulationsListWrapper.Elements()
                 where e.Name == "list"
                 select e).FirstOrDefault();
            list.Nodes().Last().AddAfterSelf(prototype.Root);

            //var articulation = new XElement("obj");
            //articulation.SetAttributeValue("class", "PSoundSlot");
            //articulation.SetAttributeValue("ID", GetNextId().ToString());
            //list.Add(articulation);

            //var midiAction = new XElement("obj");
            //midiAction.SetAttributeValue("class", "PSlotMidiAction");
            //midiAction.SetAttributeValue("name", "action");
            //midiAction.SetAttributeValue("ID", GetNextId().ToString());
            //articulation.Add(midiAction);

            //{
            //    var midiActionVersion = new XElement("int");
            //    midiActionVersion.SetAttributeValue("name", "version");
            //    midiActionVersion.SetAttributeValue("value", 600);
            //    // 

            //}

            //var soundslots =
            //    from s in list.Elements()
            //    where s.Attribute("class").Value == "PSoundSlot"
            //    select s;

            //var names = from s in soundslots.Elements()
            //            where s.Name == "member" && s.Attribute("name").Value == "name"
            //            select s.Elements("string").First().Attribute("value").Value;

            return result;
        }
        //public Articulation(XDocument doc, string name) : this()
        //{
        //    Document = new ArticulationDocument(doc, name);
        //}

        public bool Checked { get; set; }
        public XElement XElement => Document?.XElement;

        public ArticulationDocument Document { get; set; }

        public string Name
        {
            get
            {
                return XElement?.Descendants("member")
                    .Where(d => d.Attribute("name").Value == "name")
                    .FirstOrDefault()
                    ?.Descendants("string")
                    .FirstOrDefault()
                    .Attribute("value")?.Value;
            }
            set
            {
                var nameEl = XElement.Descendants("member")
                    .Where(d => d.Attribute("name").Value == "name")
                    .FirstOrDefault()
                    ?.Descendants("string")
                    .FirstOrDefault();
                nameEl.SetAttributeValue("value", value);
            }
        }
        public List<Note> Notes { get; set; } = new List<Note>();

        public List<Visual> Visuals
        {
            get => visuals; set
            {
                // TODO
            }
        }
        private List<Visual> visuals = new List<Visual>();

        internal void LoadVisuals()
        {
            var x = Document.XElement;
            var a = this;

            var visuals = x.Descendants("member")
                                .Where(d => d.Attribute("name").Value == "sv")
                                .FirstOrDefault()
                                ?.Element("list")?.Elements();

            if (visuals != null)
            {
                foreach (var visual in visuals)
                {
                    var group = visual.Elements().Where(e => e.Attribute("name").Value == "group").FirstOrDefault();
                    if (group == null) { continue; }
                    int groupId = Convert.ToInt32(group.Attribute("value")?.Value ?? "-1");
                    if (groupId == -1 || groupId >= 4) continue;
                    a.Visuals[groupId].Text = visual.Elements().Where(e => e.Attribute("name").Value == "text").FirstOrDefault()?.Attribute("value").Value;

                    a.Visuals[groupId].Description = visual.Elements().Where(e => e.Attribute("name").Value == "description").FirstOrDefault()?.Attribute("value").Value;
                }
            }
        }
        internal void SetVisuals(IEnumerable<Visual> newVisuals)
        {
            var visuals = Document.XElement.Descendants("member")
                                            .Where(d => d.Attribute("name").Value == "sv")
                                            .FirstOrDefault()
                                            ?.Element("list")?.Elements();



        }

        public bool HasIssue => InvalidName || MissingNotes || InvalidNotes;

        public bool InvalidName => Name.StartsWith("Slot ");
        public bool InvalidNotes => Notes.Where(n => n.Octave == 3 && n.NoteString == "C").Any();
        public bool MissingNotes => !Notes.Any();

        public bool NameMatches => Name == ExpectedName;
        public string ExpectedName
        {
            get
            {
                var list = new List<string>();

                for (int i = 0; i < 4; i++)
                {
                    var chunk = ToGroupName(Visuals[i]);

                    if (chunk == "soft-r" && list.Contains("soft-a"))
                    {
                        list.Remove("soft-a");
                        list.Add("soft-ar");
                    }
                    else if (!string.IsNullOrWhiteSpace(chunk) && !list.Contains(chunk)) { list.Add(chunk); }
                }

                if (list.Count == 0) { return ""; }

                Move(list, "expressivo", 1);

                if (list.Contains("sustain") && list.Contains("expressivo"))
                {
                    list.Remove("sustain");
                    Move(list, "expressivo", 0);
                }

                if (list.Contains("crescendo") || list.Contains("dimuendo"))
                {
                    Move(list, "short", -1);
                    Move(list, "long", -1);
                }
                //Move(list, "bold", 1);
                //Move(list, "agile", 1);

                list.Remove("normal");
                if (list[0] == "staccato" || list[0] == "portato")
                {
                    list.Remove("long");
                    Move(list, "short", 1);
                }

                return list.Aggregate((x, y) => $"{x} {y}");
            }
        }

        private static void Move(List<string> list, string item, int index = 1)
        {
            if (index < 0) { index = Math.Max(0, list.Count + index); }

            if (list.Contains(item) && list.Count > 1)
            {
                list.Remove(item);
                list.Insert(index, item);
            }
        }

        public string ToGroupName(Visual s)
        {
            switch (s.Text)
            {
                case "expressivo1":
                    return "expressivo";

                case "long2":
                    return "long";

                case "normal2":
                    return "normal";
                case "normal3":
                    return "normal";
                case "normal4":
                    return "normal";

                case "con fortissimo":
                    return "con-ff";

                #region Attack

                case "normal attack":
                    return "";
                case "fast attack":
                    return "fast-a";
                case "soft attack":
                    return "soft-a";

                #endregion

                #region Vibrato

                case "reg vibrato":
                    return "";

                case "con vibrato":
                    return "con-v";
                case "molto vibrato":
                    return "molto-v";
                case "senza vibrato":
                    return "senza-v";
                case "senza x molto":
                    return "senza/molto-v";
                case "senza x reg":
                    return "senza/reg-v";
                case "reg x molto":
                    return "reg/molto-v";

                case "senza/con vibrato":
                    return "senza/con-v";


                #endregion

                #region Tremolo

                case "long x tremolo":
                    return "long/tremolo";

                #endregion

                #region Detache

                case "perf detache":
                    return "perf detache";
                //case "perf detache agile":
                //    return "legato detache agile";
                case "agile perf detache":
                    return "agile perf detache";
                case "AS perf detache":
                    return "AS perf detache";

                #endregion

                #region Release

                case "normal release":
                    return "";
                case "cut-release":
                    return "cut-r";
                case "soft release":
                    return "soft-r";

                #endregion

                default:
                    return s.Text;
            }
        }

    }


}
