using System.ComponentModel.DataAnnotations;

namespace Verne.FileSystem.Core.Dtos;

public class CreateNodeDto
{
    [Required]
    [StringLength(255)]
    public string Name { get; set; }
    
    public bool IsFolder { get; set; }
    
    public Guid? ParentId { get; set; }
}