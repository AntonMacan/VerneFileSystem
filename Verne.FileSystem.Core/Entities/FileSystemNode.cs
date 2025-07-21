using System.ComponentModel.DataAnnotations;

namespace Verne.FileSystem.Core.Entities;

public class FileSystemNode
{
    public Guid Id { get; set; }
    
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;
    
    public bool IsFolder { get; set; }
    
    public Guid? ParentId { get; set; }
    
    public FileSystemNode? Parent { get; set; }
    
    public ICollection<FileSystemNode> Children { get; set; } = new List<FileSystemNode>();
}