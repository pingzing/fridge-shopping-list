using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FridgeShoppingList.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
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

        public static string GeneratedId(string generatedId)
        {
            return generatedId;
        }

        public static string Body => "body";
        public static string Title => "title";
    }

    public enum OneNoteChangeAction
    {
        Append,
        Insert,
        Prepend,
        Replace
    }

    public enum OneNoteChangePosition
    {
        After,
        Before
    }
}
