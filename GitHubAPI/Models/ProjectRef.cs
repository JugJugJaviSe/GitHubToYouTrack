using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GitHubAPI.Models
{
    public class ProjectRef
    {
        [JsonPropertyName("$type")]
        public string Type { get; set; } = "Project";

        [JsonPropertyName("id")]
        public string Id { get; set; }
    }
}
