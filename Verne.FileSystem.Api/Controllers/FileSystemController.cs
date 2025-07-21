using Microsoft.AspNetCore.Mvc;
using Verne.FileSystem.Core.Dtos;
using Verne.FileSystem.Core.Interfaces;

namespace Verne.FileSystem.Api.Controllers;

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