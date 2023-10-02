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

            //Authors
            CreateMap<Author, AuthorViewModel>();
            CreateMap<AuthorFormViewModel, Author>().ReverseMap();
            //CreateMap<Author, SelectListItem>()
            //    .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id))
            //    .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name));
        }
    }
}