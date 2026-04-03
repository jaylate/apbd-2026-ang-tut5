namespace LegacyRenewalApp
{
    public class NoteBuilderFactory : INoteBuilderFactory
    {
        public INoteBuilder Create() => new NoteBuilder();
    }
}
