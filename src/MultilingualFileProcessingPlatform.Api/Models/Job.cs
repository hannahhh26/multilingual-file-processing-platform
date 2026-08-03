namespace MultilingualFileProcessingPlatform.Api.Models;

/// <summary>
/// Represents a file-processing job.
/// </summary>
public class Job
{
    /// <summary>
    /// Unique identifier for the job.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User-friendly name for the job.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Current processing status of the job.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Date and time when the job was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}