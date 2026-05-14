namespace TaskManager.API.DTOs;

public class TagDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid ProcessId { get; set; }
}

public class CreateTagRequest
{
    public string Name { get; set; } = string.Empty;
}
