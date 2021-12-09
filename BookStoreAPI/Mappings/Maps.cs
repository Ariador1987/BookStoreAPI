using AutoMapper;
using BookStoreAPI.Data;
using BookStoreAPI.Data.DTOs;
using Microsoft.AspNetCore.Identity;

namespace BookStoreAPI.Mappings
{
    public class Maps : Profile
    {
        public Maps()
        {
            CreateMap<Author, AuthorDTO>().ReverseMap();
            CreateMap<Author, AuthorCreateDTO>().ReverseMap();
            CreateMap<Author, AuthorUpdateDTO>().ReverseMap();
            CreateMap<Book, BookDTO>().ReverseMap();
            CreateMap<Book, BookCreateDTO>().ReverseMap();
            CreateMap<Book, BookUpdateDTO>().ReverseMap();
            CreateMap<IdentityUser, UserViewDTO>();
        }
    }
}
