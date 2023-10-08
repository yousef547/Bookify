using Bookify.Web.Core.Consts;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Bookify.Web.Core.ViewModel
{
    public class AuthorFormViewModel
    {
        public int Id { get; set; }

        [MaxLength(100, ErrorMessage = Errors.MaxLength), Display(Name = "Author")]
        [Remote("AllowItem", null!, AdditionalFields = "Id", ErrorMessage = Errors.Duplicated),
            RegularExpression(RegexPatterns.CharactersOnly_Eng, ErrorMessage = Errors.OnlyEnglishLetters)]
        public string Name { get; set; } = null!;
    }
}