using MechanicShop.Application.Features.Customers.Mappers;
using MechanicShop.Domain.Customers;
using MechanicShop.Tests.Common.Customers;
using Xunit;

namespace MechanicShop.Application.UnitTests.Mappers;

public class CustomerMapperTest
{
    [Fact]
    public void ToDto_ShouldMapCorrectly()
    {
        var vehicle1 = VehicleFactory.CreateVehicle().Value;
        var vehicle2 = VehicleFactory.CreateVehicle().Value;

        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle1, vehicle2]).Value;

        var dto = customer.ToDto();

        Assert.Equal(customer.Id, dto.CustomerId);
        Assert.Equal(customer.Name, dto.Name);
        Assert.Equal(customer.Email, dto.Email);
        Assert.Equal(customer.PhoneNumber, dto.PhoneNumber);
        Assert.Equal(2, dto.Vehicles.Count);

        var v1 = dto.Vehicles[0];
        Assert.NotNull(v1);
        Assert.Equal(vehicle1.Id, v1.VehicleId);
        Assert.Equal(vehicle1.Make, v1.Make);
        Assert.Equal(vehicle1.Model, v1.Model);
        Assert.Equal(vehicle1.Year, v1.Year);
        Assert.Equal(vehicle1.LicensePlate, v1.LicensePlate);

        var v2 = dto.Vehicles[1];
        Assert.NotNull(v2);
        Assert.Equal(vehicle2.Id, v2.VehicleId);
        Assert.Equal(vehicle2.Make, v2.Make);
        Assert.Equal(vehicle2.Model, v2.Model);
        Assert.Equal(vehicle2.Year, v2.Year);
        Assert.Equal(vehicle2.LicensePlate, v2.LicensePlate);
    }

    [Fact]
    public void ToDtos_ShouldMapListCorrectly()
    {
        var vehicle1 = VehicleFactory.CreateVehicle().Value;
        var vehicle2 = VehicleFactory.CreateVehicle().Value;

        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle1, vehicle2]).Value;
        var customers = new List<Customer> { customer };

        var dtos = customers.ToDtos();

        Assert.Single(dtos);
        var dto = dtos[0];

        Assert.Equal(customer.Id, dto.CustomerId);
        Assert.Equal(customer.Name, dto.Name);
        Assert.Equal(customer.Email, dto.Email);
        Assert.Equal(customer.PhoneNumber, dto.PhoneNumber);
        Assert.Equal(2, dto.Vehicles.Count);

        var v1 = dto.Vehicles[0];
        Assert.NotNull(v1);
        Assert.Equal(vehicle1.Id, v1.VehicleId);
        Assert.Equal(vehicle1.Make, v1.Make);
        Assert.Equal(vehicle1.Model, v1.Model);
        Assert.Equal(vehicle1.Year, v1.Year);
        Assert.Equal(vehicle1.LicensePlate, v1.LicensePlate);

        var v2 = dto.Vehicles[1];
        Assert.NotNull(v2);
        Assert.Equal(vehicle2.Id, v2.VehicleId);
        Assert.Equal(vehicle2.Make, v2.Make);
        Assert.Equal(vehicle2.Model, v2.Model);
        Assert.Equal(vehicle2.Year, v2.Year);
        Assert.Equal(vehicle2.LicensePlate, v2.LicensePlate);
    }
}