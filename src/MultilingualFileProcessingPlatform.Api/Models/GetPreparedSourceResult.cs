namespace MultilingualFileProcessingPlatform.Api.Models
{
    public enum GetPreparedSourceResultType
    {
        Success,
        JobNotFound,
        PreparedSourceNotFound
    }

    public class GetPreparedSourceResult
    {
        public GetPreparedSourceResultType Result { get; set; }

        public string? FilePath { get; set; }
    }
}