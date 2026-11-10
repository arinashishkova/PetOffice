using System;
using System.Collections.ObjectModel;

namespace PetOffice.WpfApp.ViewModels
{
    public sealed class LogEntry
    {
        public string Message { get; }

        public DateTime Time { get; }

        public string TimeText =>
            Time.ToString("HH:mm");


        public LogEntry(
            string message,
            DateTime time)
        {
            Message = message;
            Time = time;
        }
    }


    public sealed class LogEntryCollection
        : ObservableCollection<LogEntry>
    {
        public void Insert(
            int index,
            string message)
        {
            base.Insert(
                index,
                new LogEntry(
                    message,
                    DateTime.Now));
        }
    }
}