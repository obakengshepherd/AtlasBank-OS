using AtlasBank.Compliance.API.Application.Commands;
using AtlasBank.Compliance.API.Persistence;
using AtlasBank.Core.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AtlasBank.Compliance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ComplianceController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ComplianceDbContext _context;

    public ComplianceController(IMediator mediator, ComplianceDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    [HttpPost("checks")]
    public async Task<IActionResult> CreateCheck([FromBody] CreateComplianceCheckCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok(new { checkId = result.Value });
    }

    [HttpPost("checks/{id}/approve")]
    public async Task<IActionResult> ApproveCheck(Guid id, [FromBody] ApproveComplianceCheckCommand command)
    {
        var result = await _mediator.Send(command with { CheckId = id });
        
        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok();
    }

    [HttpGet("checks/entity/{entityId}")]
    public async Task<IActionResult> GetChecksByEntity(string entityId)
    {
        var checks = await _context.ComplianceChecks
            .Where(c => c.EntityId == entityId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return Ok(checks);
    }

    [HttpGet("checks/pending")]
    public async Task<IActionResult> GetPendingChecks([FromQuery] string tenantId)
    {
        var checks = await _context.ComplianceChecks
            .Where(c => c.TenantId == tenantId && c.Status == ComplianceStatus.Pending)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        return Ok(checks);
    }
}
