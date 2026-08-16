using MultilingualFileProcessingPlatform.Api.Services;

namespace MultilingualFileProcessingPlatform.Tests
{
    public class JsonProcessingServiceTests
    {
        [Fact]
        public void ExtractStrings_ExtractsNestedStringValues()
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

            Dictionary<string, string> result = service.ExtractStrings(json);

            Assert.Equal("Welcome", result["page.title"]);
            Assert.Equal("Continue", result["page.button"]);
        }

        [Fact]
        public void ExtractStrings_ExtractsStringValuesFromArrays()
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

            Dictionary<string, string> result = service.ExtractStrings(json);

            Assert.Equal("Hello", result["messages[0]"]);
            Assert.Equal("Goodbye", result["messages[1]"]);
        }

        [Fact]
        public void ExtractStrings_IgnoresNonStringValues()
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

            Dictionary<string, string> result = service.ExtractStrings(json);

            Assert.Single(result);
            Assert.Equal("Welcome", result["title"]);
        }
    }
}