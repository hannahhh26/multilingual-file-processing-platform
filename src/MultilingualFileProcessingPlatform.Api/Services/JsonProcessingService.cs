using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Encodings.Web;
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

        public string RebuildJson(string reconstructionJson, string translationJson)
        {
            JsonNode? reconstructionRoot = JsonNode.Parse(reconstructionJson);
            JsonNode? translationRoot = JsonNode.Parse(translationJson);

            Dictionary<string, string> translations = new();

            JsonArray? segments = translationRoot?["segments"]?.AsArray();

            if (segments != null)
            {
                foreach (JsonNode? segmentNode in segments)
                {
                    if (segmentNode is not JsonObject segmentObject)
                    {
                        continue;
                    }

                    string? id = segmentObject["id"]?.GetValue<string>();
                    string? source = segmentObject["source"]?.GetValue<string>();

                    if (id != null && source != null)
                    {
                        translations[id] = source;
                    }
                }
            }

            JsonNode? rebuilt = RebuildNode(reconstructionRoot, translations);

            return rebuilt?.ToJsonString(new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            }) ?? string.Empty;
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

        private JsonNode? RebuildNode(JsonNode? node, Dictionary<string, string> translations)
        {
            if (node is JsonObject jsonObject)
            {
                if (jsonObject.TryGetPropertyValue("__segmentId", out JsonNode? segmentIdNode))
                {
                    string? segmentId = segmentIdNode?.GetValue<string>();

                    if (segmentId != null &&
                        translations.TryGetValue(segmentId, out string? translatedValue))
                    {
                        return JsonValue.Create(translatedValue);
                    }
                }

                JsonObject rebuiltObject = new();

                foreach (KeyValuePair<string, JsonNode?> property in jsonObject)
                {
                    rebuiltObject[property.Key] =
                        RebuildNode(property.Value, translations);
                }

                return rebuiltObject;
            }

            if (node is JsonArray jsonArray)
            {
                JsonArray rebuiltArray = new();

                foreach (JsonNode? item in jsonArray)
                {
                    rebuiltArray.Add(
                        RebuildNode(item, translations));
                }

                return rebuiltArray;
            }

            return node?.DeepClone();
        }
    }
}