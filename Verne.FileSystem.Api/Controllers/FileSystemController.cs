using Microsoft.AspNetCore.Mvc;
using Verne.FileSystem.Core.Dtos;
using Verne.FileSystem.Core.Entities;
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
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var createdNodeDto = await _fileSystemService.CreateNodeAsync(createNodeDto);

        if (createdNodeDto == null)
        {
            return BadRequest("Creation failed.");
        }
        return CreatedAtAction(nameof(GetNodeById), new { id = createdNodeDto.Id }, createdNodeDto);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNode(Guid id)
    {
        var success = await _fileSystemService.DeleteNodeAsync(id);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }
    
    [HttpGet("{id}")]
    public IActionResult GetNodeById(Guid id)
    {
        return Ok(new { Id = id, Message = "Not implemented yet" });
    }

    [HttpGet("search/parent")]
    public async Task<IActionResult> SearchInParent([FromQuery] Guid parentId, String name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return BadRequest("Name is empty");
        }
        var result = await _fileSystemService.SearchInParentAsync(parentId,name);
        return Ok(result);
    }

    [HttpGet("search/all")]
    public async Task<IActionResult> SearchAll([FromQuery] String name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return BadRequest("Name is empty");
        }
        var result = await _fileSystemService.SearchAllFilesAsync(name);
        return Ok(result);
    }
    
    [HttpGet("search")]
    public async Task<IActionResult> SearchFiles([FromQuery] string query)
    {
        if (string.IsNullOrEmpty(query))
        {
            return Ok(new List<NodeDto>());
        }
        var result = await _fileSystemService.SearchFilesAsync(query);
        return Ok(result);
    }
}