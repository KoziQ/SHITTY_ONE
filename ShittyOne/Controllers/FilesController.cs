using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShittyOne.Data;
using ShittyOne.Models;
using ShittyOne.Services;

namespace ShittyOne.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/{version:apiVersion}/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class FilesController : Controller
{
    private readonly AppDbContext _dbContext;
    private readonly IFileService _fileService;
    private readonly IMapper _mapper;

    public FilesController(IFileService fileService, AppDbContext dbContext, IMapper mapper)
    {
        _fileService = fileService;
        _dbContext = dbContext;
        _mapper = mapper;
    }

    [RequestFormLimits(ValueLengthLimit = 502000, MultipartBodyLengthLimit = 5000000)]
    [HttpPost("Upload")]
    public async Task<IActionResult> Upload([FromForm(Name = "file")] IFormFile file)
    {
        if (file == null) return BadRequest();

        var result = await _fileService.AddFile(file);
        if (result == null) return BadRequest();

        var response = _mapper.Map<FileModel>(result);

        return Ok(response);
    }
}