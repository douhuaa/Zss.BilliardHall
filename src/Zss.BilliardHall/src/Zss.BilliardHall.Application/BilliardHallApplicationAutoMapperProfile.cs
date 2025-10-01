using AutoMapper;
using Zss.BilliardHall.Books;

namespace Zss.BilliardHall;

public class BilliardHallApplicationAutoMapperProfile : Profile
{
    public BilliardHallApplicationAutoMapperProfile()
    {
        CreateMap<Book, BookDto>();
        CreateMap<CreateUpdateBookDto, Book>();
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */
    }
}
