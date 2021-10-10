using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace ExpressionMapEditor.Pages
{

    public partial class Index
    {
        public int NumberOfNotesToTranspose { get; set; } = 12;
        public string TransposeFromNote { get; set; } = "C";
        public int TransposeFromNoteOctave { get; set; } = 1;
        public string TransposeToNote { get; set; } = "C";
        public int TransposeToNoteOctave { get; set; } = 5;
        public async Task Transpose()
        {
            var startNote = new Note(TransposeFromNote, TransposeFromNoteOctave);
            var endNote = new Note(TransposeToNote, TransposeToNoteOctave);
            int delta = endNote.Value - startNote.Value;

            var startNoteValue = startNote.Value;
            var lastNoteValue = startNote.Value + NumberOfNotesToTranspose - 1;

            foreach (var note in FilteredArticulations.SelectMany(a => a.Notes.Where(n => n.Value >= startNoteValue && n.Value <= lastNoteValue)))
            {
                note.Value += delta;
            }
            await OnChanged();
        }

        public bool CheckAll
        {
            get => checkAll;
            set
            {
                checkAll = value;
                foreach (var a in Articulations) { a.Checked = checkAll; }
            }
        }
        private bool checkAll;

        protected override async Task OnInitializedAsync()
        {
            //global::ExpressionMapEditor.Pages.
            await Load();
        }

        public List<string> ScaleNotes = new()
        {
            "C",
            "C#",
            "D",
            "D#",
            "E",
            "F",
            "F#",
            "G",
            "G#",
            "A",
            "A#",
            "B",
        };
        public IEnumerable<int> Octaves = Enumerable.Range(-2, 11);



        public List<string> CurrentGroupFilter = new(Enumerable.Repeat("(All)", 4));

        [Parameter]
        //public string SavePath { get; set; } = @"C:\src\cubase-expression-maps\VSL\SY Brass\SY Trumpet 1.expressionmap";
        public string SavePath { get; set; } = @"C:\src\cubase-expression-maps\VSL\SY Elite Strings\SYE Violins and Violas.expressionmap";

        public void FixAllNames()
        {
            foreach (var articulation in Articulations)
            {
                articulation.Name = articulation.ExpectedName;
            }
        }

        public List<Articulation> Articulations { get; set; } = new List<Articulation>();

        public IEnumerable<Articulation> FilteredArticulations
        {
            get
            {
                foreach (var articulation in Articulations)
                {
                    bool matchesFilter = true;
                    for (int i = 0; i < 4; i++)
                    {
                        if (CurrentGroupFilter[i] != "(All)" && CurrentGroupFilter[i] != articulation.Visuals[i].Text)
                        {
                            matchesFilter = false;
                            break;
                        }
                    }
                    if (matchesFilter) yield return articulation;
                }
            }
        }
        XElement RootElement;
        XDocument Document;

        public List<HashSet<string>> GroupVisuals { get; set; } = new List<HashSet<string>>(Enumerable.Range(0, 4).Select(i => new HashSet<string>()));

        public const string ext = ".expressionmap";
        public async Task Save()
        {

            var savePath = SavePath;
            if (File.Exists(savePath))
            {
                var backupDir = Path.Combine(Path.GetDirectoryName(savePath), "_backup");
                Directory.CreateDirectory(backupDir);
                File.Move(savePath, Path.Combine(backupDir, DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss ") + Path.GetFileName(savePath)));
            }
            //if (!savePath.Contains("-save")) { savePath = savePath.Replace(ext, "") + "-save" + ext; }

            StringBuilder sb = new StringBuilder();
            XmlWriterSettings xws = new XmlWriterSettings()
            {
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = true,
            };
            //xws.OmitXmlDeclaration = true;
            xws.Indent = true;

            using (XmlWriter xw = XmlWriter.Create(sb, xws))
            {
                RootElement.Save(xw);
            }

            await File.WriteAllTextAsync(savePath, sb.ToString());
            SavePath = savePath;
        }

        //public void AddNote()
        //{
        //}

        internal void LoadGroupVisuals(XElement x)
        {
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

                    var text = visual.Elements().Where(e => e.Attribute("name").Value == "text").FirstOrDefault()?.Attribute("value").Value;

                    var hashSet = GroupVisuals[groupId];
                    if (!hashSet.Contains(text)) { hashSet.Add(text); }
                }
            }
        }


        public async Task Load()
        {
            Articulations.Clear();

            if (!File.Exists(SavePath)) { return; }

            GroupVisuals = new List<HashSet<string>>(Enumerable.Range(0, 4).Select(i => new HashSet<string>(new string[] { "(All)" })));

            using var fs = new FileStream(SavePath, FileMode.Open);
            Document = await XDocument.LoadAsync(fs, LoadOptions.PreserveWhitespace, default);
            RootElement = Document.Root;
            //docElement = await XElement.LoadAsync(fs, LoadOptions.PreserveWhitespace, default);

            //HashSet<int> ids = new HashSet<int>();

            //foreach (var d in doc.Descendants())
            //{
            //    var idString = d.Attribute("ID");
            //    if (idString == null) continue;

            //    if (idString.Value == "x")
            //    {
            //        d.SetAttributeValue("ID", nextId.ToString());
            //        nextId += 400;
            //    }
            //}
            //foreach (var d in doc.Descendants())
            //{
            //    var idString = d.Attribute("ID");
            //    if (idString == null) continue;

            //    var id = Convert.ToInt32(idString.Value);
            //    if (ids.Contains(id))
            //    {
            //        int newId;
            //        newId = id;
            //        do
            //        {
            //            newId++;
            //        } while (ids.Contains(newId));
            //        d.SetAttributeValue("ID", newId.ToString());
            //        Debug.WriteLine($"replacing duplicate ID: {idString} => {newId}");
            //        id = newId;
            //    }
            //    ids.Add(id);
            //    if (nextId < id)
            //    {
            //        nextId = id + 1;
            //    }
            //}

            // InstrumentMap Member name="slots"
            //var lv1s = from lv1 in doc.Descendants("member")
            //           where lv1.Attribute("name").Value == "slots"
            //           select lv1.Descendants("list").First().Descendants("obj").Where(d => d.Attribute("class").Value == "PSoundSlot")
            //  ;

            var articulationsListWrapper =
                from lv1 in RootElement.Descendants("member")
                where lv1.Attribute("name").Value == "slots"
                select lv1;

            var articulationsList =
                from e in articulationsListWrapper.Elements()
                where e.Name == "list"
                select e;

            var soundslots =
                from s in articulationsList.Elements()
                where s.Attribute("class").Value == "PSoundSlot"
                select s;

            var names = from s in soundslots.Elements()
                        where s.Name == "member" && s.Attribute("name").Value == "name"
                        select s.Elements("string").First().Attribute("value").Value;

            var articulations = new List<Articulation>();

            foreach (var x in soundslots)
            {
                var a = new Articulation();
                a.Document = new ArticulationDocument(x);

                a.LoadVisuals();
                LoadGroupVisuals(x);

                var action = x.Elements().Where(e => e.Attribute("class").Value == "PSlotMidiAction").FirstOrDefault();
                if (action != null)
                {
                    var messages = action.Elements().Where(e => e.Attribute("name").Value == "midiMessages").FirstOrDefault().Element("list");
                    if (messages != null)
                    {
                        foreach (var message in messages.Elements())
                        {
                            a.Notes.Add(new Note(message.Elements().Where(m => m.Attribute("name").Value == "data1").FirstOrDefault()));
                        }
                    }
                }

                articulations.Add(a);
            }

            Articulations = articulations;
            Change = new EventCallback(null, new Action(() => OnChange(null, null)));

            //CloneNotes();
            StateHasChanged();
        }

        private void CloneNotes()
        {
            Dictionary<string, string> sourceNotes = new Dictionary<string, string>();

            foreach (var dest in Articulations.ToArray().Where(a => a.Visuals.Count > 0 && a.Visuals[0].Text == "slur"))
            {
                var source = Articulations.ToArray().Where(a =>
                {
                    if (a.Visuals.Count == 0) return false;
                    if (a.Visuals[0].Text != "legato") return false;
                    for (int i = 1; i < a.Visuals.Count; i++)
                    {
                        if (a.Visuals[i].Text != dest.Visuals[i].Text) return false;
                    }
                    return true;
                }).FirstOrDefault();

                if (source == null)
                {
                    Debug.WriteLine("Couldn't find source for " + dest);
                    continue;
                }


                var list = source.Document.XElement.Descendants("list").Select(l => l.ToString()).Where(s => s.Contains("POutputEvent")).FirstOrDefault();

                var destNode = dest.Document.XElement.Descendants("member").Where(o => o.Attribute("name").Value == "midiMessages").FirstOrDefault();

                Debug.WriteLine(destNode);

                destNode.Add(XElement.Parse(list));
            }

        }

    private void CloneArticulations() // HARDCODED
    {
        foreach (var a in Articulations.ToArray().Where(a => a.Visuals.Count > 0 && a.Visuals[0].Text == "legato"))
        {
            a.Notes.Add(new Note("A#", 1));
            var notes = new List<Note>(a.Notes);
            notes[notes.Count - 1] = new Note("B", 1);
            var visuals = new List<Visual>(a.Visuals);
            visuals[0] = new Visual { Description = "slur", Text = "slur" };

            Articulations.Add(Articulation.CloneFrom(a, RootElement.Document, a.Name.Replace("legato", "slur")));
        }
    }

    public void Change2(ChangeEventArgs args)
    {
        _Change(0, "staccato short");
    }
    public EventCallback Change { get; set; }
    public void OnChange(object sender, object args)
    {
        Debug.WriteLine(args.ToString());
    }
    public void _Change(int i, string t)
    {
        //MudSelect a;
        //a.onch
        CurrentGroupFilter[i] = t;
        StateHasChanged();
        //Debug.WriteLine(i.ToString() + " " + t);
    }

    public void SelectedValuesChanged()
    {
        Debug.WriteLine("");
    }

    public string NoteToAdd { get; set; } = "C";
    public int NoteOctaveToAdd { get; set; } = 1;
    public async void AddNote()
    {
        bool changedSomething = true;
        //using var fs = new FileStream(Path, FileMode.Open);
        //doc = await XElement.LoadAsync(fs, LoadOptions.PreserveWhitespace, default);

        var note = new Note(NoteToAdd, NoteOctaveToAdd);

        var lv1s =
            from lv1 in RootElement.Descendants("member")
            where lv1.Attribute("name").Value == "slots"
            select lv1;

        var list =
            from toolbox in lv1s.Elements()
            where toolbox.Name == "list"
            select toolbox;

        var soundslots =
            from s in list.Elements()
            where s.Attribute("class").Value == "PSoundSlot"
            select s;

        var names = from s in soundslots.Elements()
                    where s.Name == "member" && s.Attribute("name").Value == "name"
                    select s.Elements("string").First().Attribute("value").Value;

        var articulations = new List<Articulation>();

        foreach (var x in soundslots)
        {
            var a = new Articulation();
            a.Document = new ArticulationDocument(x);
            //a.Name = x.Descendants("member")
            //    .Where(d => d.Attribute("name").Value == "name")
            //    .FirstOrDefault()
            //    ?.Descendants("string")
            //    .FirstOrDefault()
            //    .Attribute("value")?.Value;

            if (Articulations.Where(articulation => a.Name == articulation.Name).FirstOrDefault().Checked != true) continue;

            var visuals = x.Descendants("member")
            .Where(d => d.Attribute("name").Value == "sv")
            .FirstOrDefault()
            ?.Element("list")?.Elements();

            bool matchesFilters = true;

            for (int i = 0; i < 4; i++)
            {
                a.Visuals.Add(new Visual());
            }

            foreach (var visual in visuals)
            {
                var group = visual.Elements().Where(e => e.Attribute("name").Value == "group").FirstOrDefault();
                if (group == null) { continue; }
                int groupId = Convert.ToInt32(group.Attribute("value")?.Value ?? "-1");
                if (groupId == -1 || groupId >= 4) continue;
                a.Visuals[groupId].Text = visual.Elements().Where(e => e.Attribute("name").Value == "text").FirstOrDefault()?.Attribute("value").Value;

                if (CurrentGroupFilter[groupId] != AllValue && CurrentGroupFilter[groupId] != a.Visuals[groupId].Text)
                {
                    matchesFilters = false;
                    break;
                }



                var hashSet = GroupVisuals[groupId];
                if (!hashSet.Contains(a.Visuals[groupId].Text)) { hashSet.Add(a.Visuals[groupId].Text); }
                a.Visuals[groupId].Description = visual.Elements().Where(e => e.Attribute("name").Value == "description").FirstOrDefault()?.Attribute("value").Value;
            }
            if (!matchesFilters) continue;

            bool alreadyHasNote = false;
            var action = x.Elements().Where(e => e.Attribute("class").Value == "PSlotMidiAction").FirstOrDefault();
            XElement messages = null;
            XElement messagesList = null;
            if (action != null)
            {
                messages = action.Elements().Where(e => e.Attribute("name").Value == "midiMessages").FirstOrDefault();
                messagesList = messages.Element("list");
                if (messagesList != null)
                {
                    foreach (var message in messagesList.Elements())
                    {
                        var noteValueStr = message.Elements().Where(m => m.Attribute("name").Value == "data1").FirstOrDefault().Attribute("value").Value;
                        var noteObj = new Note(Convert.ToInt32(noteValueStr));

                        if (noteObj.Octave == NoteOctaveToAdd && noteObj.NoteString == NoteToAdd)
                        {
                            alreadyHasNote = true;
                        }
                    }
                }
            }

            if (alreadyHasNote)
            {
                Debug.WriteLine($"{a.Name} - Already has note: {NoteToAdd}-{NoteOctaveToAdd}");

            }
            else
            {
                Debug.WriteLine($"{a.Name} - Adding note: {NoteToAdd}-{NoteOctaveToAdd}");
                changedSomething = true;

                if (action == null)
                {
                    Debug.WriteLine($"{a.Name} - TODO: add PSlotMidiAction element");
                }
                if (messages == null)
                {
                    messages = new XElement("member");
                    messages.SetAttributeValue("name", "midiMessages");
                    action.Add(messages);

                    var ownership = new XElement("int");
                    ownership.SetAttributeValue("name", "ownership");
                    ownership.SetAttributeValue("value", "1");
                    messages.Add(ownership);
                }
                if (messagesList == null)
                {
                    messagesList = new XElement("list");
                    messagesList.SetAttributeValue("name", "obj");
                    messagesList.SetAttributeValue("type", "obj");
                    messages.Add(messagesList);
                }

                var noteObj = new XElement("obj");
                noteObj.SetAttributeValue("class", "POutputEvent");
                noteObj.SetAttributeValue("ID", nextId++.ToString());

                var status = new XElement("int");
                status.SetAttributeValue("name", "status");
                status.SetAttributeValue("value", "144");


                var data1 = new XElement("int");
                data1.SetAttributeValue("name", "data1");
                data1.SetAttributeValue("value", note.Value);
                var data2 = new XElement("int");
                data2.SetAttributeValue("name", "data2");
                data2.SetAttributeValue("value", "120");
                noteObj.Add(status, data1, data2);

                messagesList.Add(noteObj);
            }
        }

        if (changedSomething)
        {
            await Save();
            await Load();
        }

    }
    public static int nextId = 80369104;

    public const string AllValue = "(All)";

    public async Task RemoveNote(Articulation articulation, Note note)
    {
        articulation.Document.RemoveNote(note);
        await OnChanged();
    }
    private async Task OnChanged()
    {
        await Save();
        await Load();
    }
}
}
