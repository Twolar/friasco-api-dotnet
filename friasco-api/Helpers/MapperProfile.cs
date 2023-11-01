using AutoMapper;
using friasco_api.Data.Entities;
using friasco_api.Models;

namespace friasco_api.Helpers;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<UserCreateRequestModel, User>();

        CreateMap<UserUpdateRequestModel, User>()
            .ForAllMembers(o => o.Condition((src, dest, value) =>
            {
                // Add conditions here so that only the DELTA is updated when mapping from UpdateRequestModel to the User model.

                if (value == null)
                {
                    return false;
                }

                if (value.GetType() == typeof(string) && string.IsNullOrEmpty((string)value))
                {
                    return false;
                }

                return true;
            }));
        ;
    }
}
