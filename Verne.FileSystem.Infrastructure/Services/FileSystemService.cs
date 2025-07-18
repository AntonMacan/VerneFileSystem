using Microsoft.EntityFrameworkCore;
using Verne.FileSystem.Core.Dtos;
using Verne.FileSystem.Core.Entities;
using Verne.FileSystem.Core.Interfaces;
using Verne.FileSystem.Infrastructure.Persistence;

namespace Verne.FileSystem.Infrastructure.Services;

public class FileSystemService : IFileSystemService
{
    private readonly FileSystemDbContext _context;

    public FileSystemService(FileSystemDbContext context)
    {
        _context = context;
    }
    
    public async Task<NodeDto> CreateNodeAsync(CreateNodeDto createNodeDto)
    {
        if (createNodeDto.ParentId.HasValue)
        {
            var parentFolder = await _context.Nodes
                .FirstOrDefaultAsync(n => n.Id == createNodeDto.ParentId.Value);
            Console.WriteLine(parentFolder);
            if (parentFolder == null || !parentFolder.IsFolder)
            {
                return null;
            }
        }
        var node = new FileSystemNode
        {
            Name = createNodeDto.Name,
            IsFolder = createNodeDto.isFolder,
            ParentId = createNodeDto.ParentId.HasValue ?  createNodeDto.ParentId.Value : null
        };

        try
        {
            await _context.Nodes.AddAsync(node);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null;
        }

        var nodeDto = new NodeDto
        {
            Id = node.Id,
            Name = node.Name,
            IsFolder = node.IsFolder,
            ParentId = node.ParentId
        };
        return nodeDto;
    }

    public async Task<bool> DeleteNodeAsync(Guid id)
    {
        var node = await _context.Nodes.FindAsync(id);

        if (node == null) return false;

        try
        {
            _context.Nodes.Remove(node);
            await _context.SaveChangesAsync();
        }
        catch (Exception)
        {
            return false;
        }

        return true;


    }
}