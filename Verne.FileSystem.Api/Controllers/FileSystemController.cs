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

    public FileSystemController(IFileSystemService fileSystemService)
    {
        _fileSystemService = fileSystemService;
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
        // Ovu logiku ćemo implementirati kasnije
        return Ok(new { Id = id, Message = "Not implemented yet" });
    }
}