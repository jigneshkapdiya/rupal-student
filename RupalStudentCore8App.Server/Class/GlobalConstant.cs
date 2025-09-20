namespace RupalStudentCore8App.Server.Class
{
    public class GlobalConstant
    {
        public static class LoginProvider
        {
            public const string Local = "Local";
            public const string TwoFactor = "TwoFactor";
            public const string Google = "Google";
        }
        public static class Common
        {
            public const string PhoneCode = "+966";
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
            public const string StudentImage = "StudentImage";
        }
        public static class AutoIncrement
        {
            public const string Student = "StudentNumber";
        }

        public static string[] FileType_Supported = new string[] { ".jpg", ".jpeg", ".jfif", ".pjpeg", ".pjp ", ".png", ".gif", ".pdf" };
        public static string[] ImageFileType_Supported = new string[] { ".jpg", ".jpeg", ".png", ".gif" };
        public static class StudentContractStatus
        {
            public const string Active = "Active";
        }

        public static class AttachmentReferenceType
        {
            public const string Student = "Student";
        }
    }
}
