using System.ComponentModel.DataAnnotations;

namespace Verne.FileSystem.Core.Entities;

public class FileSystemNode
{
    // Unique ID
    public Guid Id { get; set; }

    // File/Folder name
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    // Flag that defines if it's a folder or file
    public bool IsFolder { get; set; }

    // Parent ID - optional
    public Guid? ParentId { get; set; }

    // Navigation for the parents
    public FileSystemNode? Parent { get; set; }

    // Navigation for the children
    public ICollection<FileSystemNode> Children { get; set; } = new List<FileSystemNode>();
}