using Verne.FileSystem.Core.Dtos;
using Verne.FileSystem.Core.Entities;

namespace Verne.FileSystem.Core.Interfaces;

public interface IFileSystemService
{
    Task<NodeDto> CreateNodeAsync(CreateNodeDto createNodeDto);
    Task<bool> DeleteNodeAsync(Guid id);
    Task<IEnumerable<NodeDto>> SearchInParentAsync(Guid parentId, string name);
    Task<IEnumerable<NodeDto>> SearchAllFilesAsync(string name);
    Task<IEnumerable<NodeDto>> SearchFilesAsync(string query);
    Task<IEnumerable<NodeDto>?> GetChildrenAsync(Guid parentId);
}