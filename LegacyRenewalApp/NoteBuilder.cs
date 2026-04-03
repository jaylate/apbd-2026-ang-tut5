using System;

namespace LegacyRenewalApp
{
    public class NoteBuilder : INoteBuilder
    {
        private string _notes = string.Empty;

        public void AddNote(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
		    return;
            }
            _notes += $"{s.Trim()}; ";
        }

        public override string ToString()
        {
            return _notes.Trim();
        }
    }
}
