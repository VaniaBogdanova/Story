using System.Text.Json.Serialization;


namespace StorySpoiler.Models
{
    class ApiResponseDTO
    {
        [JsonPropertyName("msg")]
        public string? Msg { get; set; }

        [JsonPropertyName("storyId")]
        public string? FoodyId { get; set; }
    }
}
