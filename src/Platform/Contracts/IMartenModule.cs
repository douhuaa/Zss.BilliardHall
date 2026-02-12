using Marten;

namespace Zss.BilliardHall.Platform.Contracts;

public interface IMartenModule
{
    void ConfigureMarten(StoreOptions options);
}
