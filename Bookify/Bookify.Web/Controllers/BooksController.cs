﻿using AutoMapper;
using Bookify.Web.Core.Consts;
using Bookify.Web.Core.Models;
using Bookify.Web.Core.ViewModel;
using Bookify.Web.Data;
using Bookify.Web.Filters;
using Bookify.Web.Services;
using Bookify.Web.Settings;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NuGet.Packaging.Signing;
using SixLabors.ImageSharp;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using static System.Net.Mime.MediaTypeNames;

namespace Bookify.Web.Controllers
{
	public class BooksController : Controller
	{
		private readonly IWebHostEnvironment _webHostEnvironment;
		private readonly ApplicationDbContext _context;
		private readonly IMapper _mapper;
		private readonly Cloudinary _cloudinary;
		private readonly IImageService _ImageService;

		private List<string> _allowedExtensions = new() { ".jpg", ".jpeg", ".png" };
		private int _maxAllowedSize = 2097152;

		public BooksController(ApplicationDbContext context, IMapper mapper,
			IOptions<CloudinarySettings> cloudinary,
			IWebHostEnvironment webHostEnvironment, IImageService ImageService)
		{
			_context = context;
			_ImageService = ImageService;
			_mapper = mapper;
			_webHostEnvironment = webHostEnvironment;
			Account account = new()
			{
				Cloud = cloudinary.Value.Cloud,
				ApiKey = cloudinary.Value.APIKey,
				ApiSecret = cloudinary.Value.APISecret,
			};
			_cloudinary = new Cloudinary(account);
		}
		public IActionResult Index()
		{
			return View();
		}
		public IActionResult Details(int id)
		{
			var details = _context.Books
				.Include(c => c.Author)
				.Include(c => c.Copies)
				.Include(c => c.Categories)
				.ThenInclude(c => c.Category)
				.SingleOrDefault(x => x.Id == id);
			if (details == null) return NotFound();
			var viewModel = _mapper.Map<BookViewModel>(details);
			return View(viewModel);
		}
		[HttpPost]
		public IActionResult GetBooks()
		{

			var skip = int.Parse(Request.Form["start"]);
			var pageSize = int.Parse(Request.Form["length"]);
			var sortColumnIndex = Request.Form["order[0][column]"];
			var sortColumn = Request.Form[$"columns[{sortColumnIndex}][name]"];
			var sortColumnDirection = Request.Form["order[0][dir]"];
			var searchValue = Request.Form["search[value]"];

			IQueryable<Book> books = _context.Books
				.Include(c => c.Author)
					 .Include(c => c.Categories)
				.ThenInclude(c => c.Category);
			if (!string.IsNullOrEmpty(searchValue))
				books = books.Where(b => b.Title.Contains(searchValue) || b.Author!.Name.Contains(searchValue));
			books = books.OrderBy($"{sortColumn} {sortColumnDirection}");
			var data = books.Skip(skip).Take(pageSize).ToList();
			var mappedData = _mapper.Map<IEnumerable<BookViewModel>>(data);
			var recordsTotal = books.Count();
			var jsonData = new { recordsFiltered = recordsTotal, recordsTotal, data = mappedData };
			return Ok(jsonData);
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
				var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image.FileName)}";
				var (isUploaded, errorMessage) = await _ImageService.UploadAsync(model.Image, imageName, "/images/books/", hasThumbnail: true);
				if (isUploaded)
				{
					book.ImageUrl = $"/images/books/{imageName}";
					book.ImageThumbanilUrl = $"/images/books/thumb/{imageName}";
				}else
				{
					ModelState.AddModelError(nameof(model.Image), errorMessage!);
					return View("Form", PopulateViewModel(model));
				}


				//using var stream = model.Image.OpenReadStream() ;
				//var imageParams = new ImageUploadParams
				//{
				//    File = new FileDescription(imageName, stream),
				//    UseFilename = true
				//};
				//var result = await _cloudinary.UploadAsync(imageParams);
				//book.ImageUrl = result.SecureUrl.ToString();
				//book.ImageThumbanilUrl = GetThumbnail(book.ImageUrl);
				//book.ImagePublicId = result.PublicId;

			}

			foreach (var category in model.SelectedCategories)
				book.Categories.Add(new BookCategory { CategoryId = category });

			book.CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

			_context.Add(book);
			_context.SaveChanges();

			return RedirectToAction(nameof(Details), new { id = book.Id });
		}

		public IActionResult Edit(int id)
		{
			var book = _context.Books.Include(c => c.Categories).SingleOrDefault(x => x.Id == id);
			if (book is null)
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

			var book = _context.Books.Include(b => b.Categories).Include(b => b.Copies).SingleOrDefault(b => b.Id == model.Id);

			if (book is null)
				return NotFound();

			if (model.Image is not null)
			{
				if (!string.IsNullOrEmpty(book.ImageUrl))
				{
					_ImageService.Delete(model.ImageUrl, model.ImageThumbanilUrl);

					//await _cloudinary.DeleteResourcesAsync(book.ImagePublicId);
				}

				var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image.FileName)}";
				var (isUploaded, errorMessage) = await _ImageService.UploadAsync(model.Image, imageName, "/images/books/", hasThumbnail: true);
				if (isUploaded)
				{
					model.ImageUrl = $"/images/books/{imageName}";
					model.ImageThumbanilUrl = $"/images/books/thumb/{imageName}";
				}
				else
				{
					ModelState.AddModelError(nameof(model.Image), errorMessage!);
					return View("Form", PopulateViewModel(model));
				}
			
				//using var stream = model.Image.OpenReadStream();
				//var imageParams = new ImageUploadParams
				//{
				//    File = new FileDescription(imageName, stream),
				//    UseFilename = true
				//};
				//var result = await _cloudinary.UploadAsync(imageParams);
				//book.ImageUrl = result.SecureUrl.ToString();
				//book.ImagePublicId = result.PublicId;
			}

			else if (model.Image is null && !string.IsNullOrEmpty(book.ImageUrl))
			{

				model.ImageUrl = book.ImageUrl;
				model.ImageThumbanilUrl = book.ImageThumbanilUrl;
			}

			book = _mapper.Map(model, book);
			book.LastCreatedOn = DateTime.Now;
			book.ImageThumbanilUrl = GetThumbnail(book.ImageUrl!);

			foreach (var category in model.SelectedCategories)
				book.Categories.Add(new BookCategory { CategoryId = category });
			book.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

			if (!model.IsAvailableForRental)
				foreach (var copy in book.Copies)
					copy.IsAvailableForRental = false;

			_context.SaveChanges();

			return RedirectToAction(nameof(Details), new { id = book.Id });
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
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult ToggleStatus(int id)
		{
			var book = _context.Books.Find(id);

			if (book is null)
				return NotFound();

			book.IsDeleted = !book.IsDeleted;
			book.LastCreatedOn = DateTime.Now;
			book.CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

			_context.SaveChanges();

			return Ok();
		}
		private string GetThumbnail(string url)
		{
			return url.Replace("upload/", "upload/c_thumb,w_200,g_face/");
		}
		//https://res.cloudinary.com/dizw0qyma/image/upload/c_thumb,w_200,g_face/v1696323570/pq4qrwiys7jeygbelpvb.png
		//https://res.cloudinary.com/dizw0qyma/image/upload/v1696323570/pq4qrwiys7jeygbelpvb.png
	}
}
