using System.ComponentModel.DataAnnotations;

namespace Helsi.TaskManagement.Application.DTOs.Requests;

public class AddConnectionRequest
{
    [Required]
    public string UserId { get; set; } = string.Empty;
}