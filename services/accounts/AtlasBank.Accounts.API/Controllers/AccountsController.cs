using AtlasBank.Accounts.API.Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AtlasBank.Accounts.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AccountsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return CreatedAtAction(nameof(CreateAccount), new { id = result.Value }, result.Value);
    }
}
