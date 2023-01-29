using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TripsApi.Models;

namespace TripsApi.Controllers
{
    [ApiController]
    [Route("api/clients")]
    public class ClientsController : Controller
    {
        private S21482Context _context;

        public ClientsController(S21482Context context)
        {
            _context = context;
        }

        [HttpDelete("{idClient}")]
        public async Task<ActionResult<IEnumerable<Trip>>> DeleteClient(int idClient)
        {
            if (await _context.ClientTrips.AnyAsync(ct => ct.IdClient == idClient))
            {
                return BadRequest("Client is registered to a trip");
            }

            var client = await _context.Clients.FindAsync(idClient);

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
