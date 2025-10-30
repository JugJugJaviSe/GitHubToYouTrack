using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GitHubAPI.Models
{
    public class IssueWithTypedFields
    {
        [JsonPropertyName("$type")]
        public string Type { get; set; } = "Issue";

        [JsonPropertyName("idReadable")]
        public string IdReadable { get; set; }

        [JsonPropertyName("project")]
        public ProjectRef Project { get; set; }

        [JsonPropertyName("summary")]
        public string Summary { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("customFields")]
        public CustomTypedField[] CustomFields { get; set; }
    }
}
