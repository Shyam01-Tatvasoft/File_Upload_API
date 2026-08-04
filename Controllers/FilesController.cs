using Microsoft.AspNetCore.Mvc;
using Backend.DTOs;
using Backend.Interfaces;
using Backend.DTOs;
using Microsoft.AspNetCore.Mvc;

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

        [HttpPost("upload-multiple")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadMultiple([FromForm] FileUploadMultipleDto dto)
        {
            var results = await _fileService.UploadMultipleFilesAsync(dto);
            return Created(string.Empty, results);
        }



        [HttpGet]
        public async Task<IActionResult> GetFiles([FromQuery] FileQueryParamsDto queryParams)
        {
            var result = await _fileService.GetFilesAsync(queryParams);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFileById(int id)
        {
            var file = await _fileService.GetByIdAsync(id);
            if (file == null)
                return NotFound(new { message = $"File with id {id} not found." });

            return Ok(file);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFile(int id)
        {
            var deleted = await _fileService.DeleteFileAsync(id);

            if (!deleted)
                return NotFound(new { message = $"File with id {id} not found." });

            return NoContent();
        }

        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateFile(int id, [FromForm] FileUpdateDto dto)
        {
            var result = await _fileService.UpdateFileAsync(id, dto);

            if (result == null)
                return NotFound(new { message = $"File with id {id} not found." });

            return Ok(result);
        }
    }
}