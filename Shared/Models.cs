using Newtonsoft.Json;

namespace CosmosConsole.Models
{
    public class Product
    {
        // Cosmosdb is id
        // App is ProductId
        [JsonProperty(PropertyName = "id")]
        public string? ProductId { get; set; }
        public string? Name { get; set; }
        public string? CategoryId { get; set; } // PartitionKey
        public double Price { get; set; }
        public string[]? Tags { get; set; }
    }
}
