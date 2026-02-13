namespace Zss.BilliardHall.Platform.Contracts;

public interface ICommandHandler<in TCommand, TResult>
{
    Task<TResult> Handle(TCommand command);
}
