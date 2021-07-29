using System;

namespace ExpressionMapEditor6.Pages
{
    public class Note
    {
        public Note(int value)
        {
            Value = value;
        }

        public Note(string noteString, int octave)
        {
            var value = (octave + 2) * 12;
            value += NoteToNumber(noteString);
            Value = value;
        }

        public int Value { get; }

        public static int NoteToNumber(string note) => note switch
        {
            "C" => 0,
            "C#" => 1,
            "D" => 2,
            "D#" => 3,
            "E" => 4,
            "F" => 5,
            "F#" => 6,
            "G" => 7,
            "G#" => 8,
            "A" => 9,
            "A#" => 10,
            "B" => 11,
            _ => throw new ArgumentException()
        };

        public int Octave => (Value / 12) - 2;
        public string NoteString
        {
            get => (Value % 12) switch
            {
                0 => "C",
                1 => "C#",
                2 => "D",
                3 => "D#",
                4 => "E",
                5 => "F",
                6 => "F#",
                7 => "G",
                8 => "G#",
                9 => "A",
                10 => "A#",
                11 => "B",
                _ => "",
            };
        }

        public static implicit operator Note(int value) => new Note(value);

        public override string ToString() => $"{NoteString}-{Octave}";
    }
}
