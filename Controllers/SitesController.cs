using System.Security.Policy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Real_Estate_Services.db;
using Real_Estate_Services.Models;

namespace Real_Estate_Services.Controllers
{
    public class SitesController : Controller
    {
        private readonly DB _context;
        private readonly IWebHostEnvironment _env;

        // Limits
        //image
        private const long MaxImageSizeBytes = 5 * 1024 * 1024;     // 5 MB per 

        //brochure
        private const long MaxBrochureSizeBytes = 10 * 1024 * 1024; // 10 MB for 

        private static readonly string[] AllowedBrochureContentTypes = new[]
        {
            "application/pdf",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
        };

        public SitesController(DB context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Sites
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var list = await _context.Sites.AsNoTracking().ToListAsync();
            return View(list);
        }

        // GET: Sites/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var site = await _context.Sites.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id.Value);
            if (site == null) return NotFound();

            return View(site);
        }

        // GET: Sites/Create
        [Authorize]
        public IActionResult Create()
        {
           return View();
        }

        // POST: Sites/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Sites model)
        {
            // basic upload validation
            if (model.ImageFiles != null && model.ImageFiles.Count > 7)
                ModelState.AddModelError(nameof(model.ImageFiles), "You can upload up to 7 images.");

            if (model.ImageFiles != null)
            {
                foreach (var f in model.ImageFiles)
                {
                    if (!IsImage(f))
                    {
                        ModelState.AddModelError(nameof(model.ImageFiles), "Only image files are allowed.");
                        break;
                    }
                    if (f.Length > MaxImageSizeBytes)
                    {
                        ModelState.AddModelError(nameof(model.ImageFiles), $"Each image must be <= {MaxImageSizeBytes / (1024 * 1024)} MB.");
                        break;
                    }
                }
            }

            if (model.BrochureFile != null)
            {
                if (!IsBrochure(model.BrochureFile))
                {
                    ModelState.AddModelError(nameof(model.BrochureFile), "Only PDF/DOC/DOCX files are allowed for brochure.");
                }
                else if (model.BrochureFile.Length > MaxBrochureSizeBytes)
                {
                    ModelState.AddModelError(nameof(model.BrochureFile), $"Brochure must be <= {MaxBrochureSizeBytes / (1024 * 1024)} MB.");
                }
            }

            if (!ModelState.IsValid) return View(model);

            // create record to get Id
            var site = new Sites
            {
                SiteName = model.SiteName,
                Category = model.Category,
                Address = model.Address,
                Description = model.Description
            };

            _context.Sites.Add(site);
            await _context.SaveChangesAsync(); // now site.Id is available

            // prepare upload folder
            var uploadFolder = Path.Combine(_env.WebRootPath, "uploads", "sites", site.Id.ToString());
            Directory.CreateDirectory(uploadFolder);

            // Save image files (sequentially into Image1..Image7)
            if (model.ImageFiles != null && model.ImageFiles.Any())
            {
                var files = model.ImageFiles.Take(7).ToList();
                for (int i = 0; i < files.Count; i++)
                {
                    var rel = await SaveFileAsync(files[i], uploadFolder, site.Id);
                    SetImagePathByIndex(site, i + 1, rel);
                }
            }

            // Save brochure if provided
            if (model.BrochureFile != null)
            {
                var broRel = await SaveFileAsync(model.BrochureFile, uploadFolder, site.Id);
                site.Brochure = broRel;
            }

            _context.Update(site);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Site created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Sites/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var site = await _context.Sites.FindAsync(id.Value);
            if (site == null) return NotFound();

            return View(site);
        }

