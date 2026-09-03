namespace LibraryMS.Domain.Entities;

public class Publisher
{
    public int PublisherId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Email   { get; set; }
    public string? PhoneNo  { get; set; }
    public string? Address  { get; set; }
    public string? Gender   { get; set; }
    public string? Country  { get; set; }
    public string? City     { get; set; }

    // Navigation
    public ICollection<Book> Books { get; set; } = new List<Book>();
}
