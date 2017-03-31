using HtmlAgilityPack;
using System.Diagnostics;
using System;
using FridgeShoppingList.Services.SettingsServices;
using Microsoft.Practices.ServiceLocation;
using DynamicData;
using System.Linq;
using System.Collections.Generic;
using Optional;
using Windows.Foundation;
using System.Net;

namespace FridgeShoppingList.Models
{
    [DebuggerDisplay("{Content}, IsChecked: {IsChecked}, DataId: {DataId}, GeneratedId: {GeneratedId}")]
    public class OneNoteCheckboxNode
    {
        private static Lazy<SettingsService> _settingsService = new Lazy<SettingsService>(
            () => ServiceLocator.Current.GetInstance<SettingsService>());

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

            Content = WebUtility.UrlDecode(html.InnerText);
        }

        public OneNoteCheckboxNode(ShoppingListEntry entry)
        {
            GeneratedId = null;
            DataId = entry.ItemType.ItemTypeId.ToString();
            IsChecked = false;
            Content = $"{entry.ItemType.Name} x{entry.Count}";
        }

        public Option<ShoppingListEntry> AsShoppingListEntry()
        {
            SettingsService settings = _settingsService.Value;
            IEnumerable<GroceryItemType> itemTypes = settings.GroceryTypes.AsObservableList().Items;

            GroceryItemType itemType = itemTypes.FirstOrDefault(x => x.ItemTypeId.ToString() == this.DataId);
            if (itemType == null)
            {                
                itemType = itemTypes.FirstOrDefault(x => x.Name == this.Content);
            }

            if(itemType == null)
            {
                // TODO: This is a candidate for where we might add new types we don't already have
                return Option.None<ShoppingListEntry>();
            }

            uint itemCount;
            bool parseItemCountSuccess = UInt32.TryParse(this.Content.Substring(this.Content.LastIndexOf('x') + 1), out itemCount);

            return new ShoppingListEntry
            {
                ItemType = itemType,
                Count = parseItemCountSuccess ? itemCount : 1
            }.Some();
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
