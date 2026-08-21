namespace MultilingualFileProcessingPlatform.Api.Models
{
    public enum SaveTranslationFileResultType
    {
        Success,
        JobNotFound,
        InvalidFileType,
        InvalidJson
    }

    public class SaveTranslationFileResult
    {
        public SaveTranslationFileResultType Result { get; set; }
    }
}