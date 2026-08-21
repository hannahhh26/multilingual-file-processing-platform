using System.Text.Json.Nodes;

namespace MultilingualFileProcessingPlatform.Api.Models
{
    public class JsonPreprocessingResult
    {
        public List<TranslationSegment> Segments { get; set; } = new();

        public JsonNode? ReconstructionData { get; set; }
    }
}