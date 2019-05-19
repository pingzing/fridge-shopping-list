using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FridgeShoppingList.Models
{    
    public class OneNoteChangeObject
    {
        [JsonProperty("target")]
        public string Target { get; set; }

        [JsonProperty("action")]
        public OneNoteChangeAction Action { get; set; }

        [JsonProperty("position")]
        public OneNoteChangePosition? Position { get; set; }

        [JsonProperty("content")]
        public string HtmlContent { get; set; }
    }

    public static class OneNoteChangeTarget
    {
        public static string DataId(string idValue)
        {
            return $"#{idValue}";
        }

        public static string Body => "body";
        public static string Title => "title";
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum OneNoteChangeAction
    {
        [JsonProperty("append")]
        Append,

        [JsonProperty("insert")]
        Insert,

        [JsonProperty("prepend")]
        Prepend,

        [JsonProperty("replace")]
        Replace
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum OneNoteChangePosition
    {
        [JsonProperty("after")]
        After,

        [JsonProperty("before")]
        Before
    }
}
