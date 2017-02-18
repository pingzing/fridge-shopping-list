using HtmlAgilityPack;
using System.Diagnostics;

namespace FridgeShoppingList.Models
{
    [DebuggerDisplay("{Content}, IsChecked: {IsChecked}, Id: {Id}")]
    public class OneNoteCheckboxNode
    {
        public string Id { get; set; }
        public bool IsChecked { get; set; }
        public string Content { get; set; }
        
        public OneNoteCheckboxNode(HtmlNode html)
        {
            if (html.Attributes["data-tag"].Value == "to-do")
            {
                IsChecked = false;
            }
            else if (html.Attributes["data-tag"].Value == "to-do:completed")
            {
                IsChecked = true;
            }

            if (html.Attributes.Contains("id"))
            {
                Id = html.Attributes["id"].Value;
            }

            Content = html.InnerText;
        }
    }
}
