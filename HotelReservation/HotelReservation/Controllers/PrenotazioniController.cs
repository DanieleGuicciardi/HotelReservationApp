using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HotelReservation.Data;
using HotelReservation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace HotelReservation.Controllers
{
    [Authorize]
    public class PrenotazioniController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PrenotazioniController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Prenotazioni
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(user);

            var prenotazioni = _context.Prenotazioni
                .Include(p => p.Camera)
                .Include(p => p.Cliente)
                .AsQueryable();

            // Se è cliente, mostra solo le sue prenotazioni
            if (userRoles.Contains("Cliente"))
            {
                var cliente = await _context.Clienti.FirstOrDefaultAsync(c => c.Email == user.Email);
                if (cliente != null)
                {
                    prenotazioni = prenotazioni.Where(p => p.ClienteId == cliente.ClienteId);
                }
            }

            return View(await prenotazioni.ToListAsync());
        }

        private readonly UserManager<ApplicationUser> _userManager;

public PrenotazioniController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
{
    _context = context;
    _userManager = userManager;
}

        // GET: Prenotazioni/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prenotazione = await _context.Prenotazioni
                .Include(p => p.Camera)
                .Include(p => p.Cliente)
                .FirstOrDefaultAsync(m => m.PrenotazioneId == id);
            if (prenotazione == null)
            {
                return NotFound();
            }

            return View(prenotazione);
        }

        // GET: Prenotazioni/Create
        public async Task<IActionResult> Create()
        {
            var camereDisponibili = _context.Camere
                .Where(c => !_context.Prenotazioni.Any(p =>
                    p.CameraId == c.CameraId &&
                    (p.DataInizio <= DateTime.Now && p.DataFine >= DateTime.Now)))
                .ToList();

            var user = await _userManager.GetUserAsync(User);
            var cliente = await _context.Clienti.FirstOrDefaultAsync(c => c.Email == user.Email);

            ViewData["ClienteId"] = new SelectList(new[] { cliente }, "ClienteId", "Nome");
            ViewData["CameraId"] = new SelectList(camereDisponibili, "CameraId", "Numero");

            return View();
        }

        // POST: Prenotazioni/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClienteId,CameraId,DataInizio,DataFine,Stato")] Prenotazione prenotazione)
        {
            if (ModelState.IsValid)
            {
                _context.Add(prenotazione);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClienteId"] = new SelectList(_context.Clienti, "ClienteId", "Nome", prenotazione.ClienteId);
            ViewData["CameraId"] = new SelectList(_context.Camere, "CameraId", "Numero", prenotazione.CameraId);
            return View(prenotazione);
        }

        // GET: Prenotazioni/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prenotazione = await _context.Prenotazioni.FindAsync(id);
            if (prenotazione == null)
            {
                return NotFound();
            }
            ViewData["CameraId"] = new SelectList(_context.Camere, "CameraId", "CameraId", prenotazione.CameraId);
            ViewData["ClienteId"] = new SelectList(_context.Clienti, "ClienteId", "ClienteId", prenotazione.ClienteId);
            return View(prenotazione);
        }

        // POST: Prenotazioni/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PrenotazioneId,ClienteId,CameraId,DataInizio,DataFine,Stato")] Prenotazione prenotazione)
        {
            if (id != prenotazione.PrenotazioneId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(prenotazione);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PrenotazioneExists(prenotazione.PrenotazioneId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CameraId"] = new SelectList(_context.Camere, "CameraId", "CameraId", prenotazione.CameraId);
            ViewData["ClienteId"] = new SelectList(_context.Clienti, "ClienteId", "ClienteId", prenotazione.ClienteId);
            return View(prenotazione);
        }

        // GET: Prenotazioni/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prenotazione = await _context.Prenotazioni
                .Include(p => p.Camera)
                .Include(p => p.Cliente)
                .FirstOrDefaultAsync(m => m.PrenotazioneId == id);
            if (prenotazione == null)
            {
                return NotFound();
            }

            return View(prenotazione);
        }

        // POST: Prenotazioni/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var prenotazione = await _context.Prenotazioni.FindAsync(id);
            if (prenotazione != null)
            {
                _context.Prenotazioni.Remove(prenotazione);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PrenotazioneExists(int id)
        {
            return _context.Prenotazioni.Any(e => e.PrenotazioneId == id);
        }

        [HttpPost]
        public async Task<IActionResult> SaveAjax(Prenotazione prenotazione)
        {
            if (ModelState.IsValid)
            {
                if (prenotazione.PrenotazioneId == 0)
                {
                    _context.Prenotazioni.Add(prenotazione);
                }
                else
                {
                    _context.Prenotazioni.Update(prenotazione);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }

            return PartialView("_PrenotazioneForm", prenotazione);
        }
    }
}
