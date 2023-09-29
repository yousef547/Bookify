using Bookify.Web.Core.Models;
using Bookify.Web.Core.ViewModel;
using Bookify.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Bookify.Web.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult Index()
        {
            var catogery = _context.Categories.AsNoTracking().ToList();
            return View(catogery);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View("Form");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CategoryFormViewModel model)
        {
            if(!ModelState.IsValid)
                return View("Form",model);

            var category = new Category { Name = model.Name };
            _context.Categories.Add(category);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var category = _context.Categories.Find(id);

            if (category is null)
                return NotFound();

            var viewModel = new CategoryFormViewModel
            {
                Id = id,
                Name = category.Name
            };

            return View("Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", model);

            var category = _context.Categories.Find(model.Id);

            if (category is null)
                return NotFound();

            category.Name = model.Name;
            category.LastCreatedOn = DateTime.Now;

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int id)
        {
            var category = _context.Categories.Find(id);

            if (category is null)
                return NotFound();

            category.IsDeleted = !category.IsDeleted;
            category.LastCreatedOn = DateTime.Now;

            _context.SaveChanges();

            return Ok(category.LastCreatedOn.ToString());
        }
    }
}
