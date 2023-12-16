namespace ShittyOne.Entities
{
    public class File
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public string SubDir { get; set; }
        public string ContentType { get; set; }
        public DateTime CreationDate { get; private set; }
        public File()
        {
            CreationDate = DateTime.UtcNow;
        }
    }
}
