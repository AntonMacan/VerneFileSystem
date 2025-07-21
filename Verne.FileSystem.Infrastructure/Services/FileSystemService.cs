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
            IsFolder = createNodeDto.IsFolder,
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

    public async Task<IEnumerable<NodeDto>> SearchInParentAsync(Guid parentId, string name)
    {
        var allDescendants = new List<FileSystemNode>();
        var foldersToVisit = new Queue<Guid>();
        
        // We can't start with the parent itself, so we get its direct children first.
        var directChildren = await _context.Nodes.Where(n => n.ParentId == parentId).ToListAsync();

            foreach (var child in directChildren)
        {
            allDescendants.Add(child);
            if (child.IsFolder)
            {
                foldersToVisit.Enqueue(child.Id);
            }
        }

        while (foldersToVisit.Count > 0)
        {
            var currentParentId = foldersToVisit.Dequeue();
            var grandchildren = await _context.Nodes.Where(n => n.ParentId == currentParentId).ToListAsync();

            foreach (var grandchild in grandchildren)
            {
                allDescendants.Add(grandchild);
                if (grandchild.IsFolder)
                {
                    foldersToVisit.Enqueue(grandchild.Id);
                }
            }
        }
        
        var matchingNodes = allDescendants
            .Where(n => n.Name == name)
            .Select(n => new NodeDto 
            {
                Id = n.Id,
                Name = n.Name,
                IsFolder = n.IsFolder,
                ParentId = n.ParentId
            })
            .ToList();

        return matchingNodes;
}
    public async Task<IEnumerable<NodeDto>> SearchAllFilesAsync(string name)
    {
        var files = await _context.Nodes
            .Where(n => n.Name == name && !n.IsFolder)
            .Select(n => new NodeDto
            {
                Id = n.Id,
                Name = n.Name,
                IsFolder = n.IsFolder,
                ParentId = n.ParentId
            })
            .ToListAsync();
        
        return files;
    }

    public async Task<IEnumerable<NodeDto>> SearchFilesAsync(string query)
    {
        var files = await _context.Nodes
            .Where(n => !n.IsFolder && n.Name.StartsWith(query))
            .OrderBy(n => n.Name)
            .Take(10)
            .Select(n => new NodeDto
            {
                Id = n.Id,
                Name = n.Name,
                IsFolder = n.IsFolder,
                ParentId = n.ParentId
            })
            .ToListAsync();
        
        return files;
    }

    public async Task<IEnumerable<NodeDto>?> GetChildrenAsync(Guid parentId)
    {
        var parentsExists = await _context.Nodes.AnyAsync(n => n.ParentId == parentId && n.IsFolder);
        if (!parentsExists)
        {
            return null;
        }
        
        var children = await _context.Nodes
            .Where(n => n.ParentId == parentId)
            .Select(n => new NodeDto
            {
                Id = n.Id,
                Name = n.Name,
                IsFolder = n.IsFolder,
                ParentId = n.ParentId
            })
            .ToListAsync();
        
        return children;
    }

    public async Task<NodeDto?> GetNodeAsync(Guid id)
    {
        var node = await _context.Nodes
            .Where(n => n.Id == id)
            .Select(n => new NodeDto
        {
            Id = n.Id,
            IsFolder = n.IsFolder,
            ParentId = n.ParentId,
            Name = n.Name
        }).FirstOrDefaultAsync();
        
        if (node == null) return null;
        
        return node;
    }
}