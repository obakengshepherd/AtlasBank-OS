using AtlasBank.Products.API.Application.Commands;
using AtlasBank.Products.API.Persistence;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AtlasBank.Products.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ProductsDbContext _context;

    public ProductsController(IMediator mediator, ProductsDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return CreatedAtAction(nameof(GetProduct), new { id = result.Value }, result.Value);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        
        if (product == null)
            return NotFound();

        return Ok(product);
    }

    [HttpGet("tenant/{tenantId}")]
    public async Task<IActionResult> GetProductsByTenant(string tenantId)
    {
        var products = await _context.Products
            .Where(p => p.TenantId == tenantId && p.IsActive)
            .ToListAsync();

        return Ok(products);
    }

    [HttpPost("loans")]
    public async Task<IActionResult> CreateLoan([FromBody] CreateLoanCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok(new { loanNumber = result.Value });
    }
}
