using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PetOffice.Core.Models;

namespace PetOffice.WpfApp.ViewModels
{
    public sealed class LogEntry
    {
        // Original message. Kept for compatibility.
        public string Message { get; }
        public DateTime Time { get; }
        public string TimeText => Time.ToString("HH:mm");

        // Text before the first pet. Example: "Added: "
        public string Prefix { get; }

        // First pet.
        public string PetName { get; }
        public string PetType { get; }
        public string PetTypeText => string.IsNullOrWhiteSpace(PetType) ? "" : $" ({PetType})";

        // Text between two pets. Example: " welcomes "
        public string MiddleText { get; }

        // Optional second pet.
        public string SecondPetName { get; }
        public string SecondPetType { get; }
        public string SecondPetTypeText => string.IsNullOrWhiteSpace(SecondPetType) ? "" : $" ({SecondPetType})";

        // Text after the last pet.
        public string Suffix { get; }

        // Colors for pet name and class.
        public string PetColor => GetPetColor(PetType);
        public string SecondPetColor => GetPetColor(SecondPetType);

        public bool HasPet => !string.IsNullOrWhiteSpace(PetName);
        public bool HasSecondPet => !string.IsNullOrWhiteSpace(SecondPetName);


        // Plain log entry.
        public LogEntry(string message, DateTime time)
        {
            Message = message;
            Time = time;

            Prefix = message;
            PetName = "";
            PetType = "";
            MiddleText = "";
            SecondPetName = "";
            SecondPetType = "";
            Suffix = "";
        }

        private LogEntry(
            string message, DateTime time, string prefix,
            string petName, string petType, string middleText,
            string secondPetName, string secondPetType, string suffix)
        {
            Message = message;
            Time = time;

            Prefix = prefix;
            PetName = petName;
            PetType = petType;
            MiddleText = middleText;
            SecondPetName = secondPetName;
            SecondPetType = secondPetType;
            Suffix = suffix;
        }


        // Message produced by one known pet.
        public static LogEntry FromPet(string message, OfficePet pet, DateTime time)
        {
            string type = pet.GetType().Name;

            // Messages containing "Name - Class".
            int displayNameIndex = message.IndexOf(pet.DisplayName, StringComparison.Ordinal);

            if (displayNameIndex >= 0)
            {
                string prefix = message[..displayNameIndex];
                string suffix = FormatSuffix(message[(displayNameIndex + pet.DisplayName.Length)..]);

                return new LogEntry(message, time, prefix, pet.Name, type, "", "", "", suffix);
            }

            // Messages containing only the pet name.
            int nameIndex = message.IndexOf(pet.Name, StringComparison.Ordinal);

            if (nameIndex >= 0)
            {
                string prefix = message[..nameIndex];
                string suffix = FormatSuffix(message[(nameIndex + pet.Name.Length)..]);

                return new LogEntry(message, time, prefix, pet.Name, type, "", "", "", suffix);
            }

            return new LogEntry(message, time);
        }


        // Reaction messages may contain two pets.
        // Example: Meow - CatManager welcomes Leonard - CatManager.
        public static LogEntry FromReaction(string message, IEnumerable<OfficePet> pets, DateTime time)
        {
            List<PetMessageMatch> matches = pets
                .Select(pet => new PetMessageMatch(
                    pet, message.IndexOf(pet.DisplayName, StringComparison.Ordinal)))
                .Where(match => match.Index >= 0)
                .OrderBy(match => match.Index)
                .ToList();

            if (matches.Count == 0)
            {
                return new LogEntry(message, time);
            }

            PetMessageMatch first = matches[0];
            string prefix = message[..first.Index];
            int firstEnd = first.Index + first.Pet.DisplayName.Length;

            // Only one pet was found.
            if (matches.Count == 1)
            {
                string suffix = FormatSuffix(message[firstEnd..]);
                return new LogEntry(message, time, prefix, first.Pet.Name, first.Pet.GetType().Name, "", "", "", suffix);
            }

            // Two pets were found.
            PetMessageMatch second = matches[1];
            string middleText = $" {message[firstEnd..second.Index].Trim()} ";
            int secondEnd = second.Index + second.Pet.DisplayName.Length;
            string finalSuffix = FormatSuffix(message[secondEnd..]);

            return new LogEntry(
                message, time, prefix,
                first.Pet.Name, first.Pet.GetType().Name, middleText,
                second.Pet.Name, second.Pet.GetType().Name, finalSuffix);
        }


        // Controls spacing after a pet/class.
        private static string FormatSuffix(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";

            string value = text.TrimStart();
            if (value.Length == 0) return "";

            char first = value[0];

            // Punctuation must stay attached to the previous word.
            if (first is '.' or ',' or '!' or '?' or ';' or ':' or '\'') return value;

            return " " + value;
        }


        // Color for each pet class.
        private static string GetPetColor(string petType) => petType switch
        {
            "CatManager" => "#9A5B27",
            "DogIntern" => "#70568F",
            "HamsterIT" => "#9B6A18",
            "ParrotReceptionist" => "#3E7653",
            "RabbitAccountant" => "#A65376",
            "RaccoonBarista" => "#7A5C40",
            "OwlSysAdmin" => "#56638C",
            _ => "#4F5F58"
        };


        private sealed class PetMessageMatch
        {
            public OfficePet Pet { get; }
            public int Index { get; }

            public PetMessageMatch(OfficePet pet, int index)
            {
                Pet = pet;
                Index = index;
            }
        }
    }


    public sealed class LogEntryCollection : ObservableCollection<LogEntry>
    {
        // Normal message without a known pet.
        public void Insert(int index, string message)
        {
            base.Insert(index, new LogEntry(message, DateTime.Now));
        }

        // Message produced by one known pet.
        public void Insert(int index, string message, OfficePet pet)
        {
            base.Insert(index, LogEntry.FromPet(message, pet, DateTime.Now));
        }

        // Repository reaction that may contain two pets.
        public void InsertReaction(int index, string message, IEnumerable<OfficePet> pets)
        {
            base.Insert(index, LogEntry.FromReaction(message, pets, DateTime.Now));
        }
    }
}