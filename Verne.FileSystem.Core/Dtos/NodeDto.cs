namespace Verne.FileSystem.Core.Dtos;

public class NodeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsFolder { get; set; }
    public Guid? ParentId { get; set; }
}