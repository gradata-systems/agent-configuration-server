using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using ACS.Shared;
using ACS.Shared.Models;
using ACS.Admin.Auth;
using static System.Net.Mime.MediaTypeNames;
using System.Text.Json;

namespace ACS.Admin.Controllers
{
    [Authorize]
    public class TargetsController : Controller
    {
        private readonly AppDbContext _dbContext;

        public TargetsController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: Targets
        [Authorize(Roles = UserRole.Administrator + "," + UserRole.Editor + "," + UserRole.ReadonlyUser)]
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
        [Authorize(Roles = UserRole.Administrator + "," + UserRole.Editor)]
        public async Task<IActionResult> Create()
        {
            ViewBag.FragmentSelections = await GetLinkedFragments();

            return View();
        }

        // POST: Targets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = UserRole.Administrator + "," + UserRole.Editor)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Description,AgentName,AgentMinVersion,AgentMaxVersion,UserNamePattern,ActiveUserNamePattern,HostNamePattern,HostIpv4Cidr,HostRolePattern,EnvironmentNamePattern,Enabled,LinkedFragmentIds")] Target target)
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
        [Authorize(Roles = UserRole.Administrator + "," + UserRole.Editor + "," + UserRole.ReadonlyUser)]
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
        [Authorize(Roles = UserRole.Administrator + "," + UserRole.Editor)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Description,AgentName,AgentMinVersion,AgentMaxVersion,UserNamePattern,ActiveUserNamePattern,HostNamePattern,HostIpv4Cidr,HostRolePattern,EnvironmentNamePattern,Enabled,Created,CreatedBy,Modified,ModifiedBy,LinkedFragmentIds")] Target target)
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
        [Authorize(Roles = UserRole.Administrator + "," + UserRole.Editor)]
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
        [Authorize(Roles = UserRole.Administrator + "," + UserRole.Editor)]
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

        [Authorize(Roles = UserRole.Administrator)]
        [HttpGet]
        public async Task<IActionResult> Import()
        {
            return View();
        }

        [Authorize(Roles = UserRole.Administrator)]
        [HttpPost, ActionName("Import")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportFromFile(IFormFile file)
        {
            if (file == default || file.Length == 0)
            {
                return BadRequest("No file provided");
            }

            string fileName = Path.GetFileName(file.FileName);
            if (fileName == null || Path.GetExtension(fileName).ToLowerInvariant() != ".json")
            {
                return BadRequest("Unacceptable file extension");
            }

            try
            {
                // Read JSON file from form data
                using MemoryStream memoryStream = new();
                await file.CopyToAsync(memoryStream);
                List<Target>? targets = JsonSerializer.Deserialize<List<Target>>(memoryStream.ToArray());

                if (targets?.Count > 0)
                {
                    await _dbContext.Targets.AddRangeAsync(targets.Select(target =>
                    {
                        target.Id = default;
                        target.Created = DateTime.Now;
                        target.CreatedBy = ClaimsIdentity.FromPrincipal(HttpContext.User).Name ?? "";
                        target.Modified = DateTime.Now;
                        target.ModifiedBy = ClaimsIdentity.FromPrincipal(HttpContext.User).Name ?? "";

                        return target;
                    }));

                    await _dbContext.SaveChangesAsync();

                    Log
                        .ForContext("Targets", targets)
                        .Information("Imported {TargetCount} targets from file {FileName}", targets.Count, fileName);
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to import targets from file");
                return BadRequest($"Invalid targets in JSON file '{fileName}': {ex.Message}");
            }
        }

        [Authorize(Roles = UserRole.Administrator)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Export(List<int> targetIds)
        {
            var targets = await (from f in _dbContext.Targets
                                 where targetIds.Count == 0 || targetIds.Contains(f.Id)
                                 select f).ToListAsync();

            // Return the target list as a file download, containing the serialised JSON string
            byte[] json = JsonSerializer.SerializeToUtf8Bytes(targets);

            Log
                .ForContext("Targets", targets)
                .Information("Exported {TargetCount} targets to file", targets.Count);

            return new FileContentResult(json, Application.Json)
            {
                FileDownloadName = "targets.json"
            };
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
                orderby f.Name, f.Priority descending
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
