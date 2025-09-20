namespace RupalStudentCore8App.Server.Class
{
    public class GlobalConstant
    {
        public static string AppName = "CwiSchool";

        public static class LoginProvider
        {
            public const string Local = "Local";
            public const string TwoFactor = "TwoFactor";
            public const string Google = "Google";
        }


        public static class TokenType
        {
            // RefreshToken, AccessToken, etc.
            public const string RefreshToken = "RefreshToken";
            public const string AccessToken = "AccessToken";
        }
        public static class RoleType
        {
            public const string Administrator = "Administrator";
            public const string Admin = "Admin";
            public const string User = "User";
        }

        public static class ParentClass
        {
            public const string Company = "Company";
            public const string Staff = "Staff";
            public const string General = "General";
        }

        public static class Common
        {
            public const string PhoneCode = "+966";
        }

        public static class TaxSettlementFrequency
        {
            public const string Monthly = "Monthly";
            public const string Quarterly = "Quarterly";
        }

        public static class RelationType
        {
            public const string Father = "Father";
            public const string Mother = "Mother";
            public const string EmergencyContact = "Emergency Contact";
        }

        public static class FinanceStatus
        {
            public const string Saved = "Saved";
            public const string Posted = "Posted";
            public const string Cancelled = "Cancelled";
        }
        public static class ProductType
        {
            public const string Product = "Product";
            public const string Service = "Service";
        }
        public static class PurchaseStatus
        {
            public const string Saved = "Saved";
            public const string Post = "Posted";
            public const string Cancelled = "Cancelled";
            public const string Void = "Void";
        }
        public static class BankTransactionDocType
        {
            public const string Transaction = "Transaction";
            public const string Receipt = "Receipt";
            public const string Reversal = "Reversal";
        }
        public static class AdjustmentType
        {
            public const string Increase = "Increase";
            public const string Decrease = "Decrease";
        }
        public static class DocType
        {
            public const string Invoice = "Invoice";
            public const string Quotation = "Quotation";
            public const string Return = "Return";
            public const string POS = "POS";
            public const string POSReturn = "POSReturn";
        }
        public static class PurchasePaymentType
        {
            public const string Payment = "Payment";
            public const string CreditNote = "Credit Note";
            public const string DebitNote = "Debit Note";
        }
        public static class TransactionType
        {
            public const string Credit = "Credit";
            public const string Debit = "Debit";
        }

        public static class ContractStatus
        {
            public const string Created = "Created";
            public const string Updated = "Updated";
            public const string Active = "Active";
        }

        public static class ParentInfoStatus
        {
            public const string Created = "Created";
            public const string Submitted = "Submitted";
            public const string Active = "Active";
        }
        public static class PaymentScheduleStatus
        {
            public const string Pending = "Pending";
        }

        public static class VendorDocType
        {
            public const string InvoiceAgainstPO = "PO Invoice";
            public const string InvoiceAgainstReceiving = "Receiving Invoice";
            public const string LandedCostInvoice = "Landed Cost Invoice";
            public const string Standard = "Standard";
        }
        public static class FilePath
        {
            public const string Default = "/Upload/";
            public const string Attachment = "/Upload/Attachment/";
            public const string Student = "/Upload/Student/";
            public const string ProfileImage = "/Upload/ProfileImage/";
        }

        public static class FileType
        {
            public const string Default = "Default";
            public const string Attachment = "Attachment";
            public const string ProfileImage = "ProfileImage";
            public const string Student = "Student";
            public static string GetPath(string fileType)
            {

                switch (fileType)
                {
                  
                    case FileType.Attachment:
                        return FilePath.Attachment;
                    case FileType.ProfileImage:
                        return FilePath.ProfileImage;
                    case FileType.Student:
                        return FilePath.Student;
                    default:
                        return FilePath.Default;
                }
            }
        }
        public static class Msg
        {
            public const string DeleteRefersToOther = "You can't delete this record, it refers to another record, so you need to delete that first.";
            public const string DeleteRecordNotExists = "The record you are trying to delete does not exist.";
            public const string DeleteFailed = "Failed to delete a record.";
            public const string DeleteError = "Error to delete a record.";
            public const string UpdateRecordNotExists = "The record you are trying to update does not exist.";
            public const string RecordNotExists = "The record does not exist.";
            public const string InvalidTimeFormat = "Invalid Time Format.";
            public const string InvalidDayName = "Invalid Day Name.";
            public const string Unauthorised = "You are not authorised to perform this action.";
        }

        public static class FilePathType
        {
            public const string Company = "CompanyImage";
            public const string Product = "Product";
            public const string XML = "XMLFiles";
            public const string Vendor = "Vendor";
            public const string Customer = "Customer";
            public const string Currency = "Currency";
            public const string StudentImage = "StudentImage";
        }

        public static class AutoIncrement
        {
            public const string Invoice = "Invoice";
            public const string Quotation = "Quotation";
            public const string Return = "Return";
            public const string CashReceipt = "CashReceipt";
            public const string CreditMemo = "CreditMemo";
            public const string DebitMemo = "DebitMemo";
            public const string POSInvoice = "POSInvoice";
            public const string XMLInvoiceCounter = "XMLInvoiceCounter";
            public const string PurchaseOrder = "PurchaseOrder";
            public const string Receiving = "Receiving";
            public const string VendorInvoice = "VendorInvoice";
            public const string PurchasePayment = "PurchasePayment";
            public const string PurchaseCreditNote = "PurchaseCreditNote";
            public const string PurchaseDebitNote = "PurchaseDebitNote";
            public const string TransferRequest = "TransferRequest";
            public const string TransferIssuance = "TransferIssuance";
            public const string TransferReceiving = "TransferReceiving";
            public const string Adjustment = "Adjustment";
            public const string SalesOrder = "SalesOrder";
            public const string GlEntry = "GlEntry";
            public const string GlReversal = "GlReversal";
            public const string BankTransaction = "BankTransaction";
            public const string BankReceipt = "BankReceipt";
            public const string BankReversal = "BankReversal";
            public const string BankTransferTransaction = "BankTransferTransaction";
            public const string BankTransferReversal = "BankTransferReversal";
            public const string CreditNote = "CreditNote";
            public const string DebitNote = "DebitNote";
            public const string TaxSettlement = "TaxSettlement";
            public const string EntityName = "EntityName";
            public const string ParentRegForm = "ParentRegForm";
            public const string ContractNumber = "ContractNumber";
            public const string Payment = "Payment";
            public const string Student = "StudentNumber";
        }

        public static class GLModules
        {
            public const string GeneralLedger = "General Ledger";
            public const string Bank = "Bank";
            public const string Sales = "Sales";
            public const string Purchase = "Purchase";
            public const string Inventory = "Inventory";
        }

        public static class GLDocType
        {
            public const string Entry = "Entry";
            public const string Reversal = "Reversal";
        }

        public static class GlReferenceType
        {
            public const string GlEntry = "GlEntry";
            public const string GlReversal = "GlReversal";
            public const string BankTransaction = "BankTransaction";
            public const string BankTransactionReceipt = "BankTransactionReceipt";
            public const string BankTransactionReversal = "BankTransactionReversal";
            public const string BankTransfer = "BankTransfer";
            public const string BankTransferReversal = "BankTransferReversal";
            public const string SalesCashReceipt = "SalesCashReceipt";
            public const string SalesInvoice = "SalesInvoice";
            public const string SalesReturn = "SalesReturn";
            public const string SalesPOSInvoice = "SalesPOSInvoice";
            public const string SalesPOSReturn = "SalesPOSReturn";
            public const string PurchasePayment = "PurchasePayment";
            public const string PurchaseCreditNote = "PurchaseCreditNote";
            public const string PurchaseDebitNote = "PurchaseDebitNote";
            public const string PurchaseStandardInvoice = "PurchaseStandardInvoice";
            public const string PurchasePOInvoice = "PurchasePOInvoice";
            public const string PurchaseReceivingInvoice = "PurchaseReceivingInvoice";
            public const string PurchaseReceiving = "PurchaseReceiving";
            public const string PurchaseLandedCostInvoice = "PurchaseLandedCostInvoice";
            public const string InventoryAdjustment = "InventoryAdjustment";
        }

        public static string[] FileType_Supported = new string[] { ".jpg", ".jpeg", ".jfif", ".pjpeg", ".pjp ", ".png", ".gif", ".pdf" };
        public static string[] ImageFileType_Supported = new string[] { ".jpg", ".jpeg", ".png", ".gif" };

        public static class TransferStatus
        {
            public const string Saved = "Saved";
            public const string Submitted = "Submitted";
            public const string Posted = "Posted";
            public const string Transferred = "Transferred";
            public const string Received = "Received";
        }

        public static class StudentContractStatus
        {
            public const string Active = "Active";
        }

    

        public static class AttachmentReferenceType
        {
            public const string Student = "Student";
        }

        public static class PurchaseHistoryType
        {
            public const string PO = "Purchase Order";
            public const string Receiving = "Receiving";
            public const string InvoiceAgainstPO = "PO Invoice";
            public const string InvoiceAgainstReceiving = "Receiving Invoice";
            public const string LandedCostInvoice = "Landed Cost Invoice";
            public const string Standard = "Standard Invoice";
            public const string Payment = "Payment";
            public const string CreditNote = "Credit Note";
            public const string DebitNote = "Debit Note";
            public const string TransferRequest = "Transfer Request";
            public const string TransferIssuance = "Transfer Issuance";
            public const string TransferReceiving = "Transfer Receiving";
            public const string Adjustment = "Adjustment";
        }

        public static string CompanyLogoPath = "/Upload/CompanyLogo/";

    }
}
