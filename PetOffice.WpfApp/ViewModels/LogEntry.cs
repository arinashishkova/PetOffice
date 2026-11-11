using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PetOffice.Core.Models;

namespace PetOffice.WpfApp.ViewModels
{
    public sealed class LogEntry
    {
        // Original message. Kept for compatibility with the current UI.
        public string Message { get; }
        public DateTime Time { get; }
        public string TimeText => Time.ToString("HH:mm");

        // Text before the first pet. Example: "Added: "
        public string Prefix { get; }

        // First pet.
        public string PetName { get; }
        public string PetType { get; }
        public string PetTypeText => string.IsNullOrWhiteSpace(PetType) ? "" : $"({PetType})";

        // Text between two pets. Example: " welcomes "
        public string MiddleText { get; }

        // Optional second pet.
        public string SecondPetName { get; }
        public string SecondPetType { get; }
        public string SecondPetTypeText => string.IsNullOrWhiteSpace(SecondPetType) ? "" : $"({SecondPetType})";

        // Text after the last pet.
        public string Suffix { get; }

        // Colors used later by the Activity Log XAML.
        public string PetColor => GetPetColor(PetType);
        public string PetBackgroundColor => GetPetBackgroundColor(PetType);
        public string SecondPetColor => GetPetColor(SecondPetType);
        public string SecondPetBackgroundColor => GetPetBackgroundColor(SecondPetType);

        // True when the entry contains structured pet information.
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

        // Creates a structured entry when one known pet caused the message.
        public static LogEntry FromPet(string message, OfficePet pet, DateTime time)
        {
            string type = pet.GetType().Name;

            // Some messages already contain: "Name - Class"
            int displayNameIndex = message.IndexOf(pet.DisplayName, StringComparison.Ordinal);

            if (displayNameIndex >= 0)
            {
                string prefix = message[..displayNameIndex];
                string suffix = message[(displayNameIndex + pet.DisplayName.Length)..];
                return new LogEntry(message, time, prefix, pet.Name, type, "", "", "", suffix);
            }

            // Other pet actions contain only the pet name.
            int nameIndex = message.IndexOf(pet.Name, StringComparison.Ordinal);

            if (nameIndex >= 0)
            {
                string prefix = message[..nameIndex];
                string suffix = message[(nameIndex + pet.Name.Length)..];
                return new LogEntry(message, time, prefix, pet.Name, type, "", "", "", suffix);
            }

            // If the message does not contain the name, keep it as a normal log entry.
            return new LogEntry(message, time);
        }

        // Reaction messages can contain two pets (e.g. "Meow - CatManager welcomes Leonard - CatManager.").
        // This keeps both pets separate for UI formatting.
        public static LogEntry FromReaction(string message, IEnumerable<OfficePet> pets, DateTime time)
        {
            List<PetMessageMatch> matches = pets
                .Select(pet => new PetMessageMatch(pet, message.IndexOf(pet.DisplayName, StringComparison.Ordinal)))
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
                string suffix = message[firstEnd..];
                return new LogEntry(message, time, prefix, first.Pet.Name, first.Pet.GetType().Name, "", "", "", suffix);
            }

            // Two pets were found.
            PetMessageMatch second = matches[1];
            string middleText = message[firstEnd..second.Index].Trim();
            int secondEnd = second.Index + second.Pet.DisplayName.Length;
            string finalSuffix = message[secondEnd..].TrimStart();

            return new LogEntry(
                message, time, prefix,
                first.Pet.Name, first.Pet.GetType().Name, middleText,
                second.Pet.Name, second.Pet.GetType().Name, finalSuffix);
        }

        // Darker color for the pet name / class text.
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

        // Same pastel palette used for the animal cards.
        private static string GetPetBackgroundColor(string petType) => petType switch
        {
            "CatManager" => "#FFF4E8",
            "DogIntern" => "#F5F0FA",
            "HamsterIT" => "#FFF8EC",
            "ParrotReceptionist" => "#EFF8F2",
            "RabbitAccountant" => "#FDEFF5",
            "RaccoonBarista" => "#FBEFE3",
            "OwlSysAdmin" => "#EEF0F8",
            _ => "#F4F6F5"
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