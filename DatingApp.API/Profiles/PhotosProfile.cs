using AutoMapper;
using DatingApp.API.Controllers;
using DatingApp.API.Dtos;
using DatingApp.API.Models;

namespace DatingApp.API.Profiles
{
    public class PhotosProfile : Profile
    {
        public PhotosProfile()
        {
            CreateMap<Photo, PhotoForDetailedDto>();
            CreateMap<PhotoForCreationDto, Photo>();
            CreateMap<Photo, PhotoForReturnDto>();            
        }
    }
}