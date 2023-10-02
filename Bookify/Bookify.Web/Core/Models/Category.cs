
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Bookify.Web.Core.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(100)]
        public string Name { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedOn { get; set; }= DateTime.Now;
        public DateTime? LastCreatedOn { get; set; }
    }
}
