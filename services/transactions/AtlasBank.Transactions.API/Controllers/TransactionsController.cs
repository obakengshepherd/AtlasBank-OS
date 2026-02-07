using AtlasBank.Transactions.API.Application.Commands;
using AtlasBank.Transactions.API.Application.Queries;
using AtlasBank.Core.Application.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AtlasBank.Transactions.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITenantContext _tenantContext;

    public TransactionsController(IMediator mediator, ITenantContext tenantContext)
    {
        _mediator = mediator;
        _tenantContext = tenantContext;
    }

    /// <summary>
    /// Create a transfer between two accounts
    /// </summary>
    [HttpPost("transfer")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTransfer([FromBody] CreateTransferRequest request)
    {
        var command = new TransferCommand(
            _tenantContext.TenantId,
            request.SourceAccountId,
            request.DestinationAccountId,
            request.Amount,
            request.Description
        );

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(ApiResponse.ErrorResponse(result.Error, traceId: HttpContext.TraceIdentifier));

        return CreatedAtAction(nameof(GetTransactionById), new { id = result.Value },
            ApiResponse<Guid>.SuccessResponse(result.Value, "Transfer created successfully"));
    }

    /// <summary>
    /// Get transaction by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<TransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTransactionById(Guid id)
    {
        var query = new GetTransactionByIdQuery(id);
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
            return NotFound(ApiResponse.ErrorResponse(result.Error, traceId: HttpContext.TraceIdentifier));

        return Ok(ApiResponse<TransactionDto>.SuccessResponse(result.Value));
    }

    /// <summary>
    /// Get all transactions with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<TransactionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllTransactions([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetAllTransactionsQuery(_tenantContext.TenantId, pageNumber, pageSize);
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
            return BadRequest(ApiResponse.ErrorResponse(result.Error, traceId: HttpContext.TraceIdentifier));

        return Ok(ApiResponse<PagedResult<TransactionDto>>.SuccessResponse(result.Value));
    }

    /// <summary>
    /// Get transactions for a specific account
    /// </summary>
    [HttpGet("account/{accountId}")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<TransactionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransactionsByAccount(string accountId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetTransactionsByAccountQuery(accountId, _tenantContext.TenantId, pageNumber, pageSize);
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
            return BadRequest(ApiResponse.ErrorResponse(result.Error, traceId: HttpContext.TraceIdentifier));

        return Ok(ApiResponse<PagedResult<TransactionDto>>.SuccessResponse(result.Value));
    }

    /// <summary>
    /// Process a pending transaction
    /// </summary>
    [HttpPost("{id}/process")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessTransaction(Guid id)
    {
        var command = new ProcessTransactionCommand(id);
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(ApiResponse.ErrorResponse(result.Error, traceId: HttpContext.TraceIdentifier));

        return Ok(ApiResponse<Guid>.SuccessResponse(result.Value, "Transaction processing started"));
    }

    /// <summary>
    /// Complete a processing transaction
    /// </summary>
    [HttpPost("{id}/complete")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompleteTransaction(Guid id)
    {
        var command = new CompleteTransactionCommand(id);
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(ApiResponse.ErrorResponse(result.Error, traceId: HttpContext.TraceIdentifier));

        return Ok(ApiResponse<Guid>.SuccessResponse(result.Value, "Transaction completed"));
    }

    /// <summary>
    /// Fail a transaction
    /// </summary>
    [HttpPost("{id}/fail")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FailTransaction(Guid id, [FromBody] FailTransactionRequest request)
    {
        var command = new FailTransactionCommand(id, request.Reason);
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(ApiResponse.ErrorResponse(result.Error, traceId: HttpContext.TraceIdentifier));

        return Ok(ApiResponse<Guid>.SuccessResponse(result.Value, "Transaction marked as failed"));
    }
}

// =============== Request DTOs ===============
public record CreateTransferRequest(
    string SourceAccountId,
    string DestinationAccountId,
    decimal Amount,
    string Description
);

public record FailTransactionRequest(string Reason);
