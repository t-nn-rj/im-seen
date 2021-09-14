﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IMWebAPI.Data;
using IMWebAPI.Models;
using IMWebAPI.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IMWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardsController : ControllerBase
    {
        private readonly IM_API_Context _context;

        public DashboardsController(IM_API_Context context)
        {
            _context = context;
        }

        // GET: api/Dashboards
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Dashboard>>> GetDashboard()
        {
            return await _context.Dashboards.ToListAsync();
        }

        // GET: api/Dashboards/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Dashboard>> GetDashboard(int id)
        {
            var dashboard = await _context.Dashboards.FindAsync(id);

            if (dashboard == null)
            {
                return NotFound();
            }

            return dashboard;
        }

        // PUT: api/Dashboards/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDashboard(int id, Dashboard dashboard)
        {
            if (id != dashboard.DashboardID)
            {
                return BadRequest();
            }

            _context.Entry(dashboard).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DashboardExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Dashboards
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Dashboard>> PostDashboard(Dashboard dashboard)
        {
            _context.Dashboards.Add(dashboard);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDashboard", new { id = dashboard.DashboardID }, dashboard);
        }

        // DELETE: api/Dashboards/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Dashboard>> DeleteDashboard(int id)
        {
            var dashboard = await _context.Dashboards.FindAsync(id);
            if (dashboard == null)
            {
                return NotFound();
            }

            _context.Dashboards.Remove(dashboard);
            await _context.SaveChangesAsync();

            return dashboard;
        }

        private bool DashboardExists(int id)
        {
            return _context.Dashboards.Any(e => e.DashboardID == id);
        }
    }
}
