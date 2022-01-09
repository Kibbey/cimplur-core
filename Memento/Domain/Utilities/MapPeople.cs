using Domain.Models;
using Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Domain.Utilities
{
    public class MapUserProfile
    {
        public static List<PersonModelV2> CurrentPeople(UserProfile userProfile)
        {
            var people = new List<PersonModelV2>();
            var currentPeople = userProfile.CurrentPeople;
            if (currentPeople != null)
            {
                people = JsonSerializer.Deserialize<List<PersonModelV2>>(currentPeople);
            }
            return people.Distinct(new PersonModelEqualityComparer()).ToList();
        }
    }
}
