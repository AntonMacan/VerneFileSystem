using Verne.FileSystem.Core.Dtos;
using Verne.FileSystem.Core.Entities;

namespace Verne.FileSystem.Core.Interfaces;

/// <summary>
/// Defines the contract for file system operations.
/// </summary>
public interface IFileSystemService
{
    /// <summary>
    /// Creates a new file or folder.
    /// </summary>
    /// <param name="createNodeDto">The data transfer object containing the details for the new node.</param>
    /// <returns>A DTO of the newly created node, or null if creation fails (e.g., parent not found).</returns>
    Task<NodeDto?> CreateNodeAsync(CreateNodeDto createNodeDto);

    /// <summary>
    /// Deletes a node (file or folder) by its ID. If a folder is deleted, all its contents are also deleted.
    /// </summary>
    /// <param name="id">The unique identifier of the node to delete.</param>
    /// <returns>True if the deletion was successful, otherwise false.</returns>
    Task<bool> DeleteNodeAsync(Guid id);

    /// <summary>
    /// Recursively searches for nodes with an exact name match within a given parent folder and all its subfolders.
    /// </summary>
    /// <param name="parentId">The ID of the folder to start the search from.</param>
    /// <param name="name">The exact name of the node to search for.</param>
    /// <returns>A collection of matching nodes.</returns>
    Task<IEnumerable<NodeDto>> SearchInParentAsync(Guid parentId, string name);

    /// <summary>
    /// Searches for all files with an exact name match across the entire file system.
    /// </summary>
    /// <param name="name">The exact name of the file to search for.</param>
    /// <returns>A collection of matching files.</returns>
    Task<IEnumerable<NodeDto>> SearchAllFilesAsync(string name);

    /// <summary>
    /// Searches for the top 10 files whose names start with the given query string, for autocomplete functionality.
    /// </summary>
    /// <param name="query">The string to search for at the beginning of file names.</param>
    /// <returns>A collection of up to 10 matching files.</returns>
    Task<IEnumerable<NodeDto>> SearchFilesAsync(string query);

    /// <summary>
    /// Gets a list of the direct children (files and folders) of a specific folder.
    /// </summary>
    /// <param name="parentId">The ID of the parent folder.</param>
    /// <returns>A collection of child nodes, or null if the parent folder is not found.</returns>
    Task<IEnumerable<NodeDto>?> GetChildrenAsync(Guid parentId);

    /// <summary>
    /// Gets a single node by its unique identifier.
    /// </summary>
    /// <param name="id">The ID of the node to retrieve.</param>
    /// <returns>A DTO of the found node, or null if no node with the given ID exists.</returns>
    Task<NodeDto?> GetNodeAsync(Guid id);
}
