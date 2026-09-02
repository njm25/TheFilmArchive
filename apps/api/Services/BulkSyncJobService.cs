namespace Api.Services;

public enum BulkSyncState
{
    Idle,
    Running,
    Completed,
    Failed
}

public class BulkSyncError
{
    public string Title { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

public class BulkSyncStatus
{
    public BulkSyncState State { get; set; } = BulkSyncState.Idle;
    public string Phase { get; set; } = string.Empty;
    public int TotalFilms { get; set; }
    public int ProcessedFilms { get; set; }
    public int CreatedCount { get; set; }
    public int RefreshedCount { get; set; }
    public int SkippedCount { get; set; }
    public int FailedCount { get; set; }
    public string? CurrentFilmTitle { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<BulkSyncError> Errors { get; set; } = new();
}

/// <summary>
/// Holds in-memory progress for the single admin-triggered bulk import/refresh job.
/// State is intentionally not persisted - it resets on app restart, which is fine
/// for a low-frequency, safely-re-runnable admin action.
/// </summary>
public class BulkSyncJobService
{
    private readonly object _lock = new();
    private BulkSyncStatus _status = new();

    public bool TryStart()
    {
        lock (_lock)
        {
            if (_status.State == BulkSyncState.Running)
                return false;

            _status = new BulkSyncStatus
            {
                State = BulkSyncState.Running,
                Phase = "Starting",
                StartedAt = DateTime.UtcNow
            };

            return true;
        }
    }

    public void SetPhase(string phase)
    {
        lock (_lock) { _status.Phase = phase; }
    }

    public void SetTotal(int total)
    {
        lock (_lock) { _status.TotalFilms = total; }
    }

    public void IncreaseTotalBy(int amount)
    {
        lock (_lock) { _status.TotalFilms += amount; }
    }

    public void SetCurrent(string title)
    {
        lock (_lock) { _status.CurrentFilmTitle = title; }
    }

    public void IncrementCreated()
    {
        lock (_lock) { _status.CreatedCount++; }
    }

    public void IncrementRefreshed()
    {
        lock (_lock) { _status.RefreshedCount++; }
    }

    public void IncrementSkipped()
    {
        lock (_lock) { _status.SkippedCount++; }
    }

    public void IncrementProcessed()
    {
        lock (_lock) { _status.ProcessedFilms++; }
    }

    public void RecordFailure(string title, string reason)
    {
        lock (_lock)
        {
            _status.FailedCount++;
            _status.Errors.Add(new BulkSyncError { Title = title, Reason = reason });
        }
    }

    public void Complete()
    {
        lock (_lock)
        {
            _status.State = BulkSyncState.Completed;
            _status.Phase = "Done";
            _status.CurrentFilmTitle = null;
            _status.CompletedAt = DateTime.UtcNow;
        }
    }

    public void Fail(string reason)
    {
        lock (_lock)
        {
            _status.State = BulkSyncState.Failed;
            _status.Phase = "Failed";
            _status.CurrentFilmTitle = null;
            _status.CompletedAt = DateTime.UtcNow;
            _status.Errors.Add(new BulkSyncError { Title = "(job)", Reason = reason });
        }
    }

    public BulkSyncStatus GetSnapshot()
    {
        lock (_lock)
        {
            return new BulkSyncStatus
            {
                State = _status.State,
                Phase = _status.Phase,
                TotalFilms = _status.TotalFilms,
                ProcessedFilms = _status.ProcessedFilms,
                CreatedCount = _status.CreatedCount,
                RefreshedCount = _status.RefreshedCount,
                SkippedCount = _status.SkippedCount,
                FailedCount = _status.FailedCount,
                CurrentFilmTitle = _status.CurrentFilmTitle,
                StartedAt = _status.StartedAt,
                CompletedAt = _status.CompletedAt,
                Errors = new List<BulkSyncError>(_status.Errors)
            };
        }
    }
}
