namespace TaskManager.API.DTOs;

public class ProcessDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class CreateProcessRequest
{
    public string Name { get; set; } = string.Empty;
}

public class AddUserToProcessRequest
{
    public Guid UserId { get; set; }
    public string Role { get; set; } = "Member";
}
