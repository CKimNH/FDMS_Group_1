using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using GroundTerminalSoftware.Models;

namespace GroundTerminalSoftware.ViewModels
{
    public class RealTimeViewModel : INotifyPropertyChanged
    {
        private static RealTimeViewModel _instance;
        public static RealTimeViewModel Instance => _instance ?? (_instance = new RealTimeViewModel());
        public bool IsActive { get; private set; }

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

        private RealTimeViewModel()
        {
            FlightDataEntries = new ObservableCollection<FlightDataEntry>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public void StartBackgroundTasks ()
        {
            if (!IsActive) { IsActive = true; }
            else { return; }
        }
    }
}