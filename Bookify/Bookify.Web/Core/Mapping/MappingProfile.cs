using AutoMapper;
using Bookify.Web.Core.Models;
using Bookify.Web.Core.ViewModel;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bookify.Web.Core.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //Category
            CreateMap<Category, CategoryViewModel>();
            CreateMap<CategoryFormViewModel, Category>().ReverseMap();
            CreateMap<Category, SelectListItem>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name));

            //Authors
            CreateMap<Author, AuthorViewModel>();
            CreateMap<AuthorFormViewModel, Author>().ReverseMap();
            CreateMap<Author, SelectListItem>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name));

            //Books
            CreateMap<BookFormViewModel, Book>()
                .ReverseMap()
                .ForMember(dest => dest.Categories, opt => opt.Ignore());

            CreateMap<Book, BookViewModel>()
                 .ForMember(src => src.Author, opt => opt.MapFrom(dest => dest.Author!.Name))
                 .ForMember(src => src.Categories, opt => opt.MapFrom(dest => dest.Categories.Select(c => c.Category!.Name)));

            CreateMap<BookCopy, BookCopyViewModel>()
             .ForMember(dest => dest.BookTitle, opt => opt.MapFrom(src => src.Book!.Title));

            CreateMap<BookCopy, BookCopyFormViewModel>();
            
            CreateMap<ApplicationUser, UserViewModel>();

        }
    }
}