namespace CinemaReservation.Models
{
    public class Seat
    {
        public int Id { get; set; }
        public int Row { get; set; }
        public int Place { get; set; }
        public bool IsReserved { get; set; }
        public double? PlaceHolderPhone { get; set; }

        public int? HallId { get; set; }
    }
}