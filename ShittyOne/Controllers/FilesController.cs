using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShittyOne.Models;
using ShittyOne.Services;

namespace ShittyOne.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/{version:apiVersion}/files")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class FilesController(IFileService fileService, IMapper mapper)
    : Controller
{
    [RequestFormLimits(ValueLengthLimit = 502000, MultipartBodyLengthLimit = 5000000)]
    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm(Name = "file")] IFormFile? file)
    {
        if (file == null) return BadRequest();

        var result = await fileService.AddFile(file);
        if (result == null) return BadRequest();

        var response = mapper.Map<FileModel>(result);

        return Ok(response);
    }
}