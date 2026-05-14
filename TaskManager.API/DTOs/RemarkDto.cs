namespace TaskManager.API.DTOs;

public class RemarkDto
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AddRemarkRequest
{
    public string Text { get; set; } = string.Empty;
}
