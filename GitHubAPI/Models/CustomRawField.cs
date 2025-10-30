using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GitHubAPI.Models
{
    public class CustomRawField
    {
        [JsonPropertyName("$type")]
        public string Type { get; set; } = "SingleEnumIssueCustomField";

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("value")]
        public JsonElement? JsonElement { get; set; }
    }
}
