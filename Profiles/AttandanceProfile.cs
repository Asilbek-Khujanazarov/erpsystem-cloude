using AutoMapper;
using HRsystem.Models;

namespace HRsystem.Profiles
{
    public class AttendanceProfile : Profile
    {
        public AttendanceProfile()
        {
            // Attendance -> AttendanceDto
            CreateMap<Attendance, AttendanceDto>()
                .ForMember(dest => dest.EmployeeName, 
                    opt => opt.MapFrom(src => $"{src.Employee.FirstName} {src.Employee.LastName}"))
                .ForMember(dest => dest.HoursWorked, 
                    opt => opt.MapFrom(src => src.ExitTime.HasValue ? (src.ExitTime.Value - src.EntryTime).TotalHours : 0));

            // AttendanceEntryDto -> Attendance (kirish va chiqish uchun)
            CreateMap<AttendanceEntryDto, Attendance>()
                .ForMember(dest => dest.EmployeeId, opt => opt.MapFrom(src => src.EmployeeId))
                .ForMember(dest => dest.EntryTime, opt => opt.Ignore()) // Dinamik tarzda controller’da o‘rnatiladi
                .ForMember(dest => dest.ExitTime, opt => opt.Ignore())  // Dinamik tarzda controller’da o‘rnatiladi
                .ForMember(dest => dest.AttendanceDate, opt => opt.Ignore()); // Dinamik tarzda controller’da o‘rnatiladi
        }
    }
}