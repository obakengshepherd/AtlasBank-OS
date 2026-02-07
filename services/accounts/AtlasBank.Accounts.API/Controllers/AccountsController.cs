using AtlasBank.Accounts.API.Application.Commands;
using AtlasBank.Accounts.API.Application.Queries;
using AtlasBank.Core.Application.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AtlasBank.Accounts.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITenantContext _tenantContext;

    public AccountsController(IMediator mediator, ITenantContext tenantContext)
    {
        _mediator = mediator;
        _tenantContext = tenantContext;
    }

    /// <summary>
    /// Create a new account
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(ApiResponse.ErrorResponse(result.Error, traceId: HttpContext.TraceIdentifier));

        return CreatedAtAction(nameof(GetAccountById), new { id = result.Value }, 
            ApiResponse<Guid>.SuccessResponse(result.Value, "Account created successfully"));
    }

    /// <summary>
    /// Get account by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<AccountDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAccountById(Guid id)
    {
        var query = new GetAccountByIdQuery(id);
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
            return NotFound(ApiResponse.ErrorResponse(result.Error, traceId: HttpContext.TraceIdentifier));

        return Ok(ApiResponse<AccountDto>.SuccessResponse(result.Value));
    }

    /// <summary>
    /// Get all accounts with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AccountDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAccounts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetAllAccountsQuery(_tenantContext.TenantId, pageNumber, pageSize);
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
            return BadRequest(ApiResponse.ErrorResponse(result.Error, traceId: HttpContext.TraceIdentifier));

        return Ok(ApiResponse<PagedResult<AccountDto>>.SuccessResponse(result.Value));
    }

    /// <summary>
    /// Get accounts by customer
    /// </summary>
    [HttpGet("customer/{customerId}")]
    [ProducesResponseType(typeof(ApiResponse<List<AccountDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccountsByCustomer(string customerId)
    {
        var query = new GetAccountsByCustomerQuery(customerId, _tenantContext.TenantId);
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
            return BadRequest(ApiResponse.ErrorResponse(result.Error, traceId: HttpContext.TraceIdentifier));

        return Ok(ApiResponse<List<AccountDto>>.SuccessResponse(result.Value));
    }

    /// <summary>
    /// Activate a pending account
    /// </summary>
    [HttpPost("{id}/activate")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateAccount(Guid id)
    {
        var command = new ActivateAccountCommand(id);
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(ApiResponse.ErrorResponse(result.Error, traceId: HttpContext.TraceIdentifier));

        return Ok(ApiResponse<Guid>.SuccessResponse(result.Value, "Account activated successfully"));
    }

    /// <summary>
    /// Deposit funds into account
    /// </summary>
    [HttpPost("{id}/deposit")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DepositFunds(Guid id, [FromBody] DepositRequest request)
    {
        var command = new DepositCommand(id, request.Amount);
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(ApiResponse.ErrorResponse(result.Error, traceId: HttpContext.TraceIdentifier));

        return Ok(ApiResponse<Guid>.SuccessResponse(result.Value, "Deposit successful"));
    }

    /// <summary>
    /// Withdraw funds from account
    /// </summary>
    [HttpPost("{id}/withdraw")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> WithdrawFunds(Guid id, [FromBody] WithdrawRequest request)
    {
        var command = new WithdrawCommand(id, request.Amount);
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(ApiResponse.ErrorResponse(result.Error, traceId: HttpContext.TraceIdentifier));

        return Ok(ApiResponse<Guid>.SuccessResponse(result.Value, "Withdrawal successful"));
    }

    /// <summary>
    /// Freeze an account
    /// </summary>
    [HttpPost("{id}/freeze")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FreezeAccount(Guid id, [FromBody] FreezeRequest request)
    {
        var command = new FreezeAccountCommand(id, request.Reason);
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(ApiResponse.ErrorResponse(result.Error, traceId: HttpContext.TraceIdentifier));

        return Ok(ApiResponse<Guid>.SuccessResponse(result.Value, "Account frozen successfully"));
    }

    /// <summary>
    /// Apply interest to account
    /// </summary>
    [HttpPost("{id}/apply-interest")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApplyInterest(Guid id)
    {
        var command = new ApplyInterestCommand(id);
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(ApiResponse.ErrorResponse(result.Error, traceId: HttpContext.TraceIdentifier));

        return Ok(ApiResponse<Guid>.SuccessResponse(result.Value, "Interest applied successfully"));
    }
}

// =============== Request DTOs ===============
public record DepositRequest(decimal Amount);
public record WithdrawRequest(decimal Amount);
public record FreezeRequest(string Reason);
