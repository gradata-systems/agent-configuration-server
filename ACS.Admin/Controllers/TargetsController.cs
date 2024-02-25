using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using ACS.Shared;
using ACS.Shared.Models;

namespace ACS.Admin.Controllers
{
    [Authorize]
    public class TargetsController : Controller
    {
        private readonly AppDbContext _context;

        public TargetsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Targets
        public async Task<IActionResult> Index()
        {
            List <Target> targets = await _context.Targets.ToListAsync();

            // Count the number of fragments linked to each target
            foreach (Target target in targets)
            {
                target.LinkedFragments = await GetLinkedFragmentCount(target);
            }

            return View(targets);
        }

        // GET: Targets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var target = await _context.Targets.FirstOrDefaultAsync(m => m.Id == id);
            if (target == null)
            {
                return NotFound();
            }

            return View(target);
        }

        // GET: Targets/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.FragmentSelections = await GetLinkedFragments();

            return View();
        }

        // POST: Targets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Description,AgentName,AgentMinVersion,AgentMaxVersion,UserNamePattern,HostNamePattern,Enabled,LinkedFragmentIds")] Target target)
        {
            if (ModelState.IsValid)
            {
                DateTime now = DateTime.Now;
                target.Created = now;
                target.Modified = now;
                target.CreatedBy = ClaimsIdentity.FromPrincipal(HttpContext.User).Name ?? "";
                target.ModifiedBy = ClaimsIdentity.FromPrincipal(HttpContext.User).Name ?? "";
                
                _context.Add(target);
                await _context.SaveChangesAsync();

                // Associated the newly created target with the selected fragments
                await UpdateLinkedFragments(target);
                await _context.SaveChangesAsync();

                Log.Information("Created target {Target}", target);

                return RedirectToAction(nameof(Index));
            }

            ViewBag.FragmentSelections = await GetLinkedFragments(target);

            return View(target);
        }

        // GET: Targets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var target = await _context.Targets.FindAsync(id);
            if (target == null)
            {
                return NotFound();
            }

            ViewBag.FragmentSelections = await GetLinkedFragments(target);
            
            return View(target);
        }

        // POST: Targets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Description,AgentName,AgentMinVersion,AgentMaxVersion,UserNamePattern,HostNamePattern,Enabled,Created,CreatedBy,Modified,ModifiedBy,LinkedFragmentIds")] Target target)
        {
            if (id != target.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                target.Modified = DateTime.Now;
                target.ModifiedBy = ClaimsIdentity.FromPrincipal(HttpContext.User).Name ?? "";

                try
                {
                    _context.Update(target);
                    await UpdateLinkedFragments(target);
                    await _context.SaveChangesAsync();

                    Log.Information("Updated target {Target}", target);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TargetExists(target.Id))
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

            ViewBag.FragmentSelections = await GetLinkedFragments(target);

            return View(target);
        }

        // GET: Targets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var target = await _context.Targets
                .FirstOrDefaultAsync(m => m.Id == id);
            if (target == null)
            {
                return NotFound();
            }

            return View(target);
        }

        // POST: Targets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var target = await _context.Targets.FindAsync(id);
            if (target != null)
            {
                _context.Targets.Remove(target);
            }

            await _context.SaveChangesAsync();

            Log.Information("Deleted target {Target}", target);

            return RedirectToAction(nameof(Index));
        }

        private bool TargetExists(int id)
        {
            return _context.Targets.Any(e => e.Id == id);
        }

        /// <summary>
        /// Gets the number of fragments linked with the specified target
        /// </summary>
        private async Task<int> GetLinkedFragmentCount(Target target)
        {
            return await (
                from tf in _context.TargetFragments
                where tf.TargetId == target.Id
                select tf.Id
            ).CountAsync();
        }

        /// <summary>
        /// Gets a list of all fragments, including whether they are linked with the specified target
        /// </summary>
        private async Task<IEnumerable<FragmentSelection>> GetLinkedFragments(Target? target = null)
        {
            IQueryable<FragmentSelection> selectedFragments =
                from f in _context.Fragments
                select new FragmentSelection
                {
                    Fragment = f,
                    Linked = (
                        from tf in _context.TargetFragments
                        where target != null && tf.TargetId == target.Id && tf.FragmentId == f.Id
                        select tf.Id
                    ).Any()
                };

            return await selectedFragments.ToListAsync();
        }

        /// <summary>
        /// Associates fragments by their ID, with the specified target
        /// </summary>
        private async Task UpdateLinkedFragments(Target target)
        {
            await _context.TargetFragments
                .Where(f => f.TargetId == target.Id)
                .ExecuteDeleteAsync();

            if (target.LinkedFragmentIds != null)
            {
                IEnumerable<TargetFragment> targetFragments = target.LinkedFragmentIds.Select(fragmentId => new TargetFragment
                {
                    TargetId = target.Id,
                    FragmentId = fragmentId,
                    Created = DateTime.Now,
                    CreatedBy = ClaimsIdentity.FromPrincipal(HttpContext.User).Name ?? ""
            });

                await _context.TargetFragments.AddRangeAsync(targetFragments);
            }
        }
    }
}
