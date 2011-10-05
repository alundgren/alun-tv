namespace AlunTv.Test
{
    public abstract class ShowInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool HasEnded { get; set; }
        public abstract string SourceId { get; }
    }
}