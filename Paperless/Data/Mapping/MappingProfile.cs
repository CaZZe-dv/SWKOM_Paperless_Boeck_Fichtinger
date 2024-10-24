using AutoMapper;
using Paperless.Data.Entities;
using Paperless.Models;

namespace Paperless.Data.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Document, DocumentDto>();
            CreateMap<DocumentDto, Document>();
        }
    }
}
