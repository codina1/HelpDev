using HelpDev.SharedContracts.Observability;



namespace HelpDev.Observability.Tests;



public sealed class HealthStatusAggregatorTests

{

    private static readonly DateTime CheckedAt = DateTime.UtcNow;



    [Fact]

    public void Aggregate_empty_components_returns_healthy()

    {

        var result = HealthStatusAggregator.Aggregate([]);



        Assert.Equal(OperationalHealthStates.Healthy, result);

    }



    [Fact]

    public void Aggregate_all_healthy_returns_healthy()

    {

        var components = new[]

        {

            Component(OperationalHealthStates.Healthy, isCritical: false),

            Component(OperationalHealthStates.Healthy, isCritical: true),

        };



        var result = HealthStatusAggregator.Aggregate(components);



        Assert.Equal(OperationalHealthStates.Healthy, result);

    }



    [Fact]

    public void Aggregate_degraded_component_returns_degraded()

    {

        var components = new[]

        {

            Component(OperationalHealthStates.Healthy),

            Component(OperationalHealthStates.Degraded),

        };



        var result = HealthStatusAggregator.Aggregate(components);



        Assert.Equal(OperationalHealthStates.Degraded, result);

    }



    [Fact]

    public void Aggregate_non_critical_unhealthy_returns_degraded()

    {

        var components = new[]

        {

            Component(OperationalHealthStates.Healthy),

            Component(OperationalHealthStates.Unhealthy, isCritical: false),

        };



        var result = HealthStatusAggregator.Aggregate(components);



        Assert.Equal(OperationalHealthStates.Degraded, result);

    }



    [Fact]

    public void Aggregate_critical_unhealthy_returns_unhealthy()

    {

        var components = new[]

        {

            Component(OperationalHealthStates.Healthy),

            Component(OperationalHealthStates.Unhealthy, isCritical: true),

        };



        var result = HealthStatusAggregator.Aggregate(components);



        Assert.Equal(OperationalHealthStates.Unhealthy, result);

    }



    [Fact]

    public void OrderComponents_sorts_by_name()

    {

        var components = new List<ComponentHealthResult>

        {

            Component(OperationalHealthStates.Healthy, name: "search"),

            Component(OperationalHealthStates.Healthy, name: "audit"),

            Component(OperationalHealthStates.Healthy, name: "outbox"),

        };



        var ordered = HealthStatusAggregator.OrderComponents(components);



        Assert.Equal(["audit", "outbox", "search"], ordered.Select(c => c.Name).ToList());

    }



    private static ComponentHealthResult Component(

        string status,

        bool isCritical = false,

        string name = "component") =>

        new(name, status, null, null, 0, CheckedAt, isCritical, null);

}


