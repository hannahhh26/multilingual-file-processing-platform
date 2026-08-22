namespace MultilingualFileProcessingPlatform.Api.Models
{
    public enum GetDeliveryResultType
    {
        Success,
        JobNotFound,
        DeliveryNotFound
    }

    public class GetDeliveryResult
    {
        public GetDeliveryResultType Result { get; set; }

        public string? FilePath { get; set; }
    }
}