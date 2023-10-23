using Bookify.Web.Core.Models;
using Bookify.Web.Core.Utilities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Bookify.Web.Core.ViewModel
{
	public class BooksReportViewModel
	{
		[Display(Name = "Authors")]
		public List<int>? SelectedAuthors { get; set; } = new List<int>();
		public IEnumerable<SelectListItem> Authors { get; set; } = new List<SelectListItem>();

		[Display(Name = "Categories")]
		public List<int>? SelectedCategories { get; set; } = new List<int>();
		public IEnumerable<SelectListItem> Categories { get; set; } = new List<SelectListItem>();

		public PaginatedList<Book> Books { get; set; }
	}
}