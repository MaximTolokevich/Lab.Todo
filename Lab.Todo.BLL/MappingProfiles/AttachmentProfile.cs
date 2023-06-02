using AutoMapper;
using Lab.Todo.BLL.Services.AttachmentManagers.Models;
using Lab.Todo.DAL.Entities;

namespace Lab.Todo.BLL.MappingProfiles
{
    public class AttachmentProfile : Profile
    {
        public AttachmentProfile()
        {
            CreateMap<AttachmentCreateData, AttachmentDbEntry>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UniqueFileName, opt => opt.Ignore());

            CreateMap<AttachmentDbEntry, Attachment>();
        }
    }
}