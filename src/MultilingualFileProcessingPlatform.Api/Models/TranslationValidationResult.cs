namespace MultilingualFileProcessingPlatform.Api.Models
{
    public class TranslationValidationResult
    {
        public bool IsValid { get; set; }

        public List<string> MissingSegmentIds { get; set; } = new();

        public List<string> DuplicateSegmentIds { get; set; } = new();

        public List<string> UnexpectedSegmentIds { get; set; } = new();
    }
}