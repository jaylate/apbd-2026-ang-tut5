namespace LegacyRenewalApp
{
    public class NoteBuilder : INoteBuilder
    {
        private string _notes { get; set; }

        public void AddNote(string s)
        {
            _notes += $"{s}; ";
        }

        public override string ToString()
        {
            return _notes.Trim();
        }
    }
}
