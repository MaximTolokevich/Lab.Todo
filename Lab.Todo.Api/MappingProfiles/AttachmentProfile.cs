using AutoMapper;
using Lab.Todo.Api.DTOs.Requests;
using Lab.Todo.Api.DTOs.Responses;
using Lab.Todo.BLL.Services.AttachmentManagers.Models;

namespace Lab.Todo.Api.MappingProfiles
{
    public class AttachmentProfile : Profile
    {
        public AttachmentProfile()
        {
            CreateMap<AttachmentCreateRequest, AttachmentCreateData>()
                .ForMember(dest => dest.ProvidedFileName, opt => opt.MapFrom(src => src.FileName))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content));

            CreateMap<Attachment, AttachmentCreateResponse>()
                .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.ProvidedFileName))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));
        }
    }
}