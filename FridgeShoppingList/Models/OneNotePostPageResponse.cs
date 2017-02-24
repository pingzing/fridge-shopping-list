using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FridgeShoppingList.Models
{
    public class OneNotePostPageResponse
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

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("self")]
        public string Self { get; set; }        
    }
}
