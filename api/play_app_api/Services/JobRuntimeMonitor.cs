using System.Collections.Concurrent;

namespace play_app_api.Services;

/// <summary>
/// In-memory runtime monitor for tracking job states and subtasks independent of the database
/// </summary>
public class JobRuntimeMonitor
{
    private readonly ConcurrentDictionary<string, RuntimeJobInfo> _queuedJobs = new();
    private readonly ConcurrentDictionary<string, RuntimeJobInfo> _activeJobs = new();
    private readonly ConcurrentQueue<RuntimeJobInfo> _recentlyCompleted = new();
    private readonly object _heartbeatLock = new();
    private WorkerHeartbeat? _lastWorker;

    public void MarkQueued(string jobToken, string contentType)
    {
        var job = new RuntimeJobInfo
        {
            JobToken = jobToken,
            ContentType = contentType,
            State = "queued",
            LastUpdatedAt = DateTime.UtcNow,
            LastEvent = "queued"
        };
        _queuedJobs[jobToken] = job;
    }

    public void MarkPicked(string jobToken, string contentType)
    {
        if (_queuedJobs.TryRemove(jobToken, out var job))
        {
            job.State = "in_progress";
            job.StartedAt = DateTime.UtcNow;
            job.LastUpdatedAt = DateTime.UtcNow;
            job.LastEvent = "picked";
            _activeJobs[jobToken] = job;
        }
    }

    public void MarkEvent(string jobToken, string eventName)
    {
        if (_activeJobs.TryGetValue(jobToken, out var job))
        {
            job.LastEvent = eventName;
            job.LastUpdatedAt = DateTime.UtcNow;
        }
    }

    public void MarkOpenRouterStart(string jobToken)
    {
        if (_activeJobs.TryGetValue(jobToken, out var job))
        {
            job.OpenRouterStartedAt = DateTime.UtcNow;
            job.LastEvent = "openrouter_start";
            job.LastUpdatedAt = DateTime.UtcNow;
        }
    }

    public void MarkOpenRouterFinish(string jobToken, long elapsedMs, int statusCode)
    {
        if (_activeJobs.TryGetValue(jobToken, out var job))
        {
            job.OpenRouterElapsedMs = elapsedMs;
            job.OpenRouterStatusCode = statusCode;
            job.LastEvent = "openrouter_finish";
            job.LastUpdatedAt = DateTime.UtcNow;
        }
    }

    public void MarkCompleted(string jobToken, bool success)
    {
        if (_activeJobs.TryRemove(jobToken, out var job))
        {
            job.State = success ? "completed" : "failed";
            job.LastEvent = success ? "completed" : "failed";
            job.LastUpdatedAt = DateTime.UtcNow;
            
            _recentlyCompleted.Enqueue(job);
            
            // Keep only last 10 completed jobs
            while (_recentlyCompleted.Count > 10)
            {
                _recentlyCompleted.TryDequeue(out _);
            }
        }
    }

    public void MarkSubtaskStart(string jobToken, string subtaskName)
    {
        StartSubtask(jobToken, subtaskName);
    }

    public void MarkSubtaskFinish(string jobToken, string subtaskName, bool success)
    {
        CompleteSubtask(jobToken, subtaskName, success);
    }

    public void MarkSubtaskError(string jobToken, string subtaskName, string error)
    {
        if (_activeJobs.TryGetValue(jobToken, out var job))
        {
            var subtask = job.Subtasks.FirstOrDefault(s => s.Name == subtaskName);
            if (subtask != null)
            {
                subtask.State = "failed";
                subtask.CompletedAt = DateTime.UtcNow;
                subtask.Success = false;
                subtask.Error = error;
            }
            
            job.LastEvent = $"subtask_error:{subtaskName}";
            job.LastUpdatedAt = DateTime.UtcNow;
        }
    }

    public void StartSubtask(string jobToken, string subtaskName)
    {
        if (_activeJobs.TryGetValue(jobToken, out var job))
        {
            var subtask = new SubtaskInfo
            {
                Name = subtaskName,
                State = "running",
                StartedAt = DateTime.UtcNow
            };
            job.Subtasks.Add(subtask);
            job.LastEvent = $"subtask_start:{subtaskName}";
            job.LastUpdatedAt = DateTime.UtcNow;
        }
    }

