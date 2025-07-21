using Microsoft.EntityFrameworkCore;
using Verne.FileSystem.Core.Dtos;
using Verne.FileSystem.Core.Entities;
using Verne.FileSystem.Infrastructure.Persistence;
using Verne.FileSystem.Infrastructure.Services;

namespace Verne.FileSystem.Tests;

public class FileSystemServiceTests
{
    private readonly DbContextOptions<FileSystemDbContext> _dbContextOptions;
    
    public FileSystemServiceTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<FileSystemDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }
    
    private async Task SeedDatabaseAsync(FileSystemDbContext context)
    {
        var rootFolder = new FileSystemNode { Id = Guid.NewGuid(), Name = "Root", IsFolder = true };
        var subFolder = new FileSystemNode { Id = Guid.NewGuid(), Name = "Sub", IsFolder = true, ParentId = rootFolder.Id };
        var fileInRoot = new FileSystemNode { Id = Guid.NewGuid(), Name = "file1.txt", IsFolder = false, ParentId = rootFolder.Id };
        var fileInSub = new FileSystemNode { Id = Guid.NewGuid(), Name = "file2.txt", IsFolder = false, ParentId = subFolder.Id };

        await context.Nodes.AddRangeAsync(rootFolder, subFolder, fileInRoot, fileInSub);
        await context.SaveChangesAsync();
    }

    #region CreateNodeAsync Tests

    [Fact]
    public async Task CreateNodeAsync_ShouldCreateRootFolder_WhenParentIdIsNull()
    {
        await using var context = new FileSystemDbContext(_dbContextOptions);
        var service = new FileSystemService(context);
        var createDto = new CreateNodeDto { Name = "RootFolder", IsFolder = true, ParentId = null };

        var result = await service.CreateNodeAsync(createDto);

        Assert.NotNull(result);
        Assert.Equal("RootFolder", result.Name);
        var nodeInDb = await context.Nodes.CountAsync();
        Assert.Equal(1, nodeInDb);
    }

    [Fact]
    public async Task CreateNodeAsync_ShouldCreateSubFolder_WhenParentExists()
    {
        await using var context = new FileSystemDbContext(_dbContextOptions);
        await SeedDatabaseAsync(context);
        var service = new FileSystemService(context);
        var rootFolder = await context.Nodes.FirstAsync(n => n.Name == "Root");
        var createDto = new CreateNodeDto { Name = "NewSub", IsFolder = true, ParentId = rootFolder.Id };

        var result = await service.CreateNodeAsync(createDto);
        
        Assert.NotNull(result);
        Assert.Equal(rootFolder.Id, result.ParentId);
    }

    [Fact]
    public async Task CreateNodeAsync_ShouldReturnNull_WhenParentFolderDoesNotExist()
    {
        await using var context = new FileSystemDbContext(_dbContextOptions);
        var service = new FileSystemService(context);
        var createDto = new CreateNodeDto { Name = "OrphanFile", IsFolder = false, ParentId = Guid.NewGuid() };
        
        var result = await service.CreateNodeAsync(createDto);
        
        Assert.Null(result);
    }
    
    [Fact]
    public async Task CreateNodeAsync_ShouldReturnNull_WhenParentIsAFile()
    {
        // Arrange
        await using var context = new FileSystemDbContext(_dbContextOptions);
        await SeedDatabaseAsync(context);
        var service = new FileSystemService(context);
        var fileInRoot = await context.Nodes.FirstAsync(n => n.Name == "file1.txt");
        var createDto = new CreateNodeDto { Name = "InvalidFile", IsFolder = false, ParentId = fileInRoot.Id };

        var result = await service.CreateNodeAsync(createDto);
        
        Assert.Null(result);
    }

    #endregion

    #region DeleteNodeAsync Tests

    [Fact]
    public async Task DeleteNodeAsync_ShouldReturnTrueAndRemoveNode_WhenNodeExists()
    {
        await using var context = new FileSystemDbContext(_dbContextOptions);
        await SeedDatabaseAsync(context);
        var service = new FileSystemService(context);
        var fileToDelete = await context.Nodes.FirstAsync(n => n.Name == "file1.txt");

        var result = await service.DeleteNodeAsync(fileToDelete.Id);

        Assert.True(result);
        var count = await context.Nodes.CountAsync(n => n.Id == fileToDelete.Id);
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task DeleteNodeAsync_ShouldReturnFalse_WhenNodeDoesNotExist()
    {
        await using var context = new FileSystemDbContext(_dbContextOptions);
        var service = new FileSystemService(context);
        
        var result = await service.DeleteNodeAsync(Guid.NewGuid());
        
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteNodeAsync_ShouldDeleteChildren_WhenDeletingFolder()
    {
        await using var context = new FileSystemDbContext(_dbContextOptions);
        await SeedDatabaseAsync(context);
        var service = new FileSystemService(context);
        var rootFolder = await context.Nodes.FirstAsync(n => n.Name == "Root");
        
        var result = await service.DeleteNodeAsync(rootFolder.Id);
        
        Assert.True(result);
        var totalNodes = await context.Nodes.CountAsync();
        Assert.Equal(0, totalNodes);
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task SearchInParentAsync_ShouldFindNodeInSubfolder()
    {
        await using var context = new FileSystemDbContext(_dbContextOptions);
        await SeedDatabaseAsync(context);
        var service = new FileSystemService(context);
        var rootFolder = await context.Nodes.FirstAsync(n => n.Name == "Root");
        
        var result = await service.SearchInParentAsync(rootFolder.Id, "file2.txt");
        
        Assert.Single(result);
        Assert.Equal("file2.txt", result.First().Name);
    }

    [Fact]
    public async Task SearchAllFilesAsync_ShouldFindFile()
    {
        await using var context = new FileSystemDbContext(_dbContextOptions);
        await SeedDatabaseAsync(context);
        var service = new FileSystemService(context);
        
        var result = await service.SearchAllFilesAsync("file2.txt");
        
        Assert.Single(result);
    }
    
    [Fact]
    public async Task SearchFilesAsync_Autocomplete_ShouldReturnMatchingFiles()
    {
        await using var context = new FileSystemDbContext(_dbContextOptions);
        await SeedDatabaseAsync(context);
        var service = new FileSystemService(context);
        
        var result = await service.SearchFilesAsync("file2");
        
        Assert.Single(result);
        Assert.Equal("file2.txt", result.First().Name);
    }

    #endregion
    
    #region GetChildrenAsync Tests
    
    [Fact]
    public async Task GetChildrenAsync_ShouldReturnDirectChildrenOfFolder()
    {
        await using var context = new FileSystemDbContext(_dbContextOptions);
        await SeedDatabaseAsync(context);
        var service = new FileSystemService(context);
        var rootFolder = await context.Nodes.FirstAsync(n => n.Name == "Root");
        
        var result = await service.GetChildrenAsync(rootFolder.Id);
        
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetChildrenAsync_ShouldReturnNullForNonExistentParent()
    {
        await using var context = new FileSystemDbContext(_dbContextOptions);
        var service = new FileSystemService(context);
        
        var result = await service.GetChildrenAsync(Guid.NewGuid());
        
        Assert.Null(result);
    }
    
    #endregion
}