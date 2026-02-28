using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.RepaireTasks;

using Xunit;

namespace MechanicShop.Domain.UnitTests.WorkOrders;

public class WorkOrderTests
{
    [Fact]
    public void Create_ShouldReturnError_WhenIdIsEmpty()
    {
        var woResult = WorkOrder.Create(
                    id: Guid.Empty,
                    vehicleId: Guid.NewGuid(),
                    startAt: DateTimeOffset.UtcNow,
                    endAt: DateTimeOffset.UtcNow.AddHours(1),
                    laborId: Guid.NewGuid(),
                    spot: Spot.A,
                    repairTasks: [RepairTaskFactory.CreateRepairTask().Value]);

        Assert.False(woResult.IsSuccess);

        Assert.Equal(WorkOrderErrors.WorkOrderIdRequired.Code, woResult.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenVehicleIdIsEmpty()
    {
        var woResult = WorkOrder.Create(
            id: Guid.NewGuid(),
            vehicleId: Guid.Empty,
            startAt: DateTimeOffset.UtcNow,
            endAt: DateTimeOffset.UtcNow.AddHours(1),
            laborId: Guid.NewGuid(),
            spot: Spot.A,
            repairTasks: [RepairTaskFactory.CreateRepairTask().Value]);

        Assert.False(woResult.IsSuccess);

        Assert.Equal(WorkOrderErrors.VehicleIdRequired.Code, woResult.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenNoRepairTasks()
    {
        var woResult = WorkOrder.Create(
            id: Guid.NewGuid(),
            vehicleId: Guid.NewGuid(),
            startAt: DateTimeOffset.UtcNow,
            endAt: DateTimeOffset.UtcNow.AddHours(1),
            laborId: Guid.NewGuid(),
            spot: Spot.A,
            repairTasks: []);

        Assert.False(woResult.IsSuccess);

        Assert.Equal(WorkOrderErrors.RepairTasksRequired.Code, woResult.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenLaborIdIsEmpty()
    {
        var woResult = WorkOrder.Create(
            id: Guid.NewGuid(),
            vehicleId: Guid.NewGuid(),
            startAt: DateTimeOffset.UtcNow,
            endAt: DateTimeOffset.UtcNow.AddHours(1),
            laborId: Guid.Empty,
            spot: Spot.A,
            repairTasks: [RepairTaskFactory.CreateRepairTask().Value]);

        Assert.False(woResult.IsSuccess);

        Assert.Equal(WorkOrderErrors.LaborIdRequired.Code, woResult.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenTimingInvalid()
    {
        var woResult = WorkOrder.Create(
            id: Guid.NewGuid(),
            vehicleId: Guid.NewGuid(),
            startAt: DateTimeOffset.UtcNow.AddHours(1),
            endAt: DateTimeOffset.UtcNow,
            laborId: Guid.NewGuid(),
            spot: Spot.A,
            repairTasks: [RepairTaskFactory.CreateRepairTask().Value]);

        Assert.False(woResult.IsSuccess);

        Assert.Equal(WorkOrderErrors.InvalidTiming.Code, woResult.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenSpotInvalid()
    {
        const Spot invalidSpot = (Spot)999;

        var woResult = WorkOrder.Create(
            id: Guid.NewGuid(),
            vehicleId: Guid.NewGuid(),
            startAt: DateTimeOffset.UtcNow,
            endAt: DateTimeOffset.UtcNow.AddHours(1),
            laborId: Guid.NewGuid(),
            spot: invalidSpot,
            repairTasks: [RepairTaskFactory.CreateRepairTask().Value]);

        Assert.False(woResult.IsSuccess);

        Assert.Equal(WorkOrderErrors.SpotInvalid.Code, woResult.TopError.Code);
    }

    [Fact]
    public void AddRepairTask_ShouldReturnError_WhenNotEditable()
    {
        var woResult = WorkOrder.Create(
            id: Guid.NewGuid(),
            vehicleId: Guid.NewGuid(),
            startAt: DateTimeOffset.UtcNow,
            endAt: DateTimeOffset.UtcNow.AddHours(1),
            laborId: Guid.NewGuid(),
            spot: Spot.A,
            repairTasks: [RepairTaskFactory.CreateRepairTask().Value]).Value;

        woResult.UpdateState(WorkOrderState.InProgress);
        woResult.UpdateState(WorkOrderState.Completed);

        var result = woResult.AddRepairTask(RepairTaskFactory.CreateRepairTask().Value);

        Assert.False(result.IsSuccess);
        Assert.True(result.Errors.Count > 0);
    }

    [Fact]
    public void UpdateLabor_ShouldReturnError_WhenLaborIdEmpty()
    {
        var woResult = WorkOrder.Create(
            id: Guid.NewGuid(),
            vehicleId: Guid.NewGuid(),
            startAt: DateTimeOffset.UtcNow,
            endAt: DateTimeOffset.UtcNow.AddHours(1),
            laborId: Guid.NewGuid(),
            spot: Spot.A,
            repairTasks: [RepairTaskFactory.CreateRepairTask().Value]).Value;

        var result = woResult.UpdateLabor(Guid.Empty);

        Assert.False(result.IsSuccess);
        Assert.Equal(WorkOrderErrors.LaborIdEmpty(woResult.Id.ToString()).Code, result.TopError.Code);
    }

    [Fact]
    public void UpdateSpot_ShouldReturnError_WhenSpotInvalid()
    {
        var woResult = WorkOrder.Create(
            id: Guid.NewGuid(),
            vehicleId: Guid.NewGuid(),
            startAt: DateTimeOffset.UtcNow,
            endAt: DateTimeOffset.UtcNow.AddHours(1),
            laborId: Guid.NewGuid(),
            spot: Spot.A,
            repairTasks: [RepairTaskFactory.CreateRepairTask().Value]).Value;

        const Spot invalidSpot = (Spot)999;
        var result = woResult.UpdateSpot(invalidSpot);

        Assert.False(result.IsSuccess);
        Assert.Equal(WorkOrderErrors.SpotInvalid.Code, result.TopError.Code);
    }

    [Fact]
    public void UpdateTiming_ShouldReturnError_WhenInvalid()
    {
        var woResult = WorkOrder.Create(
            id: Guid.NewGuid(),
            vehicleId: Guid.NewGuid(),
            startAt: DateTimeOffset.UtcNow,
            endAt: DateTimeOffset.UtcNow.AddHours(1),
            laborId: Guid.NewGuid(),
            spot: Spot.A,
            repairTasks: [RepairTaskFactory.CreateRepairTask().Value]).Value;

        var result = woResult.UpdateTiming(DateTimeOffset.UtcNow.AddHours(2), DateTimeOffset.UtcNow);

        Assert.False(result.IsSuccess);
        Assert.Equal(WorkOrderErrors.InvalidTiming.Code, result.TopError.Code);
    }

    [Fact]
    public void UpdateState_ShouldReturnError_WhenTransitionInvalid()
    {
        var woResult = WorkOrder.Create(
            id: Guid.NewGuid(),
            vehicleId: Guid.NewGuid(),
            startAt: DateTimeOffset.UtcNow,
            endAt: DateTimeOffset.UtcNow.AddHours(1),
            laborId: Guid.NewGuid(),
            spot: Spot.A,
            repairTasks: [RepairTaskFactory.CreateRepairTask().Value]).Value;

        var result = woResult.UpdateState(WorkOrderState.Completed);

        Assert.False(result.IsSuccess);
        Assert.Equal(WorkOrderErrors.InvalidStateTransition(WorkOrderState.Scheduled, WorkOrderState.Completed).Code, result.TopError.Code);
    }

    [Fact]
    public void UpdateLabor_ShouldReturnSuccess_AndSetNewLaborId()
    {
        var woResult = WorkOrder.Create(
            id: Guid.NewGuid(),
            vehicleId: Guid.NewGuid(),
            startAt: DateTimeOffset.UtcNow,
            endAt: DateTimeOffset.UtcNow.AddHours(1),
            laborId: Guid.NewGuid(),
            spot: Spot.A,
            repairTasks: [RepairTaskFactory.CreateRepairTask().Value]).Value;

        var newLabor = Guid.NewGuid();
        var result = woResult.UpdateLabor(newLabor);

        Assert.True(result.IsSuccess);
        Assert.Equal(newLabor, woResult.LaborId);
    }

    [Fact]
    public void UpdateSpot_ShouldReturnSuccess_AndSetNewSpot()
    {
        var woResult = WorkOrder.Create(
            id: Guid.NewGuid(),
            vehicleId: Guid.NewGuid(),
            startAt: DateTimeOffset.UtcNow,
            endAt: DateTimeOffset.UtcNow.AddHours(1),
            laborId: Guid.NewGuid(),
            spot: Spot.A,
            repairTasks: [RepairTaskFactory.CreateRepairTask().Value]).Value;

        var result = woResult.UpdateSpot(Spot.B);

        Assert.True(result.IsSuccess);
        Assert.Equal(Spot.B, woResult.Spot);
    }

    [Fact]
    public void UpdateTiming_ShouldReturnSuccess_AndSetNewTiming()
    {
        var woResult = WorkOrder.Create(
            id: Guid.NewGuid(),
            vehicleId: Guid.NewGuid(),
            startAt: DateTimeOffset.UtcNow,
            endAt: DateTimeOffset.UtcNow.AddHours(1),
            laborId: Guid.NewGuid(),
            spot: Spot.A,
            repairTasks: [RepairTaskFactory.CreateRepairTask().Value]).Value;

        var newStart = woResult.StartAtUtc.AddHours(2);
        var newEnd = newStart.AddHours(1);
        var result = woResult.UpdateTiming(newStart, newEnd);

        Assert.True(result.IsSuccess);
        Assert.Equal(newStart, woResult.StartAtUtc);
        Assert.Equal(newEnd, woResult.EndAtUtc);
    }

    [Fact]
    public void UpdateState_ShouldReturnSuccess_AndSetStateToInProgress()
    {
        var woResult = WorkOrder.Create(
            id: Guid.NewGuid(),
            vehicleId: Guid.NewGuid(),
            startAt: DateTimeOffset.UtcNow,
            endAt: DateTimeOffset.UtcNow.AddHours(1),
            laborId: Guid.NewGuid(),
            spot: Spot.A,
            repairTasks: [RepairTaskFactory.CreateRepairTask().Value]).Value;

        var result = woResult.UpdateState(WorkOrderState.InProgress);

        Assert.True(result.IsSuccess);
        Assert.Equal(WorkOrderState.InProgress, woResult.State);
    }
}