    public void CompleteSubtask(string jobToken, string subtaskName, bool success)
    {
        if (_activeJobs.TryGetValue(jobToken, out var job))
        {
            var subtask = job.Subtasks.FirstOrDefault(s => s.Name == subtaskName);
            if (subtask != null)
            {
                subtask.State = success ? "completed" : "failed";
                subtask.CompletedAt = DateTime.UtcNow;
                subtask.Success = success;
            }
            
            job.LastEvent = $"subtask_{(success ? "completed" : "failed")}:{subtaskName}";
            job.LastUpdatedAt = DateTime.UtcNow;
        }
    }

    public void UpdateHeartbeat(int? pendingCount, int? inProgressCount, int concurrencyLimit)
    {
        lock (_heartbeatLock)
        {
            if (_lastWorker == null)
            {
                _lastWorker = new WorkerHeartbeat
                {
                    StartedAtUtc = DateTime.UtcNow
                };
            }
            
            _lastWorker.AtUtc = DateTime.UtcNow;
            _lastWorker.PendingCount = pendingCount;
            _lastWorker.InProgressCount = inProgressCount;
            _lastWorker.ConcurrencyLimit = concurrencyLimit;
        }
    }

    public void RecordError(string error)
    {
        lock (_heartbeatLock)
        {
            if (_lastWorker != null)
            {
                _lastWorker.LastErrorAtUtc = DateTime.UtcNow;
                _lastWorker.LastError = error;
            }
        }
    }

    public void WorkerStarted()
    {
        lock (_heartbeatLock)
        {
            _lastWorker = new WorkerHeartbeat
            {
                StartedAtUtc = DateTime.UtcNow,
                AtUtc = DateTime.UtcNow
            };
        }
    }

    public void ClearRuntime()
    {
        _queuedJobs.Clear();
        _activeJobs.Clear();
        
        // Clear recent queue
        while (_recentlyCompleted.TryDequeue(out _)) { }
        
        lock (_heartbeatLock)
        {
            _lastWorker = null;
        }
    }

    public void SyncWithDatabase(List<RuntimeJobInfo> pendingJobs, List<RuntimeJobInfo> inProgressJobs, List<RuntimeJobInfo> recentJobs)
    {
        // Clear current state
        _queuedJobs.Clear();
        _activeJobs.Clear();
        while (_recentlyCompleted.TryDequeue(out _)) { }

        // Add pending jobs
        foreach (var job in pendingJobs)
        {
            _queuedJobs[job.JobToken] = job;
        }

        // Add in-progress jobs
        foreach (var job in inProgressJobs)
        {
            _activeJobs[job.JobToken] = job;
        }

        // Add recent jobs
        foreach (var job in recentJobs)
        {
            _recentlyCompleted.Enqueue(job);
        }
    }

    public object Snapshot()
    {
        lock (_heartbeatLock)
        {
            return new
            {
                queued = _queuedJobs.Values.ToArray(),
                active = _activeJobs.Values.ToArray(),
                recent = _recentlyCompleted.ToArray(),
                queuedCount = _queuedJobs.Count,
                activeCount = _activeJobs.Count,
                recentCount = _recentlyCompleted.Count,
                lastWorker = _lastWorker
            };
        }
    }

    public object? GetJobTrace(string jobToken)
    {
        // Check active jobs first
        if (_activeJobs.TryGetValue(jobToken, out var activeJob))
        {
            return activeJob;
        }
        
        // Check recent jobs
        var recentJob = _recentlyCompleted.FirstOrDefault(j => j.JobToken == jobToken);
        if (recentJob != null)
        {
            return recentJob;
        }
        
        // Check queued jobs
        if (_queuedJobs.TryGetValue(jobToken, out var queuedJob))
        {
            return queuedJob;
        }
        
        return null;
    }
}

public class RuntimeJobInfo
{
    public string JobToken { get; set; } = "";
    public string ContentType { get; set; } = "";
    public string State { get; set; } = "";
    public DateTime? StartedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public string LastEvent { get; set; } = "";
    public DateTime? OpenRouterStartedAt { get; set; }
    public long? OpenRouterElapsedMs { get; set; }
    public int? OpenRouterStatusCode { get; set; }
    public List<SubtaskInfo> Subtasks { get; set; } = new();
}

public class SubtaskInfo
{
    public string Name { get; set; } = "";
    public string State { get; set; } = "";
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool? Success { get; set; }
    public string? Error { get; set; }
}

public class WorkerHeartbeat
{
    public DateTime? AtUtc { get; set; }
    public int? PendingCount { get; set; }
    public int? InProgressCount { get; set; }
    public int? ConcurrencyLimit { get; set; }
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? LastErrorAtUtc { get; set; }
    public string? LastError { get; set; }
}