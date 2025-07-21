using Microsoft.AspNetCore.Mvc;
using Verne.FileSystem.Core.Dtos;
using Verne.FileSystem.Core.Interfaces;

namespace Verne.FileSystem.Api.Controllers;

/// <summary>
/// Manages file system nodes.
/// </summary>
[ApiController]
[Route("api/nodes")]
public class FileSystemController : ControllerBase
{
    private readonly IFileSystemService _fileSystemService;
    private readonly ILogger<FileSystemController> _logger; 

    public FileSystemController(IFileSystemService fileSystemService, ILogger<FileSystemController> logger)
    {
        _fileSystemService = fileSystemService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new file or folder.
    /// </summary>
    /// <param name="createNodeDto">The details of the node to create.</param>
    /// <returns>The newly created node.</returns>
    [HttpPost]
    public async Task<IActionResult> CreateNode([FromBody] CreateNodeDto createNodeDto)
    {
        _logger.LogInformation("Attempting to create a new node with name: {NodeName}", createNodeDto.Name);
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("CreateNode failed due to invalid model state for node name: {NodeName}", createNodeDto.Name);
            return BadRequest(ModelState);
        }

        var createdNodeDto = await _fileSystemService.CreateNodeAsync(createNodeDto);

        if (createdNodeDto == null)
        {
            _logger.LogWarning("Node creation failed for name {NodeName}. Parent folder might not exist.", createNodeDto.Name);
            return BadRequest("Creation failed.");
        }
        
        _logger.LogInformation("Successfully created node {NodeName} with ID {NodeId}", createdNodeDto.Name, createdNodeDto.Id);
        return CreatedAtAction(nameof(GetNodeById), new { id = createdNodeDto.Id }, createdNodeDto);
    }

    /// <summary>
    /// Deletes a node by its ID. Deleting a folder also deletes all its contents.
    /// </summary>
    /// <param name="id">The ID of the node to delete.</param>
    /// <returns>A 204 No Content response on success.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNode(Guid id)
    {
        _logger.LogInformation("Attempting to delete node with ID: {NodeId}", id);
        try
        {
            var success = await _fileSystemService.DeleteNodeAsync(id);

            if (!success)
            {
                _logger.LogWarning("Could not delete node with ID {NodeId} because it was not found.", id);
                return NotFound();
            }

            _logger.LogInformation("Successfully deleted node with ID: {NodeId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred while deleting node {NodeId}", id);
            return StatusCode(500, "An internal server error occurred.");
        }
    }
    
    /// <summary>
    /// Retrieves a single node by its unique identifier.
    /// </summary>
    /// <param name="id">The ID of the node to retrieve.</param>
    /// <returns>The requested node.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetNodeById(Guid id)
    {
        _logger.LogInformation("Getting node by ID: {NodeId}", id);

        var node = await _fileSystemService.GetNodeAsync(id);

        if (node == null)
        {
            _logger.LogWarning("Node with ID {NodeId} was not found.", id);
            return NotFound();
        }

        return Ok(node);
    }

    /// <summary>
    /// Recursively searches for a file with an exact name within a specific parent folder.
    /// </summary>
    /// <param name="parentId">The ID of the folder to start the search in.</param>
    /// <param name="name">The exact name of the file to find.</param>
    /// <returns>A list of matching nodes.</returns>
    [HttpGet("search/parent")]
    public async Task<IActionResult> SearchInParent([FromQuery] Guid parentId, [FromQuery] string name)
    {
        _logger.LogInformation("Searching for node with name {NodeName} in parent {ParentId}", name, parentId);
        if (string.IsNullOrEmpty(name))
        {
            return BadRequest("Name is empty");
        }
        var result = await _fileSystemService.SearchInParentAsync(parentId, name);
        _logger.LogInformation("Found {Count} nodes for name {NodeName} in parent {ParentId}", result.Count(), name, parentId);
        return Ok(result);
    }

    /// <summary>
    /// Searches for all files with an exact name across the entire system.
    /// </summary>
    /// <param name="name">The exact name of the file to find.</param>
    /// <returns>A list of matching files.</returns>
    [HttpGet("search/all")]
    public async Task<IActionResult> SearchAll([FromQuery] string name)
    {
        _logger.LogInformation("Searching for all files with exact name: {FileName}", name);
        if (string.IsNullOrEmpty(name))
        {
            return BadRequest("Name is empty");
        }
        var result = await _fileSystemService.SearchAllFilesAsync(name);
        _logger.LogInformation("Found {Count} files with name {FileName}", result.Count(), name);
        return Ok(result);
    }
    
    /// <summary>
    /// Gets the top 10 files that start with a given query, for autocomplete.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <returns>A list of up to 10 matching files.</returns>
    [HttpGet("search")]
    public async Task<IActionResult> SearchFiles([FromQuery] string query)
    {
        _logger.LogInformation("Performing autocomplete search for query: {Query}", query);
        if (string.IsNullOrEmpty(query))
        {
            return Ok(new List<NodeDto>());
        }
        var result = await _fileSystemService.SearchFilesAsync(query);
        _logger.LogInformation("Returning {Count} autocomplete results for query: {Query}", result.Count(), query);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves the direct children of a specific folder.
    /// </summary>
    /// <param name="parentId">The ID of the parent folder.</param>
    /// <returns>A list of child nodes.</returns>
    [HttpGet("{parentId}/children")]
    public async Task<IActionResult> GetChildren(Guid parentId)
    {
        _logger.LogInformation("Attempting to get children for parent ID: {ParentId}", parentId);
        var children = await _fileSystemService.GetChildrenAsync(parentId);

        if (children == null)
        {
            _logger.LogWarning("Could not get children for parent ID {ParentId} because it was not found.", parentId);
            return NotFound("Parent not found");
        }

        _logger.LogInformation("Successfully retrieved {Count} children for parent ID: {ParentId}", children.Count(), parentId);
        return Ok(children);
    }
}
