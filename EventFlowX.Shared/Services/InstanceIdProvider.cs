namespace EventFlowX.Shared.Services;

public interface IInstanceIdProvider
{
    string InstanceId { get; }
}

public class InstanceIdProvider : IInstanceIdProvider
{
    public string InstanceId { get; }
    public InstanceIdProvider(string instanceId) => InstanceId = instanceId;
}