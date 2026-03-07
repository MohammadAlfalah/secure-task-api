 using System.ComponentModel.DataAnnotations;

namespace SecureTaskApi.Dtos;

public class TaskCreateDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }
}

