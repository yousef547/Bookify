using Bookify.Web.Core.Consts;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Bookify.Web.Core.ViewModel
{
    public class CategoryFormViewModel
    {
        public int Id { get; set; }
        [MaxLength(100, ErrorMessage = Errors.MaxLength), Display(Name = "Catogery"),
            RegularExpression(RegexPatterns.CharactersOnly_Eng, ErrorMessage = Errors.OnlyEnglishLetters)]
        [Remote("AllowItem", null!, AdditionalFields = "Id", ErrorMessage = Errors.Duplicated)]
        public string Name { get; set; }
    }
}
