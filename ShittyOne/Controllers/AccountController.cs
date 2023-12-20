using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ShittyOne.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/{version:apiVersion}/Surveys/{surveyId}/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class AccountController : ControllerBase
{
}