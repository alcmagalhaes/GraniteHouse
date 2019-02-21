using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraniteHouse.Models.ViewModels
{
    public class AppointmentsViewModel
    {
        public List<Appointments> Appointments { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}
