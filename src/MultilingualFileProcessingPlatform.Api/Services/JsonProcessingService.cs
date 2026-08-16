using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace MultilingualFileProcessingPlatform.Api.Services
{
    public class JsonProcessingService
    {
        public Dictionary<string, string> ExtractStrings(string json)
        {
            JsonNode? root = JsonNode.Parse(json);

            Dictionary<string, string> strings = new();

            ExtractNode(root, "", strings);

            return strings;
        }

        private void ExtractNode(JsonNode? node, string path, Dictionary<string, string> strings)
        {
            if (node is JsonObject jsonObject)
            {
                foreach (KeyValuePair<string, JsonNode?> property in jsonObject)
                {
                    string newPath = string.IsNullOrEmpty(path)
                        ? property.Key
                        : $"{path}.{property.Key}";

                    ExtractNode(property.Value, newPath, strings);
                }
            }
            else if (node is JsonArray jsonArray)
            {
                for (int i = 0; i < jsonArray.Count; i++)
                {
                    string newPath = $"{path}[{i}]";

                    ExtractNode(jsonArray[i], newPath, strings);
                }
            }
            else if (node is JsonValue jsonValue && jsonValue.TryGetValue<string>(out string? value))
            {
                strings[path] = value;
            }
        }
    }
}