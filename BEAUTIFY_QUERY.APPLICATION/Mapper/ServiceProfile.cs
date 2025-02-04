using AutoMapper;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
using BEAUTIFY_QUERY.CONTRACT.Services.Subscriptions;
using BEAUTIFY_QUERY.DOMAIN.Documents;

namespace BEAUTIFY_QUERY.APPLICATION.Mapper;
public class ServiceProfile : Profile
{
    public ServiceProfile()
    {
        CreateMap<SubscriptionProjection, Response.GetSubscriptionResponse>();
        CreateMap<PagedResult<SubscriptionProjection>, PagedResult<Response.GetSubscriptionResponse>>();
        
        // CreateMap<ClinicOnBoardingRequest, CONTRACT.Services.Clinics.Response.GetApplyRequest>()
        //     .ForMember(dest => dest.Id,
        //         opt 
        //             => opt.MapFrom(src => src.Id))
        //     .ForMember(dest => dest.Name,
        //         opt 
        //             => opt.MapFrom(src => src.Clinic!.Name))
        //     .ForMember(dest => dest.Email,
        //         opt 
        //             => opt.MapFrom(src => src.Clinic!.Email))
        //     .ForMember(dest => dest.Address,
        //         opt 
        //             => opt.MapFrom(src => src.Clinic!.Address))
        //     .ForMember(dest => dest.TotalApply,
        //         opt 
        //             => opt.MapFrom(src => src.Clinic!.TotalApply));
        //
        // CreateMap<List<ClinicOnBoardingRequest>, List<CONTRACT.Services.Clinics.Response.GetApplyRequest>>();
        //
        // CreateMap<PagedResult<ClinicOnBoardingRequest>, PagedResult<CONTRACT.Services.Clinics.Response.GetApplyRequest>>()
        //     .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));


        // ==================SkillMapping=====================
        // CreateMap<Skill, SkillResponse.GetSkillsQuery>().ReverseMap();
        //
        // CreateMap<PagedResult<Skill>, PagedResult<SkillResponse.GetSkillsQuery>>()
        //     .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));
        //
        // // ==================MentorMapping=====================
        // CreateMap<MentorProjection, Response.GetMentorResponse>()
        //     .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.DocumentId))
        //     .ConstructUsing((src, context) => new Response.GetMentorResponse
        //     {
        //         Email = src.Email,
        //         Point = src.Points,
        //         FullName = src.FullName,
        //         CreatedOnUtc = src.CreatedOnUtc,
        //         Skills = src.MentorSkills == null
        //             ? []
        //             : src.MentorSkills.Select(s => new Response.Skill
        //             {
        //                 SkillName = s.Name,
        //                 SkillDesciption = s.Description,
        //                 CreatedOnUtc = s.CreatedOnUtc,
        //                 SkillCategoryType = s.CateogoryType,
        //                 Cetificates = s.SkillCetificates.Select(c => new Response.Cetificate
        //                 {
        //                     CetificateName = c.Name,
        //                     CetificateImageUrl = c.ImageUrl,
        //                     CetificateDesciption = c.Description,
        //                     CreatedOnUtc = c.CreatedOnUtc
        //                 }).ToList()
        //             }).ToList()
        //     });
        //
        // CreateMap<MentorProjection, Response.GetAllMentorsResponse>()
        //     .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.DocumentId))
        //     .ConstructUsing((src, context) => new Response.GetAllMentorsResponse
        //     {
        //         Email = src.Email,
        //         Point = src.Points,
        //         FullName = src.FullName,
        //         CreatedOnUtc = src.CreatedOnUtc,
        //         Skills = src.MentorSkills == null ? [] : src.MentorSkills.Select(s => s.Name).ToList()
        //     });
        //
        // CreateMap<PagedResult<MentorProjection>, PagedResult<Response.GetAllMentorsResponse>>()
        //     .ConstructUsing((src, context) =>
        //     {
        //         var mappedItems = src.Items.Select(item => context.Mapper.Map<Response.GetAllMentorsResponse>(item))
        //             .ToList();
        //         return new PagedResult<Response.GetAllMentorsResponse>(mappedItems, src.PageIndex, src.PageSize,
        //             src.TotalCount);
        //     });
        //
        // CreateMap<Subject, Contract.Services.Subjects.Response.GetSubjectsQuery>().ReverseMap();
        // CreateMap<PagedResult<Subject>, PagedResult<Contract.Services.Subjects.Response.GetSubjectsQuery>>()
        //     .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));
    }
}