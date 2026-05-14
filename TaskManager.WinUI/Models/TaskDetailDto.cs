using System.Collections.Generic;

namespace TaskManager.WinUI.Models;

public class TaskDetailDto : TaskItemDto
{
    public List<RemarkDto> Remarks { get; set; } = new();
}
