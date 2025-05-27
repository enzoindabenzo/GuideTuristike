using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DK1.Models.Cal
{
    public class ManageLocationsViewModel
    {
        public DK1.Models.Cal.Location NewLocation { get; set; }
        public List<DK1.Models.Cal.Location> AllLocations { get; set; }
    }

}