        // POST: Sites/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Sites model)
        {
            if (id != model.Id) return BadRequest();

            var site = await _context.Sites.FindAsync(id);
            if (site == null) return NotFound();

            if (model.ImageFiles != null && model.ImageFiles.Count > 7)
                ModelState.AddModelError(nameof(model.ImageFiles), "You can upload up to 7 images.");

            if (model.ImageFiles != null)
            {
                foreach (var f in model.ImageFiles)
                {
                    if (!IsImage(f))
                    {
                        ModelState.AddModelError(nameof(model.ImageFiles), "Only image files are allowed.");
                        break;
                    }
                    if (f.Length > MaxImageSizeBytes)
                    {
                        ModelState.AddModelError(nameof(model.ImageFiles), $"Each image must be <= {MaxImageSizeBytes / (1024 * 1024)} MB.");
                        break;
                    }
                }
            }

            if (model.BrochureFile != null)
            {
                if (!IsBrochure(model.BrochureFile))
                {
                    ModelState.AddModelError(nameof(model.BrochureFile), "Only PDF/DOC/DOCX files are allowed for brochure.");
                }
                else if (model.BrochureFile.Length > MaxBrochureSizeBytes)
                {
                    ModelState.AddModelError(nameof(model.BrochureFile), $"Brochure must be <= {MaxBrochureSizeBytes / (1024 * 1024)} MB.");
                }
            }

            if (!ModelState.IsValid) return View(model);

            // update text fields
            site.SiteName = model.SiteName;
            site.Category = model.Category;
            site.Address = model.Address;
            site.Description = model.Description;

            var uploadFolder = Path.Combine(_env.WebRootPath, "uploads", "sites", site.Id.ToString());
            Directory.CreateDirectory(uploadFolder);

            // Replace images sequentially if new files uploaded
            if (model.ImageFiles != null && model.ImageFiles.Any())
            {
                var files = model.ImageFiles.Take(7).ToList();
                for (int i = 0; i < files.Count; i++)
                {
                    var existingRel = GetImagePathByIndex(site, i + 1);
                    if (!string.IsNullOrEmpty(existingRel))
                        DeletePhysicalFile(existingRel);

                    var newRel = await SaveFileAsync(files[i], uploadFolder, site.Id);
                    SetImagePathByIndex(site, i + 1, newRel);
                }
            }

            // Replace brochure if uploaded
            if (model.BrochureFile != null)
            {
                if (!string.IsNullOrEmpty(site.Brochure))
                    DeletePhysicalFile(site.Brochure);

                var broRel = await SaveFileAsync(model.BrochureFile, uploadFolder, site.Id);
                site.Brochure = broRel;
            }

            _context.Update(site);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Site updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Sites/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var site = await _context.Sites.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id.Value);
            if (site == null) return NotFound();

            return View(site);
        }

        // POST: Sites/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var site = await _context.Sites.FindAsync(id);
            if (site == null) return NotFound();

            try
            {
                var folderPath = Path.Combine(_env.WebRootPath, "uploads", "sites", site.Id.ToString());
                if (Directory.Exists(folderPath))
                    Directory.Delete(folderPath, true);
                else
                {
                    DeleteIfExists(site.Image1);
                    DeleteIfExists(site.Image2);
                    DeleteIfExists(site.Image3);
                    DeleteIfExists(site.Image4);
                    DeleteIfExists(site.Brochure);
                }
            }
            catch
            {
                // optionally log deletion errors
            }

            _context.Sites.Remove(site);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Site deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        #region Helpers

        // Save file and return relative path like "/uploads/sites/{siteId}/{fileName}"
        private async Task<string> SaveFileAsync(IFormFile file, string uploadFolder, int siteId)
        {
            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{ext}";
            var physicalPath = Path.Combine(uploadFolder, fileName);

            using (var fs = new FileStream(physicalPath, FileMode.Create))
            {
                await file.CopyToAsync(fs);
            }

            var relativePath = $"/uploads/sites/{siteId}/{fileName}";
            return relativePath;
        }

        private bool IsImage(IFormFile file)
        {
            if (file == null) return false;
            return file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
                   || new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" }
                       .Contains(Path.GetExtension(file.FileName).ToLower());
        }

        private bool IsBrochure(IFormFile file)
        {
            if (file == null) return false;
            var ext = Path.GetExtension(file.FileName).ToLower();
            return AllowedBrochureContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase)
                   || ext == ".pdf" || ext == ".doc" || ext == ".docx";
        }

        private void DeletePhysicalFile(string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return;

            var trimmed = relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var physical = Path.Combine(_env.WebRootPath, trimmed);
            if (System.IO.File.Exists(physical))
            {
                try { System.IO.File.Delete(physical); } catch { /* log if needed */ }
            }
        }

        private void DeleteIfExists(string? relativePath)
        {
            try { DeletePhysicalFile(relativePath); } catch { /* log if needed */ }
        }

        private void SetImagePathByIndex(Sites site, int index, string relativePath)
        {
            switch (index)
            {
                case 1: site.Image1 = relativePath; break;
                case 2: site.Image2 = relativePath; break;
                case 3: site.Image3 = relativePath; break;
                case 4: site.Image4 = relativePath; break;
                default: break;
            }
        }

        private string? GetImagePathByIndex(Sites site, int index)
        {
            return index switch
            {
                1 => site.Image1,
                2 => site.Image2,
                3 => site.Image3,
                4 => site.Image4,
                _ => null
            };
        }

        #endregion
    }
}
