using AtlasBank.Tenancy.API.Application.Commands;
using AtlasBank.Tenancy.API.Persistence;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AtlasBank.Tenancy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TenantsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly TenancyDbContext _context;

    public TenantsController(IMediator mediator, TenancyDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTenant([FromBody] CreateTenantCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok(new { tenantId = result.Value });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTenant(Guid id)
    {
        var tenant = await _context.Tenants.FindAsync(id);
        
        if (tenant == null)
            return NotFound();

        return Ok(tenant);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTenants()
    {
        var tenants = await _context.Tenants.ToListAsync();
        return Ok(tenants);
    }
}
