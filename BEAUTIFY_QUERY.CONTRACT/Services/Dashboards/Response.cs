namespace BEAUTIFY_QUERY.CONTRACT.Services.Dashboards;

public class Responses
{
    public class GetTotalInformationResponse
    {
        public int? TotalBranch { get; set; }
        public int? TotalBranchActive { get; set; }
        public int? TotalBranchInActive { get; set; }
        public int? TotalStaff { get; set; }
        public int TotalService { get; set; }
        public int TotalDoctor { get; set; }
    };
    
    public class GetSystemTotalInformationResponse
    {
        public int? TotalClinics { get; set; }
        public int? TotalBranding { get; set; }
        public int? TotalBranches { get; set; }
        public int? TotalBranchActive { get; set; }
        public int? TotalBranchInActive { get; set; }
        public int TotalBrandPending{ get; set; }
        public int TotalService { get; set; }
        public int TotalDoctor { get; set; }
    };
    
    public class GetDaytimeInformationResponse
    {
        public Information? DatetimeInformation { get; set; }
        public List<DatetimeInformation>? DatetimeInformationList { get; set; }
    }

    public class Information
    {
        public int TotalCountOrderCustomer { get; set; }
        public int TotalCountScheduleCustomer { get; set; }
        public int TotalCountCustomerSchedule { get; set; }
        public int TotalCountCustomerSchedulePending { get; set; }
        public int TotalCountCustomerScheduleInProgress { get; set; }
        public int TotalCountCustomerScheduleCompleted { get; set; }
        public decimal TotalSumRevenue { get; set; }
        public int TotalCountOrderPending { get; set; }
        public decimal TotalSumRevenueNormal { get; set; }
        public decimal TotalSumRevenueLiveStream { get; set; }
    }

    public class DatetimeInformation
    {
        public Information Information { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
    }
    
    public class SystemInformation
    {
        public int TotalCountBranding { get; set; }
        public int TotalCountBranches { get; set; }
        public int TotalCountService { get; set; }
        public int TotalCountDoctor { get; set; }
        public int TotalCountBronzeSubscription { get; set; }
        public int TotalCountSilverSubscription { get; set; }
        public int TotalCountGoldSubscription { get; set; }
        public decimal TotalSumRevenue { get; set; }
        public decimal TotalSystemSumRevenue { get; set; }
        public decimal TotalSumBronzeSubscriptionRevenue { get; set; }
        public decimal TotalSumSilverSubscriptionRevenue { get; set; }
        public decimal TotalSumGoldSubscriptionRevenue { get; set; }
        public decimal TotalSumClinicRevenue { get; set; }
    }
    
    public class SystemDatetimeInformation
    {
        public SystemInformation Information { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
    }
    
    public class GetSystemDaytimeInformationResponse
    {
        public SystemInformation? SystemInformation { get; set; }
        public List<SystemDatetimeInformation>? SystemInformationList { get; set; }
    }
}