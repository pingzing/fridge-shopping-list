using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FridgeShoppingList.Models
{
    public class OneNoteODataResponse
    {
        [JsonProperty("@odata.context")]
        public string RequestSource { get; set; }

        [JsonProperty("value")]
        public List<ODataValue> Data { get; set; }
    }

    public class ODataValue
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("createdByAppId ")]
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

        [JsonProperty("parentSectionodatacontext")]
        public string ParentSectionODataContext { get; set; }

        [JsonProperty("parentSection")]
        public ParentSection ParentSection { get; set; }
    
    }

    public class Links
    {
        [JsonProperty("oneNoteClientUrl")]
        public OneNoteClientUrl OneNoteClientUrl { get; set; }

        [JsonProperty("oneNoteWebUrl")]
        public OneNoteWebUrl OneNoteWebUrl { get; set; }
    }

    public class OneNoteClientUrl
    {
        [JsonProperty("href")]
        public string Url { get; set; }
    }

    public class OneNoteWebUrl
    {
        [JsonProperty("href")]
        public string Url { get; set; }
    }

    public class ParentSection
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("self")]
        public string Self { get; set; }
    }

}
