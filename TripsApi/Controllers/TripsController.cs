using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TripsApi.Models;
using TripsApi.Models.DTO.Request;
using TripsApi.Models.DTO.Response;

namespace TripsApi.Controllers
{
    [ApiController]
    [Route("api/trips")]
    public class TripsController : ControllerBase
    {
        private S21482Context _context;

        public TripsController(S21482Context context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Trip>>> GetTrips()
        {
            return Ok(await _context.Trips.Include(t => t.ClientTrips).Include(t => t.IdCountries).Select(t => new TripDTO
            {
                Name = t.Name,
                Description = t.Description,
                DateFrom = t.DateFrom,
                DateTo = t.DateTo,
                MaxPeople = t.MaxPeople,
                Clients = t.ClientTrips.Select(ct => new ClientDTO
                {
                    FirstName = ct.IdClientNavigation.FirstName,
                    LastName = ct.IdClientNavigation.LastName,
                }).ToList(),
                Countries = t.IdCountries.Select(c => new CountryDTO
                {
                    Name = c.Name,
                }).ToList(),
            }).OrderByDescending(t => t.DateFrom).ToListAsync());
        }

        [HttpPost("{idTrip}/clients")]
        public async Task<ActionResult<Client>> PostClient(int idTrip, ClientTripDTO clientTrip)
        {

            var newClient = await _context.Clients.Where(c => c.Pesel == clientTrip.Pesel).FirstOrDefaultAsync();

            Console.WriteLine("tutaj jestesm SSADASDASD");


            if (newClient == null)
            {
                newClient = new Client
                {
                    FirstName = clientTrip.FirstName,
                    LastName = clientTrip.LastName,
                    Email = clientTrip.Email,
                    Telephone = clientTrip.Telephone,
                    Pesel = clientTrip.Pesel,
                };
                await _context.Clients.AddAsync(newClient);

            }

            var isAssigned = await _context.ClientTrips.AnyAsync(ct => ct.IdClient == newClient.IdClient && ct.IdTrip == idTrip);
            if (isAssigned) return BadRequest("Client already has a trip assigned.");

            var trip = await _context.Trips.Where(t => t.IdTrip == idTrip).FirstOrDefaultAsync();

            if (trip == null)
            {
                return BadRequest("Trip doesn't exist.");
            }

            var newClientTrip = new ClientTrip
            {
                IdClientNavigation = newClient,
                IdTrip = idTrip,
                IdClient = newClient.IdClient,
                IdTripNavigation = trip,
                PaymentDate = clientTrip.PaymentDate,
                RegisteredAt = DateTime.Now
            };

            await _context.ClientTrips.AddAsync(newClientTrip);
            newClient.ClientTrips.Add(newClientTrip);
            trip.ClientTrips.Add(newClientTrip);
            await _context.SaveChangesAsync();

            return Ok();

        }

    };




}