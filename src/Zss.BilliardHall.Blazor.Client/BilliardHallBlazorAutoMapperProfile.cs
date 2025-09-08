using AutoMapper;

using Zss.BilliardHall.Books;

namespace Zss.BilliardHall.Blazor.Client;

public class BilliardHallBlazorAutoMapperProfile : Profile
{
    public BilliardHallBlazorAutoMapperProfile()
    {
        CreateMap<BookDto, CreateUpdateBookDto>();

        //Define your AutoMapper configuration here for the Blazor project.
    }
}
