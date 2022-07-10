using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AirportTrafficControlTower.Data.Contexts;
using AirportTrafficControlTower.Data.Model;
using AirportTrafficControlTower.Service;

namespace AirportTrafficControlTower.Manager.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]
    //public class StationController : ControllerBase
    //{
    //    private readonly BusinessService _businessService;

    //    public StationController(BusinessService businessService)
    //    {
    //        _businessService = businessService;
    //    }

    //    // GET: api/Station
    //    [HttpGet]
    //    public async Task<ActionResult<IEnumerable<Station>>> GetStations()
    //    {
    //      if ( == null)
    //      {
    //          return NotFound();
    //      }
    //        return await _context.Stations.ToListAsync();
    //    }

    //    // GET: api/Station/5
    //    [HttpGet("{id}")]
    //    public async Task<ActionResult<Station>> GetStation(int id)
    //    {
    //      if (_context.Stations == null)
    //      {
    //          return NotFound();
    //      }
    //        var station = await _context.Stations.FindAsync(id);

    //        if (station == null)
    //        {
    //            return NotFound();
    //        }

    //        return station;
    //    }

    //    // PUT: api/Station/5
    //    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    //    [HttpPut("{id}")]
    //    public async Task<IActionResult> PutStation(int id, Station station)
    //    {
    //        if (id != station.StationId)
    //        {
    //            return BadRequest();
    //        }

    //        _context.Entry(station).State = EntityState.Modified;

    //        try
    //        {
    //            await _context.SaveChangesAsync();
    //        }
    //        catch (DbUpdateConcurrencyException)
    //        {
    //            if (!StationExists(id))
    //            {
    //                return NotFound();
    //            }
    //            else
    //            {
    //                throw;
    //            }
    //        }

    //        return NoContent();
    //    }

    //    // POST: api/Station
    //    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    //    [HttpPost]
    //    public async Task<ActionResult<Station>> PostStation(Station station)
    //    {
    //      if (_context.Stations == null)
    //      {
    //          return Problem("Entity set 'AirPortTrafficControlContext.Stations'  is null.");
    //      }
    //        _context.Stations.Add(station);
    //        await _context.SaveChangesAsync();

    //        return CreatedAtAction("GetStation", new { id = station.StationId }, station);
    //    }

    //    // DELETE: api/Station/5
    //    [HttpDelete("{id}")]
    //    public async Task<IActionResult> DeleteStation(int id)
    //    {
    //        if (_context.Stations == null)
    //        {
    //            return NotFound();
    //        }
    //        var station = await _context.Stations.FindAsync(id);
    //        if (station == null)
    //        {
    //            return NotFound();
    //        }

    //        _context.Stations.Remove(station);
    //        await _context.SaveChangesAsync();

    //        return NoContent();
    //    }

    //    private bool StationExists(int id)
    //    {
    //        return (_context.Stations?.Any(e => e.StationId == id)).GetValueOrDefault();
    //    }
    //}
}
