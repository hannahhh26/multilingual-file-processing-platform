using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Text.Json.Nodes;
using MultilingualFileProcessingPlatform.Api.Models;

namespace MultilingualFileProcessingPlatform.Api.Services
{
    public class JsonProcessingService
    {
        public JsonPreprocessingResult PreprocessJson(string json)
        {
            JsonNode? root = JsonNode.Parse(json);

            List<TranslationSegment> segments = new();

            JsonNode? reconstructionData = ExtractNode(root, "", segments);

            return new JsonPreprocessingResult
            {
                Segments = segments,
                ReconstructionData = reconstructionData
            };
        }

        private JsonNode? ExtractNode(
            JsonNode? node,
            string path,
            List<TranslationSegment> segments)
        {
            if (node is JsonObject jsonObject)
            {
                JsonObject reconstructedObject = new();

                foreach (KeyValuePair<string, JsonNode?> property in jsonObject)
                {
                    string newPath = string.IsNullOrEmpty(path)
                        ? property.Key
                        : $"{path}.{property.Key}";

                    reconstructedObject[property.Key] =
                        ExtractNode(property.Value, newPath, segments);
                }

                return reconstructedObject;
            }
            else if (node is JsonArray jsonArray)
            {
                JsonArray reconstructedArray = new();

                for (int i = 0; i < jsonArray.Count; i++)
                {
                    string newPath = $"{path}[{i}]";

                    reconstructedArray.Add(
                        ExtractNode(jsonArray[i], newPath, segments));
                }

                return reconstructedArray;
            }
            else if (node is JsonValue jsonValue &&
                     jsonValue.TryGetValue<string>(out string? value))
            {
                string segmentId = $"seg-{segments.Count + 1:D4}";

                segments.Add(new TranslationSegment
                {
                    Id = segmentId,
                    Path = path,
                    Source = value
                });

                return new JsonObject
                {
                    ["__segmentId"] = segmentId
                };
            }

            return node?.DeepClone();
        }
    }
}