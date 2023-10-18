namespace Bookify.Web.Core.ViewModel
{
    public class RentalFormViewModel
    {
        public string SubscriberKey { get; set; } = null!;

        public IList<int> SelectedCopies { get; set; } = new List<int>();
    }
}