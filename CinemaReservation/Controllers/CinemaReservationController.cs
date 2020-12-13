using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CinemaReservation.Data;
using CinemaReservation.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaReservation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CinemaReservationController : ControllerBase
    {
        private readonly CinemaContext _context;

        public CinemaReservationController(CinemaContext context)
        {
            _context = context;
        }

        private IQueryable<Cinema> AllCinemas
        {
            get
            {
                return _context.Cinemas
                    .Include(c => c.Halls)
                    .ThenInclude(h => h.Seats);
            }
        }

        // GET api/[controller]/cinemas
        [HttpGet("cinemas")]
        [ProducesResponseType(typeof(IEnumerable<Cinema>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Get()
        {
            var allCinemas = await AllCinemas
                .ToListAsync();

            if (allCinemas == null )
            {
                NotFound("Not found cinemas");
            }

            return Ok(allCinemas);
        }

        // GET api/[controller]/halls/{cinemaId}
        [HttpGet("halls/{cinemaId}")]
        [ProducesResponseType(typeof(IEnumerable<Hall>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetHalls(int cinemaId)
        {
            var halls = await _context.Halls.Where(h => h.CinemaId == cinemaId).ToListAsync();

            if (halls == null)
            {
                NotFound("Not found halls");
            }

            return Ok(halls);
        }

        // GET api/[controller]/seats/{cinemaId}/{hallId}
        [HttpGet("seats/{cinemaId}/{hallId}")]
        [ProducesResponseType(typeof(IEnumerable<Seat>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetSeats(int cinemaId, int hallId)
        {
            var seats = await GetSeatsByCinemaIdAndHallId(cinemaId, hallId);

            return Ok(seats);
        }

        // POST api/[controller]/{cinemaId}/{seatRow}/{seatPlace}/{seatId}
        [HttpPost("{cinemaId}/{hallId}/{seatRow}/{seatPlace}/{phone}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Reservation(int cinemaId, int hallId, int seatRow, int seatPlace, double phone)
        {
            var seats = await GetSeatsByCinemaIdAndHallId(cinemaId, hallId);
            var seat = seats.FirstOrDefault(s => s.Row == seatRow && s.Place == seatPlace);

            if (seat == null)
            {
                NotFound("Seat not exist");
            }

            if (seat.IsReserved)
            {
                BadRequest("Seat already reserved");
            }

            seat.IsReserved = true;
            seat.PlaceHolderPhone = phone;

            _context.Seats.Update(seat);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // POST api/[controller]/{cinemaId}/{hallId}/{seatId}
        [HttpGet("{cinemaId}/{hallId}/{seatId}/{phone}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Reservation(int cinemaId, int hallId, int seatId, double phone)
        {
            var seats = await GetSeatsByCinemaIdAndHallId(cinemaId, hallId);
            var seat = seats.FirstOrDefault(s => s.Id == seatId);

            if (seat == null)
            {
                NotFound("Seat not exist");
            }

            if (seat.IsReserved)
            {
                BadRequest("Seat already reserved");
            }

            seat.IsReserved = true;
            seat.PlaceHolderPhone = phone;

            _context.Seats.Update(seat);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // POST api/[controller]/{cinemaId}/{seatId}/{phone}/remove
        [HttpPost("{cinemaId}/{hallId}/{seatId}/{phone}/remove")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> RemoveReservation(int cinemaId, int hallId, int seatId, double phone)
        {
            var seats = await GetSeatsByCinemaIdAndHallId(cinemaId, hallId);
            var seat = seats.FirstOrDefault(s => s.Id == seatId);

            if (seat == null)
            {
                NotFound("Seat not exist");
            }

            if (!seat.IsReserved && seat.PlaceHolderPhone != phone)
            {
                BadRequest("Error. No rights");
            }

            seat.IsReserved = false;
            seat.PlaceHolderPhone = null;

            _context.Seats.Update(seat);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // POST api/[controller]/{cinemaId}/{seatRow}/{seatPlace}/{seatId}/remove
        [HttpPost("{cinemaId}/{hallId}/{seatRow}/{seatPlace}/{phone}/remove")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> RemoveReservation(int cinemaId, int hallId, int seatRow, int seatPlace, double phone)
        {
            var seats = await GetSeatsByCinemaIdAndHallId(cinemaId, hallId);
            var seat = seats.FirstOrDefault(s => s.Row == seatRow && s.Place == seatPlace);

            if (seat == null)
            {
                NotFound("Seat not exist");
            }

            if (!seat.IsReserved && seat.PlaceHolderPhone != phone)
            {
                BadRequest("Error. No rights");
            }

            seat.IsReserved = false;
            seat.PlaceHolderPhone = null;

            _context.Seats.Update(seat);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // GET api/[controller]/reservations/{phone}
        [HttpGet("reservations/{phone}")]
        [ProducesResponseType(typeof(IEnumerable<Cinema>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetReservations(double phone)
        {
            var reservations = await _context.Cinemas
                .Include(c => c.Halls
                    .Where(h => h.Seats.Any(s => s.PlaceHolderPhone == phone)))
                .ThenInclude(h => h.Seats
                    .Where(s => s.PlaceHolderPhone == phone))
                .ToListAsync();

            if (reservations == null)
            {
                NotFound("Reservations not found");
            }

            return Ok(reservations);
        }
        private async Task<IEnumerable<Seat>> GetSeatsByCinemaIdAndHallId(int cinemaId, int hallId)
        {
            var cinema = await AllCinemas.FirstOrDefaultAsync(c => c.Id == cinemaId);

            if (cinema == null)
            {
                NotFound("Cinema not found");
            }

            var hall = cinema.Halls.FirstOrDefault(h => h.Id == hallId);

            if (hall == null)
            {
                NotFound("Hall not found");
            }

            var seats = hall.Seats;

            if (seats == null)
            {
                NotFound("Seats not found");
            }

            return seats;
        }
    }
}