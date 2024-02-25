using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using ACS.Shared;
using ACS.Shared.Models;

namespace ACS.Admin.Controllers
{
    [Authorize]
    public class FragmentsController : Controller
    {
        private readonly AppDbContext _context;

        public FragmentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Fragments
        public async Task<IActionResult> Index()
        {
            List<Fragment> fragments = await _context.Fragments.ToListAsync();

            // Count the number of targets linked to each fragment
            foreach (Fragment fragment in fragments)
            {
                fragment.LinkedTargets = await GetLinkedTargetCount(fragment);
            }

            return View(fragments);
        }

        // GET: Fragments/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fragment = await _context.Fragments
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fragment == null)
            {
                return NotFound();
            }

            return View(fragment);
        }

        // GET: Fragments/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.TargetSelections = await GetLinkedTargets();

            return View();
        }

        // POST: Fragments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Description,Value,Enabled,LinkedTargetIds")] Fragment fragment)
        {
            if (ModelState.IsValid)
            {
                DateTime now = DateTime.Now;
                fragment.Created = now;
                fragment.Modified = now;
                fragment.CreatedBy = ClaimsIdentity.FromPrincipal(HttpContext.User).Name ?? "";
                fragment.ModifiedBy = ClaimsIdentity.FromPrincipal(HttpContext.User).Name ?? "";

                _context.Add(fragment);
                await _context.SaveChangesAsync();

                await UpdateLinkedTargets(fragment);
                await _context.SaveChangesAsync();

                Log.Information("Created configuration fragment {Fragment}", fragment);

                return RedirectToAction(nameof(Index));
            }

            ViewBag.TargetSelections = await GetLinkedTargets(fragment);

            return View(fragment);
        }

        // GET: Fragments/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fragment = await _context.Fragments.FindAsync(id);
            if (fragment == null)
            {
                return NotFound();
            }

            ViewBag.TargetSelections = await GetLinkedTargets(fragment);

            return View(fragment);
        }

        // POST: Fragments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Description,Value,Enabled,Created,CreatedBy,Modified,ModifiedBy,LinkedTargetIds")] Fragment fragment)
        {
            if (id != fragment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    fragment.Modified = DateTime.Now;
                    fragment.ModifiedBy = ClaimsIdentity.FromPrincipal(HttpContext.User).Name ?? "";

                    _context.Update(fragment);
                    await UpdateLinkedTargets(fragment);
                    await _context.SaveChangesAsync();

                    Log.Information("Updated configuration fragment {Fragment}", fragment);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FragmentExists(fragment.Id))
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

            ViewBag.TargetSelections = await GetLinkedTargets(fragment);

            return View(fragment);
        }

        // GET: Fragments/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fragment = await _context.Fragments
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fragment == null)
            {
                return NotFound();
            }

            return View(fragment);
        }

        // POST: Fragments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var fragment = await _context.Fragments.FindAsync(id);
            if (fragment != null)
            {
                _context.Fragments.Remove(fragment);
            }

            await _context.SaveChangesAsync();

            Log.Information("Deleted configuration fragment {Fragment}", fragment);

            return RedirectToAction(nameof(Index));
        }

        private bool FragmentExists(string id)
        {
            return _context.Fragments.Any(e => e.Id == id);
        }

        /// <summary>
        /// Gets the number of targets linked with the specified fragment
        /// </summary>
        /// <param name="fragment"></param>
        /// <returns></returns>
        private async Task<int> GetLinkedTargetCount(Fragment fragment)
        {
            return await (
                from tf in _context.TargetFragments
                where tf.FragmentId == fragment.Id
                select tf.Id
            ).CountAsync();
        }

        /// <summary>
        /// Gets a list of all fragments, including whether they are linked with the specified target
        /// </summary>
        private async Task<IEnumerable<TargetSelection>> GetLinkedTargets(Fragment? fragment = null)
        {
            IQueryable<TargetSelection> selectedTargets =
                from t in _context.Targets
                select new TargetSelection
                {
                    Target = t,
                    Linked = (
                        from tf in _context.TargetFragments
                        where fragment != null && tf.FragmentId == fragment.Id && tf.TargetId == t.Id
                        select tf.Id
                    ).Any()
                };

            return await selectedTargets.ToListAsync();
        }

        /// <summary>
        /// Associates fragments by their ID, with the specified target
        /// </summary>
        private async Task UpdateLinkedTargets(Fragment fragment)
        {
            await _context.TargetFragments
                .Where(f => f.FragmentId == fragment.Id)
                .ExecuteDeleteAsync();

            if (fragment.LinkedTargetIds != null)
            {
                IEnumerable<TargetFragment> targetFragments = fragment.LinkedTargetIds.Select(targetId => new TargetFragment
                {
                    TargetId = targetId,
                    FragmentId = fragment.Id,
                    Created = DateTime.Now,
                    CreatedBy = ClaimsIdentity.FromPrincipal(HttpContext.User).Name ?? ""
                });

                await _context.TargetFragments.AddRangeAsync(targetFragments);
            }
        }
    }
}
