namespace BEAUTIFY_QUERY.DOMAIN;
public static class ErrorMessages
{
    public static class Clinic
    {
        public const string ClinicNotFound = "Không tìm thấy phòng khám.";
        public const string ClinicIsNotABranch = "Phòng khám không phải là chi nhánh.";
        public const string ParentClinicNotFound = "Không tìm thấy phòng khám chi nhánh chính";

        public const string ParentClinicNotFoundOrChildren =
            "Không tìm thấy phòng khám chi nhánh chính hoặc không có chi nhánh nào";

        public const string AmountMustBeGreaterThan2000 = "Số tiền phải lớn hơn 2000";
        public const string InsufficientFunds = "Số dư không đủ";
        public const string ClinicAlreadyExists = "Clinic already exists.";
        public const string ClinicBranchNotFound = "Clinic branch not found.";
        public const string ClinicBranchAlreadyExists = "Clinic branch already exists.";
        public const string ClinicBranchNotActive = "Clinic branch is not active.";
    }

    public static class Wallet
    {
        public const string WalletNotFound = "Không tìm thấy ví.";
        public const string InsufficientBalance = "Insufficient balance.";
        public const string InvalidTransactionType = "Invalid transaction type.";
        public const string InvalidTransactionStatus = "Trạng thái giao dịch không hợp lệ.";
        public const string TransactionNotFound = "Transaction not found.";
        public const string TransactionAlreadyExists = "Transaction already exists.";
        public const string TransactionFailed = "Transaction failed.";
    }
}