using AtlasBank.Tenancy.API.Application.Commands;
using AtlasBank.Tenancy.API.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AtlasBank.Tenancy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CreateUserCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok(new { userId = result.Value });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthenticateUserQuery query)
    {
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
            return Unauthorized(new { error = result.Error });

        return Ok(result.Value);
    }
}
