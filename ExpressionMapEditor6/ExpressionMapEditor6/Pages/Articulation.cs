using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Xml.Linq;

namespace ExpressionMapEditor6.Pages
{
    public class ArticulationDocument
    {
        public ArticulationDocument(XElement XElement)
        {
            this.XElement = XElement;
            action = XElement.Elements().Where(e => e.Attribute("class").Value == "PSlotMidiAction").FirstOrDefault();
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
            
            foreach(var noteElement in Notes.ToArray())
            {
                if(XElementToNote(noteElement).Value == note.Value)
                {
                    noteElement.Remove();
                    break;
                }
            }
        }
    }


    public class Articulation
    {
        public bool Checked { get; set; }
        public XElement XElement => Document.XElement;

        public ArticulationDocument Document { get; set; }

        public string Name
        {
            get
            {
                return XElement.Descendants("member")
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

        public List<Visual> Visuals { get; } = new List<Visual>();

        public bool HasIssue
        {
            get
            {
                return InvalidName || MissingNotes || InvalidNotes;
            }
        }

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

                    if(chunk == "soft-r" && list.Contains("soft-a"))
                    {
                        list.Remove("soft-a");
                        list.Add("soft-ar");
                    }
                    else if (!string.IsNullOrWhiteSpace(chunk) && !list.Contains(chunk)) { list.Add(chunk); }
                }

                return list.Aggregate((x, y) => $"{x} {y}");
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
