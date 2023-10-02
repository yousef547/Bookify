using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Bookify.Web.Core.ViewModel
{
    public class CategoryFormViewModel
    {
        public int Id { get; set; }
        [MaxLength(100, ErrorMessage = "Max length cannot be more than 100 chr.")]
        [Remote("AllowItem",null , AdditionalFields = "Id", ErrorMessage = "Catagery with the some name is already Exist")]
        public string Name { get; set; }
    }
}
