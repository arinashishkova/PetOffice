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

        // First pet or grouped pet names.
        public string PetName { get; }
        public string PetType { get; }
        public string PetTypeText => string.IsNullOrWhiteSpace(PetType) ? "" : $" ({PetType})";

        // Text between pet groups. Example: " welcomes " or " welcome "
        public string MiddleText { get; }

        // Second pet.
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


        // Grouped repository reaction.
        // Example: Leonard, Meow (CatManager) welcome Bob (CatManager).
        public static LogEntry FromReactionGroup(
            OfficePet source,
            IEnumerable<OfficePet> reactors,
            ReactionKind kind,
            DateTime time)
        {
            List<OfficePet> reactorList = reactors.ToList();

            if (reactorList.Count == 0)
            {
                return new LogEntry("No reactions.", time);
            }

            string reactorNames = string.Join(", ", reactorList.Select(pet => pet.Name));
            string reactorType = reactorList[0].GetType().Name;
            string sourceType = source.GetType().Name;
            bool multiple = reactorList.Count > 1;

            string middleText;
            string suffix;

            switch (kind)
            {
                case ReactionKind.Welcome:
                    middleText = multiple ? " welcome " : " welcomes ";
                    suffix = ".";
                    break;

                case ReactionKind.Goodbye:
                    middleText = multiple ? " say goodbye to " : " says goodbye to ";
                    suffix = ".";
                    break;

                case ReactionKind.CrazyAction:
                    middleText = multiple ? " react to " : " reacts to ";
                    suffix = "'s crazy action.";
                    break;

                default:
                    middleText = multiple ? " react to " : " reacts to ";
                    suffix = ".";
                    break;
            }

            string message = $"{reactorNames} ({reactorType}){middleText}{source.Name} ({sourceType}){suffix}";

            return new LogEntry(
                message, time, "",
                reactorNames, reactorType, middleText,
                source.Name, sourceType, suffix);
        }


        // Controls spacing after a pet/class.
        private static string FormatSuffix(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";

            string value = text.TrimStart();
            if (value.Length == 0) return "";

            char first = value[0];

            // Punctuation stays attached to the previous word.
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

        // Grouped repository reaction.
        public void InsertReactionGroup(int index, OfficePet source, IEnumerable<OfficePet> reactors, ReactionKind kind)
        {
            base.Insert(index, LogEntry.FromReactionGroup(source, reactors, kind, DateTime.Now));
        }
    }
}