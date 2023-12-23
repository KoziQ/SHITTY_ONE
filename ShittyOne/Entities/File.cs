namespace ShittyOne.Entities;

public class File
{
    public File()
    {
        CreationDate = DateTime.UtcNow;
    }

    public Guid Id { get; set; }
    public string FileName { get; set; }
    public string SubDir { get; set; }
    public string ContentType { get; set; }
    public DateTime CreationDate { get; private set; }
}