using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Tests.Common.Customers;
using Xunit;

namespace MechanicShop.Domain.UnitTests.Customers;

public class CustomerTests
{
    [Fact]
    public void CreateCustomer_ShouldSucceed_WithValidData()
    {
        var id = Guid.NewGuid();
        const string name = "Customer #1";
        const string phoneNumber = "5555555555";
        const string email = "customer01@localhost";
        List<Vehicle> vehicles = [VehicleFactory.CreateVehicle().Value];

        var result = CustomerFactory.CreateCustomer(id, name, phoneNumber, email, vehicles);

        Assert.True(result.IsSuccess);

        var customer = result.Value;
        Assert.IsType<Customer>(customer);
        Assert.NotNull(customer);
        Assert.Equal(id, customer.Id);
        Assert.Equal(name, customer.Name);
        Assert.Equal(phoneNumber, customer.PhoneNumber);
        Assert.Equal(email, customer.Email);
        Assert.Single(customer.Vehicles);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateCustomer_ShouldFail_WhenNameIsInvalid(string? invalidName)
    {
        var result = CustomerFactory.CreateCustomer(name: invalidName);

        Assert.True(result.IsError);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123")] // less than 7
    [InlineData("12345678910111213")] // greater than 15
    public void CreateCustomer_ShouldFail_WhenPhoneInvalid(string? invalidPhone)
    {
        var result = CustomerFactory.CreateCustomer(phoneNumber: invalidPhone);
        Assert.True(result.IsError);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateCustomer_ShouldFail_WhenEmailEmptyOrNull(string? value)
    {
        var result = CustomerFactory.CreateCustomer(email: value);

        Assert.True(result.IsError);
        Assert.Equal(CustomerErrors.EmailRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("abc1.@")]
    public void CreateCustomer_ShouldFail_WhenEmailInvalid(string? value)
    {
        var result = CustomerFactory.CreateCustomer(email: value);

        Assert.True(result.IsError);
    }

    [Fact]
    public void UpdateCustomer_ShouldSucceed_WithValidData()
    {
        var customer = CustomerFactory.CreateCustomer().Value;

        var result = customer.Update("Updated Name", "updated@email.com", "1234567890");

        Assert.True(result.IsSuccess);
        Assert.Equal(Result.Updated, result.Value);
    }

    [Fact]
    public void UpdateCustomer_ShouldFail_WhenInvalidName()
    {
        var customer = CustomerFactory.CreateCustomer().Value;

        var result = customer.Update(string.Empty, "newEmail@localhost", "123-1232");

        Assert.True(result.IsError);
    }

    [Fact]
    public void UpdateCustomer_ShouldFail_WhenInvalidPhoneNumber()
    {
        var customer = CustomerFactory.CreateCustomer().Value;

        var result = customer.Update("New name", "newEmail@localhost", string.Empty);

        Assert.True(result.IsError);
    }

    [Fact]
    public void UpdateCustomer_ShouldFail_WhenInvalidEmail()
    {
        var customer = CustomerFactory.CreateCustomer().Value;

        var result = customer.Update("New name", string.Empty, "123-1232");

        Assert.True(result.IsError);
    }

    [Fact]
    public void UpsertVehicles_ShouldAddNewVehiclesAndUpdateExisting()
    {
        var originalVehicle = VehicleFactory.CreateVehicle(make: "Ford").Value;
        var customer = CustomerFactory.CreateCustomer(vehicles: [originalVehicle]).Value;

        var updatedVehicle = VehicleFactory.CreateVehicle(id: originalVehicle.Id, make: "UpdatedFord").Value;
        var newVehicle = VehicleFactory.CreateVehicle(make: "NewBrand").Value;

        var result = customer.UpsertVehicles([updatedVehicle, newVehicle]);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, customer.Vehicles.Count());

        Assert.Equal(Result.Updated, result.Value);
        Assert.Contains(customer.Vehicles, v => v.Id == updatedVehicle.Id && v.Make == "UpdatedFord");
        Assert.Contains(customer.Vehicles, v => v.Id == newVehicle.Id && v.Make == "NewBrand");
    }

    [Fact]
    public void UpsertVehicles_ShouldRemoveVehiclesNotInIncomingList()
    {
        var existing1 = VehicleFactory.CreateVehicle().Value;
        var existing2 = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(vehicles: [existing1, existing2]).Value;

        var incoming = VehicleFactory.CreateVehicle(id: existing2.Id).Value;

        var result = customer.UpsertVehicles([incoming]);

        Assert.Equal(Result.Updated, result.Value);
        Assert.True(result.IsSuccess);
        Assert.Single(customer.Vehicles);
        Assert.Equal(existing2.Id, customer.Vehicles.Single().Id);
    }
}