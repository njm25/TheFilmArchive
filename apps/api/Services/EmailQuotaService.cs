namespace Api.Services;

// A hard ceiling on registration emails per UTC day, and the last line before
// the SES bill. Per-IP limits stop one attacker hammering the endpoint; this
// bounds what a distributed one - a botnet, or anyone who can rotate IPs - can
// cost before the tap shuts off.
//
// Deliberately in-memory: it is a spend ceiling, not an audit trail. That means
// it resets when the process restarts, and it counts per instance rather than
// across a fleet. Both are fine for a single EC2 box; if the API is ever scaled
// out or restarts become frequent, move the counter into the database.
public class EmailQuotaService
{
    private readonly int _dailyLimit;
    private readonly ILogger<EmailQuotaService> _logger;
    private readonly Lock _gate = new();

    private DateOnly _day = DateOnly.FromDateTime(DateTime.UtcNow);
    private int _sent;

    public EmailQuotaService(IConfiguration configuration, ILogger<EmailQuotaService> logger)
    {
        _dailyLimit = configuration.GetValue<int?>("Email:DailyRegistrationLimit") ?? 200;
        _logger = logger;
    }

    // Claims one send. Returns false when today's ceiling is already reached,
    // in which case the caller must not send.
    public bool TryReserve()
    {
        lock (_gate)
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

            if (today != _day)
            {
                _day = today;
                _sent = 0;
            }

            if (_sent >= _dailyLimit)
            {
                _logger.LogWarning(
                    "Daily registration email limit of {Limit} reached; refusing further sends today.",
                    _dailyLimit
                );

                return false;
            }

            _sent++;

            return true;
        }
    }
}
