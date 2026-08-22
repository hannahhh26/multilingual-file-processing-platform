namespace MultilingualFileProcessingPlatform.Api.Models
{
    public enum PostprocessJobResultType
    {
        Success,
        JobNotFound,
        ReconstructionDataNotFound,
        TranslationFileNotFound,
        TranslationValidationFailed
    }

    public class PostprocessJobResult
    {
        public PostprocessJobResultType Result { get; set; }

        public TranslationValidationResult? Validation { get; set; }
    }
}