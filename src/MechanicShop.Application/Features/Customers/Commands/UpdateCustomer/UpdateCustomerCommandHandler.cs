using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Customers.Commands.UpdateCustomer;

public sealed class UpdateCustomerCommandHandler(
    IAppDbContext context,
    HybridCache cache,
    ILogger<UpdateCustomerCommandHandler> logger) : IRequestHandler<UpdateCustomerCommand, Result<Updated>>
{
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly ILogger<UpdateCustomerCommandHandler> _logger = logger;

    public async Task<Result<Updated>> Handle(UpdateCustomerCommand command, CancellationToken ct)
    {
        var customer = await _context.Customers.Include(c => c.Vehicles)
                                               .FirstOrDefaultAsync(c => c.Id == command.CustomerId, ct);
        if (customer is null)
        {
            _logger.LogWarning("Customer {CustomerId} not found for update.", command.CustomerId);
            return ApplicationErrors.CustomerNotFound;
        }

        var validatedVehicles = new List<Vehicle>();
        foreach (var v in command.Vehicles)
        {
            var vehicleId = v.VehicleId ?? Guid.NewGuid();

            var vehicleResult = Vehicle.Create(vehicleId, v.Make, v.Model, v.Year, v.LicensePlate);
            if (vehicleResult.IsError)
            {
                return vehicleResult.Errors;
            }

            validatedVehicles.Add(vehicleResult.Value);
        }

        var updateCustomerResult = customer.Update(command.Name, command.Email, command.PhoneNumber);
        if (updateCustomerResult.IsError)
        {
            return updateCustomerResult.Errors;
        }

        var upsertVehiclesResult = customer.UpsertVehicles(validatedVehicles);
        if (upsertVehiclesResult.IsError)
        {
            return upsertVehiclesResult.Errors;
        }

        await _context.SaveChangesAsync(ct);
        await _cache.RemoveByTagAsync("customer", ct);

        return Result.Updated;
    }
}