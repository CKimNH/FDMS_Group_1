using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using GroundTerminalSoftware.Models;

namespace GroundTerminalSoftware.ViewModels
{
    public class RealTimeViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<FlightDataEntry> flightDataEntries;
        public ObservableCollection<FlightDataEntry> FlightDataEntries
        {
            get => flightDataEntries;
            set
            {
                if (flightDataEntries != value)
                {
                    flightDataEntries = value;
                    OnPropertyChanged(nameof(FlightDataEntries));
                }
            }
        }

        public RealTimeViewModel()
        {
            FlightDataEntries = new ObservableCollection<FlightDataEntry>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}