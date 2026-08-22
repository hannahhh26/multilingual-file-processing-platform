using System.Text.Json.Nodes;
using MultilingualFileProcessingPlatform.Api.Models;
using MultilingualFileProcessingPlatform.Api.Services;

namespace MultilingualFileProcessingPlatform.Tests
{
    public class JsonProcessingServiceTests
    {
        [Fact]
        public void PreprocessJson_ExtractsNestedStringValues()
        {
            JsonProcessingService service = new JsonProcessingService();

            string json = """
            {
                "page": {
                    "title": "Welcome",
                    "button": "Continue"
                }
            }
            """;

            JsonPreprocessingResult result = service.PreprocessJson(json);

            Assert.Equal(2, result.Segments.Count);

            Assert.Equal("seg-0001", result.Segments[0].Id);
            Assert.Equal("page.title", result.Segments[0].Path);
            Assert.Equal("Welcome", result.Segments[0].Source);

            Assert.Equal("seg-0002", result.Segments[1].Id);
            Assert.Equal("page.button", result.Segments[1].Path);
            Assert.Equal("Continue", result.Segments[1].Source);
        }

        [Fact]
        public void PreprocessJson_ExtractsStringValuesFromArrays()
        {
            JsonProcessingService service = new JsonProcessingService();

            string json = """
            {
                "messages": [
                    "Hello",
                    "Goodbye"
                ]
            }
            """;

            JsonPreprocessingResult result = service.PreprocessJson(json);

            Assert.Equal(2, result.Segments.Count);

            Assert.Equal("seg-0001", result.Segments[0].Id);
            Assert.Equal("messages[0]", result.Segments[0].Path);
            Assert.Equal("Hello", result.Segments[0].Source);

            Assert.Equal("seg-0002", result.Segments[1].Id);
            Assert.Equal("messages[1]", result.Segments[1].Path);
            Assert.Equal("Goodbye", result.Segments[1].Source);
        }

        [Fact]
        public void PreprocessJson_IgnoresNonStringValues()
        {
            JsonProcessingService service = new JsonProcessingService();

            string json = """
            {
                "title": "Welcome",
                "count": 5,
                "enabled": true,
                "nothing": null
            }
            """;

            JsonPreprocessingResult result = service.PreprocessJson(json);

            Assert.Single(result.Segments);

            Assert.Equal("seg-0001", result.Segments[0].Id);
            Assert.Equal("title", result.Segments[0].Path);
            Assert.Equal("Welcome", result.Segments[0].Source);
        }

        [Fact]
        public void PreprocessJson_CreatesReconstructionData()
        {
            JsonProcessingService service = new JsonProcessingService();

            string json = """
    {
        "page": {
            "title": "Welcome",
            "count": 5,
            "button": "Continue"
        }
    }
    """;

            JsonPreprocessingResult result = service.PreprocessJson(json);

            string reconstructionJson = result.ReconstructionData!.ToJsonString();

            Assert.Contains("\"__segmentId\":\"seg-0001\"", reconstructionJson);
            Assert.Contains("\"count\":5", reconstructionJson);
            Assert.Contains("\"__segmentId\":\"seg-0002\"", reconstructionJson);
        }

        [Fact]
        public void RebuildJson_ReplacesSegmentMarkersWithTranslatedValues()
        {
            JsonProcessingService service = new JsonProcessingService();

            string reconstructionJson = """
    {
        "product": {
            "name": {
                "__segmentId": "seg-0001"
            },
            "price": 129.99,
            "available": true
        },
        "messages": {
            "addToBasket": {
                "__segmentId": "seg-0002"
            }
        }
    }
    """;

            string translationJson = """
    {
        "segments": [
            {
                "id": "seg-0001",
                "path": "product.name",
                "source": "Casque sans fil"
            },
            {
                "id": "seg-0002",
                "path": "messages.addToBasket",
                "source": "Ajouter au panier"
            }
        ]
    }
    """;

            string result = service.RebuildJson(
                reconstructionJson,
                translationJson);

            JsonNode? rebuilt = JsonNode.Parse(result);

            Assert.Equal(
                "Casque sans fil",
                rebuilt?["product"]?["name"]?.GetValue<string>());

            Assert.Equal(
                129.99,
                rebuilt?["product"]?["price"]?.GetValue<double>());

            Assert.True(
                rebuilt?["product"]?["available"]?.GetValue<bool>());

            Assert.Equal(
                "Ajouter au panier",
                rebuilt?["messages"]?["addToBasket"]?.GetValue<string>());
        }
    }
}