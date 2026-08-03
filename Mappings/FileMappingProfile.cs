using AutoMapper;
using Backend.Models;
using Backend.DTOs;

namespace Backend.Mappings
{
    public class FileMappingProfile : Profile
    {
        public FileMappingProfile()
        {
            CreateMap<FileEntity, FileResponseDto>();
        }
    }
}