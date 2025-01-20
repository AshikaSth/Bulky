using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BulkyBook.Models
{
    public class FeatureFlag
    {
        public int Id { get; set; }
        [JsonPropertyName("name")]
        [Required(ErrorMessage = "Feature flag name is required.")]
        public string Name { get; set; }
        [JsonPropertyName("isEnabled")]
        public bool IsEnabled { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
