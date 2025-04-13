namespace BEAUTIFY_QUERY.CONTRACT.Services.Dashboards;

public class Response
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
    
    public class GetDaytimeInformationResponse
    {
        DatetimeInformation? DatetimeInformation { get; set; }
        List<DatetimeInformation>? DatetimeInformationList { get; set; }
    }

    public class DatetimeInformation
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
}