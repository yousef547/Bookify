using AutoMapper;
using Bookify.Web.Core.Consts;
using Bookify.Web.Core.Models;
using Bookify.Web.Core.ViewModel;
using Bookify.Web.Data;
using Bookify.Web.Settings;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Bookify.Web.Controllers
{
    public class BooksController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly Cloudinary _cloudinary;

        private List<string> _allowedExtensions = new() { ".jpg", ".jpeg", ".png" };
        private int _maxAllowedSize = 2097152;

        public BooksController(ApplicationDbContext context, IMapper mapper,
            IOptions<CloudinarySettings> cloudinary,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
            Account account = new()
            {
                Cloud = cloudinary.Value.Cloud,
                ApiKey = cloudinary.Value.APIKey,
                ApiSecret = cloudinary.Value.APISecret,
            };
            _cloudinary= new Cloudinary(account);
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View("Form", PopulateViewModel());
         }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", PopulateViewModel(model));

            var book = _mapper.Map<Book>(model);

            if (model.Image is not null)
            {
                var extension = Path.GetExtension(model.Image.FileName);

                if (!_allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(nameof(model.Image), Errors.NotAllowedExtension);
                    return View("Form", PopulateViewModel(model));
                }

                if (model.Image.Length > _maxAllowedSize)
                {
                    ModelState.AddModelError(nameof(model.Image), Errors.MaxSize);
                    return View("Form", PopulateViewModel(model));
                }
                var imageName = $"{Guid.NewGuid()}{extension}";

                //var path = Path.Combine($"{_webHostEnvironment.WebRootPath}/images/books", imageName);

                //using var stream = System.IO.File.Create(path);
                //await model.Image.CopyToAsync(stream);

                //book.ImageUrl = imageName;

                using var stream = model.Image.OpenReadStream() ;
                var imageParams = new ImageUploadParams
                {
                    File = new FileDescription(imageName, stream),
                    UseFilename = true
                };
                var result = await _cloudinary.UploadAsync(imageParams);
                book.ImageUrl = result.SecureUrl.ToString();
                book.ImageThumbanilUrl = GetThumbnail(book.ImageUrl);
                book.ImagePublicId = result.PublicId;

            }

            foreach (var category in model.SelectedCategories)
                book.Categories.Add(new BookCategory { CategoryId = category });

            _context.Add(book);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var book = _context.Books.Include(c=>c.Categories).SingleOrDefault(x=>x.Id == id);
            if(book is null)
                return NotFound();
            var model = _mapper.Map<BookFormViewModel>(book);
            var viewModel = PopulateViewModel(model);
            viewModel.SelectedCategories = book.Categories.Select(x => x.CategoryId).ToList();
            return View("Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BookFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", PopulateViewModel(model));

            var book = _context.Books.Include(b => b.Categories).SingleOrDefault(b => b.Id == model.Id);

            if (book is null)
                return NotFound();

            if (model.Image is not null)
            {
                if (!string.IsNullOrEmpty(book.ImageUrl))
                {
                    //var oldImagePath = Path.Combine($"{_webHostEnvironment.WebRootPath}/images/books", book.ImageUrl);

                    //if (System.IO.File.Exists(oldImagePath))
                    //    System.IO.File.Delete(oldImagePath);

                    await _cloudinary.DeleteResourcesAsync(book.ImagePublicId);
                }

                var extension = Path.GetExtension(model.Image.FileName);

                if (!_allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(nameof(model.Image), Errors.NotAllowedExtension);
                    return View("Form", PopulateViewModel(model));
                }

                if (model.Image.Length > _maxAllowedSize)
                {
                    ModelState.AddModelError(nameof(model.Image), Errors.MaxSize);
                    return View("Form", PopulateViewModel(model));
                }

                var imageName = $"{Guid.NewGuid()}{extension}";

                //var path = Path.Combine($"{_webHostEnvironment.WebRootPath}/images/books", imageName);

                //using var stream = System.IO.File.Create(path);
                //await model.Image.CopyToAsync(stream);

                //model.ImageUrl = imageName;

                using var stream = model.Image.OpenReadStream();
                var imageParams = new ImageUploadParams
                {
                    File = new FileDescription(imageName, stream),
                    UseFilename = true
                };
                var result = await _cloudinary.UploadAsync(imageParams);
                book.ImageUrl = result.SecureUrl.ToString();
                book.ImagePublicId = result.PublicId;
            }

            else if (model.Image is null && !string.IsNullOrEmpty(book.ImageUrl))
                model.ImageUrl = book.ImageUrl;

            book = _mapper.Map(model, book);
            book.LastCreatedOn = DateTime.Now;
                book.ImageThumbanilUrl = GetThumbnail(book.ImageUrl!);

            foreach (var category in model.SelectedCategories)
                book.Categories.Add(new BookCategory { CategoryId = category });

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
        private BookFormViewModel PopulateViewModel(BookFormViewModel? model = null)
        {
            BookFormViewModel viewModel = model is null ? new BookFormViewModel() : model;

            var authors = _context.Authors.Where(a => !a.IsDeleted).OrderBy(a => a.Name).ToList();
            var categories = _context.Categories.Where(a => !a.IsDeleted).OrderBy(a => a.Name).ToList();

            viewModel.Authors = _mapper.Map<IEnumerable<SelectListItem>>(authors);
            viewModel.Categories = _mapper.Map<IEnumerable<SelectListItem>>(categories);

            return viewModel;
        }

        public IActionResult AllowItem(BookFormViewModel model)
        {
            var book = _context.Books.SingleOrDefault(c => c.Title == model.Title && c.AuthorId == model.AuthorId);
            var isAllowed = book is null || book.Id.Equals(model.Id);

            return Json(isAllowed);
        }

        private string GetThumbnail(string url)
        {
            return url.Replace("upload/", "upload/c_thumb,w_200,g_face/");
        }
        //https://res.cloudinary.com/dizw0qyma/image/upload/c_thumb,w_200,g_face/v1696323570/pq4qrwiys7jeygbelpvb.png
        //https://res.cloudinary.com/dizw0qyma/image/upload/v1696323570/pq4qrwiys7jeygbelpvb.png
    }
}
