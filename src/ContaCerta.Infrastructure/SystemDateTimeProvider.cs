using ContaCerta.Application.Common.Interfaces;

namespace ContaCerta.Infrastructure;

public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateOnly Hoje => DateOnly.FromDateTime(DateTime.UtcNow);
    public DateTime AgoraUtc => DateTime.UtcNow;
}
