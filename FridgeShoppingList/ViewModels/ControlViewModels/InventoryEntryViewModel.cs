using FridgeShoppingList.Models;
using FridgeShoppingList.Services.SettingsServices;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Template10.Mvvm;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace FridgeShoppingList.ViewModels.ControlViewModels
{
    public class InventoryEntryViewModel : BindableBase
    {
        private readonly SettingsService _settings;

        public InventoryEntry Entry { get; set; }

        private DateTime _soonestExpiryDate;
        public DateTime SoonestExpiryDate
        {
            get { return _soonestExpiryDate; }
            set { Set(ref _soonestExpiryDate, value); }
        }

        private SolidColorBrush _expirationDateBackground;
        public SolidColorBrush ExpirationDateBackground
        {
            get { return _expirationDateBackground; }
            set { Set(ref _expirationDateBackground, value); }
        }

        private SolidColorBrush _expirationDateForeground;
        public SolidColorBrush ExpirationDateForeground
        {
            get { return _expirationDateForeground; }
            set { Set(ref _expirationDateForeground, value); }
        }

        private bool _isExpired;
        public bool IsExpired
        {
            get { return _isExpired; }
            set { Set(ref _isExpired, value); }
        }

        public RelayCommand<DateTime> DeleteIndividualCommand => new RelayCommand<DateTime>(DeleteIndividual);
        public RelayCommand DeleteItemCommand => new RelayCommand(DeleteItem);
        public RelayCommand AddToShoppingListCommand => new RelayCommand(AddToShoppingList);

        private DispatcherTimer _backgroundUpdateTimer = new DispatcherTimer();
        private TimeSpan _expiryShadingBaseline = TimeSpan.FromDays(3);
        private Color _baselineForegroundColor = (Color)App.Current.Resources["LcarsBlueGray"];

        public InventoryEntryViewModel(InventoryEntry entry, SettingsService settings)
        {
            _settings = settings;

            Entry = entry;
            SoonestExpiryDate = Entry.ExpiryDates.Min();
            _backgroundUpdateTimer.Interval = TimeSpan.FromHours(4);
            _backgroundUpdateTimer.Tick += UpdateExpirationsColors;
            _backgroundUpdateTimer.Start();
            UpdateExpirationsColors(null, null);
        }

        private void UpdateExpirationsColors(object sender, object e)
        {
            var now = DateTime.Now;
            DateTime nearestExpiryDate = Entry.ExpiryDates.Min();
            var timeTillExpiry = nearestExpiryDate - now;

            //If expired
            if (timeTillExpiry <= TimeSpan.Zero)
            {
                ExpirationDateBackground = new SolidColorBrush(Colors.Red);
                ExpirationDateForeground = new SolidColorBrush(Colors.Black);
                IsExpired = true;
            }
            else if (timeTillExpiry <= _expiryShadingBaseline)
            {
                double percentageOpacity = (_expiryShadingBaseline.TotalDays - timeTillExpiry.TotalDays) / _expiryShadingBaseline.TotalDays;
                ExpirationDateBackground = new SolidColorBrush { Color = Colors.Red, Opacity = percentageOpacity };                
                ExpirationDateForeground = new SolidColorBrush(_baselineForegroundColor);
            }
            else
            {
                ExpirationDateBackground = new SolidColorBrush(Colors.Transparent);
                ExpirationDateForeground = new SolidColorBrush(_baselineForegroundColor);
            }
        }

        private void DeleteItem()
        {
            _settings.RemoveFromInventoryItems(Entry);
        }

        private void AddToShoppingList()
        {
            _settings.AddToShoppingList(Entry);
        }

        private void DeleteIndividual(DateTime toDelete)
        {
            if (Entry.ExpiryDates.Count > 1)
            {
                Entry.ExpiryDates.Remove(toDelete);
            }
            else
            {
                DeleteItem();
            }
        }
    }
}
