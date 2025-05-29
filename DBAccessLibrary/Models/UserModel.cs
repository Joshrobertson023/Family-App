using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBAccessLibrary.Models
{
    public class UserModel
    {
        private int id;
        private string username;
        private string firstName;
        private string lastName;
        private string? email;
        private string passwordHash;
        private DateTime dateRegistered;
        private DateTime lastSeen;
        private string? description;
        private string? lastReadPassage;
        private int? currentReadingPlan;
        private int? lastPracticedVerse;
        private int isKidsAccount;
        private int isDeleted;
        private string? reasonDeleted;
        private int appTheme;
        private int showVersesSaved;
        private int showPopularHighlights;
        private int flagged;
        private int allowPushNotifications;
        private int followVerseOfTheDay;
        private int visibility;

        public int Id
        {
            get { return id; }
            set
            {
                if (value < 0 || value > 9999999999)
                    throw new ArgumentException("Critical error setting id: Value was outside the allowed range.");

                id = value;
            }
        }
        public string Username
        {
            get { return "@" + username; }
            set
            {
                if (value.Length > UsernameMax)
                    throw new ArgumentException($"Username is too long. Please enter a username under {UsernameMax + 1} characters.");

                username = value;
            }
        }
        public int UsernameMax { get { return 18; } }
        public string FirstName
        {
            get { return firstName; }
            set
            {
                if (value.Length > NameMax)
                    throw new ArgumentException($"First name is too long. Please enter a name under {NameMax + 1} characters.");

                firstName = value;
            }
        }
        public int NameMax { get { return 15; } }
        public string LastName
        {
            get { return lastName; }
            set
            {
                if (value.Length > NameMax)
                    throw new ArgumentException($"Last name is too long. Please enter a name under {NameMax + 1} characters");

                lastName = value;
            }
        }
        public string FullName
        {
            get 
            {
                return (firstName.Substring(0, 1).ToUpper() + firstName.Substring(1) + " " 
                        + lastName.Substring(0, 1).ToUpper() + lastName.Substring(1));
            }
        }
        public string? Email
        { 
            get
            {
                if (string.IsNullOrEmpty(email))
                    throw new NullReferenceException("Error getting Email: email is null or empty.");
                else
                    return email;
            }
            set
            {
                if (value.Length > EmailMax)
                    throw new ArgumentException($"Email is too long. Please enter an email under {EmailMax + 1} characters.");

                email = value;
            }
        }
        public int EmailMax { get { return 50; } }
        public string PasswordHash
        {
            get { return passwordHash; }
            set
            {
                if (value.Length > PasswordMax)
                    throw new ArgumentException($"Critical error setting PasswordHash: PasswordHash is too long.");

                passwordHash = value;
            }
        }
        public int PasswordMax { get { return 128; } }
        public DateTime DateRegistered
        {
            get { return dateRegistered; }
            set { dateRegistered = value; }
        }
        public DateTime LastSeen
        {
            get { return lastSeen; }
            set { lastSeen = value; }
        }
        public string? Description
        {
            get
            {
                if (string.IsNullOrEmpty(description))
                    throw new NullReferenceException("Error getting Description: description is null or empty.");
                else
                    return description;
            }
            set
            {
                if (value.Length > DescriptionMax)
                    throw new ArgumentException($"description is too long. Please enter a description under {DescriptionMax + 1} characters.");

                description = value;
            }
        }
        public int DescriptionMax { get { return 50; } }
        public string? LastReadPassage
        {
            get
            {
                if (string.IsNullOrEmpty(lastReadPassage))
                    throw new NullReferenceException("Error getting lastReadPassage: lastReadPassage was null or empty.");
                else
                    return lastReadPassage;
            }
            set
            {
                if (value.Length > LastReadPassageMax)
                    throw new ArgumentException($"Critical error setting LastReadPassage: value is too long.");

                lastReadPassage = value;
            }
        }
        public int LastReadPassageMax { get { return 10; } }
        public int? CurrentReadingPlan
        {
            get
            {
                if (currentReadingPlan == null)
                    throw new NullReferenceException("Error getting currentReadingPlan: value was null.");
                else
                    return currentReadingPlan;
            }
            set
            {
                if (value < 0 || value > 9999999999)
                    throw new ArgumentException("Critical error setting currentReadingPlan: Value was outside the allowed range.");

                currentReadingPlan = value;
            }
        }
        public int? LastPracticedVerse
        {
            get
            {
                if (lastPracticedVerse == null)
                    throw new NullReferenceException("Error getting lastPracticedVerse: value was null.");
                else
                    return lastPracticedVerse;
            }
            set
            {
                if (value < 0 || value > 9999999999)
                    throw new ArgumentException("Critical error setting lastPracticedVerse: Value was outside the allowed range.");

                lastPracticedVerse = value;
            }
        }
        public int IsKidsAccount
        {
            get { return isKidsAccount; }
            set
            {
                if (value < 0 || value > 1)
                    throw new ArgumentException("Critical error setting isKidsAccount: value was outside the allowed range.");

                isKidsAccount = value;
            }
        }
        public int IsDeleted
        {
            get { return isDeleted; }
            set
            {
                if (value < 0 || value > 1)
                    throw new ArgumentException("Critical error setting isDeleted: value was outside the allowed range.");

                isDeleted = value;
            }
        }
        public string? ReasonDeleted
        {
            get
            {
                if (string.IsNullOrEmpty(reasonDeleted))
                    throw new NullReferenceException("Error getting reasonDeleted: reasonDeleted was null or empty.");
                else
                    return reasonDeleted;
            }
            set
            {
                if (value.Length > ReasonDeletedMax)
                    throw new ArgumentException($"Critical error setting reasonDeleted: value is too long.");

                reasonDeleted = value;
            }
        }
        public int ReasonDeletedMax { get { return 100; } }
        public int AppTheme
        {
            get { return appTheme; }
            set
            {
                if (value < 0 || value > 1)
                    throw new ArgumentException("Critical error setting appTheme: value was outside the allowed range.");

                appTheme = value;
            }
        }
        public int ShowVersesSaved
        {
            get { return showVersesSaved; }
            set
            {
                if (value < 0 || value > 1)
                    throw new ArgumentException("Critical error setting showVersesSaved: value was outside the allowed range.");

                showVersesSaved = value;
            }
        }
        public int ShowPopularHighlights
        {
            get { return showPopularHighlights; }
            set
            {
                if (value < 0 || value > 1)
                    throw new ArgumentException("Critical error setting showPopularHighlights: value was outside the allowed range.");

                showPopularHighlights = value;
            }
        }
        public int Flagged
        {
            get { return flagged; }
            set
            {
                if (value < 0 || value > 1)
                    throw new ArgumentException("Critical error setting flagged: value was outside the allowed range.");

                flagged = value;
            }
        }
        public int AllowPushNotifications
        {
            get { return allowPushNotifications; }
            set
            {
                if (value < 0 || value > PushNotificationTypes)
                    throw new ArgumentException("Critical error setting allowPushNotifications: value was outside the allowed range.");

                allowPushNotifications = value;
            }
        }
        public int PushNotificationTypes { get { return 2; } }
        public int FollowVerseOfTheDay
        {
            get { return followVerseOfTheDay; }
            set
            {
                if (value < 0 || value > 1)
                    throw new ArgumentException("Critical error setting followVerseOfTheDay: value was outside the allowed range.");

                followVerseOfTheDay = value;
            }
        }
        public int Visibility
        {
            get { return visibility; }
            set
            {
                if (value < 0 || value > VisibilityTypes)
                    throw new ArgumentException("Critical error setting visibility: value was outside the allowed range.");

                visibility = value;
            }
        }
        public int VisibilityTypes { get { return 3; } }
    }
}
