using System.Collections.Generic;

namespace CinemaReservation.Models
{
    public class Hall
    {
        public int Id { get; set; }
        public int Number { get; set; }

        public int? CinemaId { get; set; }

        public virtual ICollection<Seat> Seats { get; set; }
    }
}