using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using ACS.Shared;
using ACS.Shared.Models;
using ACS.Admin.Auth;

namespace ACS.Admin.Controllers
{
    [Authorize(Roles = UserRole.ReadonlyUser)]
    public class TargetsController : Controller
    {
        private readonly AppDbContext _dbContext;

        public TargetsController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: Targets
        public async Task<IActionResult> Index()
        {
            List<Target> targets = await _dbContext.Targets.ToListAsync();

            // Count the number of fragments linked to each target
            foreach (Target target in targets)
            {
                target.LinkedFragments = await GetLinkedFragmentCount(target);
            }

            return View(targets);
        }

        // GET: Targets/Create
        [Authorize(Roles = UserRole.Administrator)]
        public async Task<IActionResult> Create()
        {
            ViewBag.FragmentSelections = await GetLinkedFragments();

            return View();
        }

        // POST: Targets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = UserRole.Administrator)]
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
                
                _dbContext.Add(target);
                await _dbContext.SaveChangesAsync();

                // Associated the newly created target with the selected fragments
                await UpdateLinkedFragments(target);
                await _dbContext.SaveChangesAsync();

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

            var target = await _dbContext.Targets.FindAsync(id);
            if (target == null)
            {
                return NotFound();
            }

            ViewBag.FragmentSelections = await GetLinkedFragments(target);

            Log.Information("Viewed target {Target}", target);
            
            return View(target);
        }

        // POST: Targets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = UserRole.Administrator)]
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
                    _dbContext.Update(target);
                    await UpdateLinkedFragments(target);
                    await _dbContext.SaveChangesAsync();

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
        [Authorize(Roles = UserRole.Administrator)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var target = await _dbContext.Targets
                .FirstOrDefaultAsync(m => m.Id == id);
            if (target == null)
            {
                return NotFound();
            }

            return View(target);
        }

        // POST: Targets/Delete/5
        [Authorize(Roles = UserRole.Administrator)]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var target = await _dbContext.Targets.FindAsync(id);
            if (target != null)
            {
                _dbContext.Targets.Remove(target);
            }

            await _dbContext.SaveChangesAsync();

            Log.Information("Deleted target {Target}", target);

            return RedirectToAction(nameof(Index));
        }

        private bool TargetExists(int id)
        {
            return _dbContext.Targets.Any(e => e.Id == id);
        }

        /// <summary>
        /// Gets the number of fragments linked with the specified target
        /// </summary>
        private async Task<int> GetLinkedFragmentCount(Target target)
        {
            return await (
                from tf in _dbContext.TargetFragments
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
                from f in _dbContext.Fragments
                select new FragmentSelection
                {
                    Fragment = f,
                    Linked = (
                        from tf in _dbContext.TargetFragments
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
            await _dbContext.TargetFragments
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

                await _dbContext.TargetFragments.AddRangeAsync(targetFragments);
            }
        }
    }
}
