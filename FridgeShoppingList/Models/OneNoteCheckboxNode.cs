using HtmlAgilityPack;
using System.Diagnostics;
using System;

namespace FridgeShoppingList.Models
{
    [DebuggerDisplay("{Content}, IsChecked: {IsChecked}, Id: {Id}")]
    public class OneNoteCheckboxNode
    {
        public string GeneratedId { get; set; }
        public string DataId { get; set; }
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
                GeneratedId = html.Attributes["id"].Value;
            }
            if (html.Attributes.Contains("data-id"))
            {
                DataId = html.Attributes["data-id"].Value;
            }

            Content = html.InnerText;
        }

        public OneNoteCheckboxNode(ShoppingListEntry entry)
        {
            GeneratedId = null;
            DataId = entry.ItemType.ItemTypeId.ToString();
            IsChecked = false;
            Content = entry.ItemType.Name;
        }

        internal string ToHtmlContent()
        {
            return $"<p data-tag=\"{BoolAsTodoAttribute(IsChecked)}\" data-id=\"{DataId}\">{Content}</p>";
        }

        private string BoolAsTodoAttribute(bool isChecked)
        {
            if (isChecked)
            {
                return "to-do:completed";
            }
            else
            {
                return "to-do";
            }
        }
    }
}
