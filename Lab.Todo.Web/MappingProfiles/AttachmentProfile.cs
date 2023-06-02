using AutoMapper;
using Lab.Todo.BLL.Services.AttachmentManagers.Models;
using Lab.Todo.Web.Extensions;
using Lab.Todo.Web.Models;

namespace Lab.Todo.Web.MappingProfiles
{
    public class AttachmentProfile : Profile
    {
        public AttachmentProfile()
        {
            CreateMap<AttachmentCreateModel, AttachmentCreateData>()
                .ForMember(dest => dest.ProvidedFileName, opt => opt.MapFrom(src => src.File.FileName))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.File.ToByteArray()));
        }
    }
}