using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBAccessLibrary.Models
{
    public class CurrentUser
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

        public string FullName
        {
            get
            {
                return (FirstName.Substring(0, 1).ToUpper() + FirstName.Substring(1) + " "
                        + LastName.Substring(0, 1).ToUpper() + LastName.Substring(1));
            }
        }
    }
}
