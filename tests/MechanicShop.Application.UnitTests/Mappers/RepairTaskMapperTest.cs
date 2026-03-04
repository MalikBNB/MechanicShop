using MechanicShop.Application.Features.RepairTasks.Mappers;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Tests.Common.RepaireTasks;
using Xunit;

namespace MechanicShop.Application.UnitTests.Mappers;

public class RepairTaskMapperTest
{
    [Fact]
    public void ToDto_ShouldMapCorrectly()
    {
        var part = PartFactory.CreatePart(cost: 100, quantity: 2).Value;

        var repairTask = RepairTaskFactory.CreateRepairTask(
            laborCost: 150m,
            parts: [part]).Value;

        var dto = repairTask.ToDto();

        Assert.Equal(repairTask.Id, dto.RepairTaskId);
        Assert.Equal(repairTask.Name, dto.Name);
        Assert.Equal(repairTask.LaborCost, dto.LaborCost);
        Assert.Equal(repairTask.EstimatedDurationInMins, dto.EstimatedDurationInMins);
        Assert.Equal(repairTask.TotalCost, dto.TotalCost);

        Assert.Single(dto.Parts);
    }

    [Fact]
    public void ToDtos_ShouldMapListCorrectly()
    {
        var part = PartFactory.CreatePart(cost: 100, quantity: 2).Value;

        var repairTask = RepairTaskFactory.CreateRepairTask(
            laborCost: 150m,
            parts: [part]).Value;

        var repairTasks = new List<RepairTask> { repairTask };

        var dtos = repairTasks.ToDtos();

        Assert.Single(dtos);
        var dto = dtos[0];

        Assert.Equal(repairTask.Id, dto.RepairTaskId);
        Assert.Equal(repairTask.Name, dto.Name);
        Assert.Equal(repairTask.LaborCost, dto.LaborCost);
        Assert.Equal(repairTask.EstimatedDurationInMins, dto.EstimatedDurationInMins);
        Assert.Equal(repairTask.TotalCost, dto.TotalCost);
        Assert.Single(dto.Parts);
    }
}