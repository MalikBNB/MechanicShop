using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Application.Features.Customers.Mappers;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Customers.Queries.GetCustomerById;

public sealed class GetCustomerByIdQueryHandler(
    IAppDbContext context,
    HybridCache cache,
    ILogger<GetCustomerByIdQueryHandler> logger) : IRequestHandler<GetCustomerByIdQuery, Result<CustomerDto>>
{
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly ILogger<GetCustomerByIdQueryHandler> _logger = logger;

    public async Task<Result<CustomerDto>> Handle(GetCustomerByIdQuery query, CancellationToken ct)
    {
        var customer = await _context.Customers.AsNoTracking()
                                               .Include(c => c.Vehicles)
                                               .FirstOrDefaultAsync(c => c.Id == query.CustomerId, ct);
        if (customer is null)
        {
            _logger.LogWarning("Customer with id {CustomerId} was not found", query.CustomerId);

            return Error.NotFound(
                code: "Customer_NotFound",
                description: $"Customer with id '{query.CustomerId}' was not found");
        }

        return customer.ToDto();
    }
}