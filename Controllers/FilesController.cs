using Microsoft.AspNetCore.Mvc;
using Backend.DTOs;
using Backend.Interfaces;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FilesController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] FileUploadDto dto)
        {
            var result = await _fileService.UploadFileAsync(dto);
            return CreatedAtAction(nameof(Upload), new { id = result.Id }, result);
        }
    }
}