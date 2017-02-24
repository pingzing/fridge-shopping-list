using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FridgeShoppingList.Models
{
    public class OneNoteGetPageMetadataResponse
    {
        [JsonProperty("@odata.context")]
        public string RequestSource { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("createdByAppId")]
        public string CreatedByAppId { get; set; }

        [JsonProperty("links")]
        public Links Links { get; set; }

        [JsonProperty("ContentUrl")]
        public string ContentUrl { get; set; }

        [JsonProperty("lastModifiedTime")]
        public DateTime LastModifiedTime { get; set; }

        [JsonProperty("createdTime")]
        public DateTime CreatedTime { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("self")]
        public string Self { get; set; }

        [JsonProperty("parentSection@odata.context")]
        public string ParentSectionODataContext { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
