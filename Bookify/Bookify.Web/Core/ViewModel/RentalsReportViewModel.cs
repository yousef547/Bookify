using Bookify.Web.Core.Models;
using Bookify.Web.Core.Utilities;

namespace Bookify.Web.Core.ViewModel
{
    public class RentalsReportViewModel
    {
        public string Duration { get; set; } = null!;
        public PaginatedList<RentalCopy> Rentals { get; set; }
    }
}