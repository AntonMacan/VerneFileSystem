using Verne.FileSystem.Core.Dtos;
using Verne.FileSystem.Core.Entities;

namespace Verne.FileSystem.Core.Interfaces;

public interface IFileSystemService
{
    Task<NodeDto> CreateNodeAsync(CreateNodeDto createNodeDto);
    
    Task<bool> DeleteNodeAsync(Guid id);
}