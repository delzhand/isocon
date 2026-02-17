namespace SingularityGroup.HotReload {
    public interface IServerHealthCheck {
        bool IsServerHealthy { get; }
    }
    
    internal interface IServerHealthCheckInternal : IServerHealthCheck {
        void CheckHealth();
    }
}
