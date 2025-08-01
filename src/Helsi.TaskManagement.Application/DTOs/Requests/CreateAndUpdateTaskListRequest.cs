using System.ComponentModel.DataAnnotations;

namespace Helsi.TaskManagement.Application.DTOs.Requests;

public class CreateAndUpdateTaskListRequest
{
    [Required]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 255 characters")]
    public string Name { get; set; } = string.Empty;
}