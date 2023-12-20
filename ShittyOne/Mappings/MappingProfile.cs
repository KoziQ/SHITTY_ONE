using AutoMapper;
using ShittyOne.Entities;
using ShittyOne.Models;
using File = ShittyOne.Entities.File;

namespace ShittyOne.Mappings;

public class MappingProfile : Profile
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MappingProfile(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;

        CreateMap<File, FileModel>()
            .ForMember(m => m.Url, map => map.MapFrom(m => GetImageUrl(m.SubDir)));

        CreateMap<Group, UserGroupModel>()
            .ForMember(m => m.Users, map => map.MapFrom(m => m.Users));

        CreateMap<User, UserModel>();

        CreateMap<SurveyWriteModel, Survey>()
            .ForMember(s => s.Questions, map => map.Ignore());

        CreateMap<SurveyQuestionWriteModel, MultipleQuestion>()
            .ForMember(q => q.Groups, map => map.Ignore())
            .ForMember(q => q.Users, map => map.Ignore())
            .ForMember(q => q.File, map => map.Ignore())
            .ForMember(q => q.Answers, map => map.MapFrom(a => a.Answers));

        CreateMap<SurveyQuestionWriteModel, StringQuestion>()
            .ForMember(q => q.Groups, map => map.Ignore())
            .ForMember(q => q.Users, map => map.Ignore())
            .ForMember(q => q.File, map => map.Ignore());

        CreateMap<QuestionAnswerWriteModel, SurveyQuestionAnswer>()
            .ForMember(q => q.Text, map => map.MapFrom(q => q.Text));

        CreateMap<SurveyQuestion, SurveyQuestionModel>()
            .Include<MultipleQuestion, MultipleQuestionModel>()
            .Include<StringQuestion, SingleQuestionModel>()
            .ForMember(q => q.Type, map => map.MapFrom(q => q.GetType().Name));

        CreateMap<Survey, SurveyModel>();

        CreateMap<MultipleQuestion, MultipleQuestionModel>();

        CreateMap<StringQuestion, SingleQuestionModel>();

        CreateMap<SurveyQuestionAnswer, SurveyQuestionAnswerModel>();
    }

    private string GetImageUrl(string subDir)
    {
        if (subDir.StartsWith("/uploads"))
            return _httpContextAccessor.HttpContext.Request.Scheme + Uri.SchemeDelimiter +
                   _httpContextAccessor.HttpContext.Request.Host.Value + subDir;
        return subDir;
    }
}