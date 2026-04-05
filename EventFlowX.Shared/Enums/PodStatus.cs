namespace EventFlowX.Shared.Enums;

public enum PodStatus
{
    Running = 0,
    Stopping = 1,  // StopAsync tetiklendi, temiz kapanış
    Dead = 2       // heartbeat kesildi, crash oldu
}