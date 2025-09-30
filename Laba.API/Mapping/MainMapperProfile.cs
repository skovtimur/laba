using AutoMapper;
using Laba.API.Infrastruction.ModelsFromDb;
using Laba.Shared.Domain.Dtos;
using Laba.Shared.Domain.Entities;
using Laba.Shared.Domain.ValueObjects;

namespace Laba.API.Mapping;

public class MainMapperProfile : Profile
{
    public MainMapperProfile()
    {
        CreateMap<UserEntity, UserDtoToView>()
            .ForMember(u => u.Email,
                opt => opt.MapFrom(x => x.Email.Email));

        CreateMap<UserModelFromDb, UserEntity>()
            .ForMember(u => u.HashedPassword,
                opt => opt.MapFrom(x => new HashedPasswordValueObject { Password = x.HashedPassword }))
            .ForMember(u => u.Email,
                opt => opt.MapFrom(x => EmailValueObject.Create(x.Email)));
    }
}