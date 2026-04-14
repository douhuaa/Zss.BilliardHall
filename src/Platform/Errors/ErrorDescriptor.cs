using Microsoft.Extensions.Logging;

namespace Zss.BilliardHall.Platform.Errors;

public sealed record ErrorDescriptor(
    string Code,
    string Title,
    int HttpStatus,
    string ProblemType,
    LogLevel LogLevel);

