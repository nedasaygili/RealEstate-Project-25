using System.Collections.Generic;

namespace RealEstateSite.Models.ViewModels
{
    public class HomeViewModel
    {
        public List<Property> LatestProperties { get; set; } = new List<Property>();
        public List<Agent> FeaturedAgents { get; set; } = new List<Agent>();
    }
}