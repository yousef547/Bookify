namespace Bookify.Web.Core.ViewModel
{
    public class AuthorViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public bool IsDeleted { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? LastCreatedOn { get; set; }
    }
}