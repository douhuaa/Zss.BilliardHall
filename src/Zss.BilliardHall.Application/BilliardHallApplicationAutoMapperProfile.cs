using AutoMapper;
using Volo.Abp.AutoMapper;
using Zss.BilliardHall.Books;
using Zss.BilliardHall.BilliardHalls;

namespace Zss.BilliardHall;

public class BilliardHallApplicationAutoMapperProfile : Profile
{
    public BilliardHallApplicationAutoMapperProfile()
    {
        // Existing Book mappings
        CreateMap<Book, BookDto>();
        CreateMap<CreateUpdateBookDto, Book>();
        
        // BilliardTable mappings - simplified
        CreateMap<BilliardTable, BilliardTableDto>();
        CreateMap<CreateBilliardTableDto, BilliardTable>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.CreationTime, opt => opt.Ignore())
            .ForMember(x => x.CreatorId, opt => opt.Ignore())
            .ForMember(x => x.LastModificationTime, opt => opt.Ignore())
            .ForMember(x => x.LastModifierId, opt => opt.Ignore())
            .ForMember(x => x.IsDeleted, opt => opt.Ignore())
            .ForMember(x => x.DeleterId, opt => opt.Ignore())
            .ForMember(x => x.DeletionTime, opt => opt.Ignore())
            .ForMember(x => x.Status, opt => opt.Ignore())
            .ForMember(x => x.TenantId, opt => opt.Ignore())
            .ForMember(x => x.BilliardHall, opt => opt.Ignore());
            
        CreateMap<UpdateBilliardTableDto, BilliardTable>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.CreationTime, opt => opt.Ignore())
            .ForMember(x => x.CreatorId, opt => opt.Ignore())
            .ForMember(x => x.LastModificationTime, opt => opt.Ignore())
            .ForMember(x => x.LastModifierId, opt => opt.Ignore())
            .ForMember(x => x.IsDeleted, opt => opt.Ignore())
            .ForMember(x => x.DeleterId, opt => opt.Ignore())
            .ForMember(x => x.DeletionTime, opt => opt.Ignore())
            .ForMember(x => x.Number, opt => opt.Ignore())
            .ForMember(x => x.Status, opt => opt.Ignore())
            .ForMember(x => x.BilliardHallId, opt => opt.Ignore())
            .ForMember(x => x.TenantId, opt => opt.Ignore())
            .ForMember(x => x.BilliardHall, opt => opt.Ignore());
    }
}
