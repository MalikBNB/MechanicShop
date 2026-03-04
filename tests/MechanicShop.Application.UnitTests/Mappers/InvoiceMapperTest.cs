using MechanicShop.Application.Features.Billing.Mappers;
using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Tests.Common.Billing;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.WorkOrders;
using Xunit;

namespace MechanicShop.Application.UnitTests.Mappers;

public class InvoiceMapperTest
{
    [Fact]
    public void ToDto_ShouldMapCorrectly()
    {
        var customer = CustomerFactory.CreateCustomer().Value;
        var vehicle = customer.Vehicles.First();
        var workOrder = WorkOrderFactory.CreateWorkOrder(vehicleId: vehicle.Id).Value;
        var lineItem = InvoiceLineItemFactory.CreateInvoiceLineItem().Value;

        var invoice = InvoiceFactory.CreateInvoice(workOrderId: workOrder.Id, items: [lineItem]).Value;

        var dto = invoice.ToDto();

        Assert.Equal(invoice.Id, dto.InvoiceId);
        Assert.Equal(invoice.WorkOrderId, dto.WorkOrderId);
        Assert.Equal(invoice.IssuedAtUtc, dto.IssuedAtUtc);
        Assert.Equal(invoice.Subtotal, dto.Subtotal);
        Assert.Equal(invoice.TaxAmount, dto.TaxAmount);
        Assert.Equal(invoice.DiscountAmount, dto.DiscountAmount);
        Assert.Equal(invoice.Total, dto.Total);
        Assert.Equal(invoice.Status.ToString(), dto.PaymentStatus);
        Assert.Single(dto.Items);

        Assert.NotNull(dto.Customer);
        Assert.Equal(customer.Id, dto.Customer.CustomerId);
        Assert.Equal(customer.Name, dto.Customer.Name);
        Assert.Equal(customer.Email, dto.Customer.Email);
        Assert.Equal(customer.PhoneNumber, dto.Customer.PhoneNumber);
        Assert.Equal(2, dto.Customer.Vehicles.Count);

        Assert.NotNull(dto.Vehicle);
        Assert.Equal(vehicle.Id, dto.Vehicle.VehicleId);
        Assert.Equal(vehicle.Make, dto.Vehicle.Make);
        Assert.Equal(vehicle.Model, dto.Vehicle.Model);
        Assert.Equal(vehicle.Year, dto.Vehicle.Year);
        Assert.Equal(vehicle.LicensePlate, dto.Vehicle.LicensePlate);
    }

    [Fact]
    public void ToDtos_ShouldMapListCorrectly()
    {
        var customer = CustomerFactory.CreateCustomer().Value;
        var vehicle = customer.Vehicles.First();
        var workOrder = WorkOrderFactory.CreateWorkOrder(vehicleId: vehicle.Id).Value;
        var lineItem = InvoiceLineItemFactory.CreateInvoiceLineItem().Value;

        var invoice = InvoiceFactory.CreateInvoice(workOrderId: workOrder.Id, items: [lineItem]).Value;
        var invoices = new List<Invoice> { invoice };

        var dtos = invoices.ToDtos();

        Assert.Single(dtos);
        var dto = dtos[0];

        Assert.Equal(invoice.Id, dto.InvoiceId);
        Assert.Equal(invoice.WorkOrderId, dto.WorkOrderId);
        Assert.Equal(invoice.IssuedAtUtc, dto.IssuedAtUtc);
        Assert.Equal(invoice.Subtotal, dto.Subtotal);
        Assert.Equal(invoice.TaxAmount, dto.TaxAmount);
        Assert.Equal(invoice.DiscountAmount, dto.DiscountAmount);
        Assert.Equal(invoice.Total, dto.Total);
        Assert.Equal(invoice.Status.ToString(), dto.PaymentStatus);
        Assert.Single(dto.Items);

        Assert.NotNull(dto.Customer);
        Assert.Equal(customer.Id, dto.Customer.CustomerId);
        Assert.Equal(customer.Name, dto.Customer.Name);
        Assert.Equal(customer.Email, dto.Customer.Email);
        Assert.Equal(customer.PhoneNumber, dto.Customer.PhoneNumber);
        Assert.Equal(2, dto.Customer.Vehicles.Count);

        Assert.NotNull(dto.Vehicle);
        Assert.Equal(vehicle.Id, dto.Vehicle.VehicleId);
        Assert.Equal(vehicle.Make, dto.Vehicle.Make);
        Assert.Equal(vehicle.Model, dto.Vehicle.Model);
        Assert.Equal(vehicle.Year, dto.Vehicle.Year);
        Assert.Equal(vehicle.LicensePlate, dto.Vehicle.LicensePlate);
    }
}