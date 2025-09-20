using RupalStudentCore8App.Server.Class;
using Dapper;
using System.Data;
using static RupalStudentCore8App.Server.Class.GlobalConstant;

namespace RupalStudentCore8App.Server.Services
{
    public interface IUtility
    {
        string AutoIncrement(string Entity, bool IsUpdate = false);
        string UploadFile(IFormFile formFile, string filePath, string prefixName = "", string fileExtension = "");
        string GetFileUrl(string filename, string fileType = "");
        string UploadImages(IFormFile formFile, string path, string prefixName = "", string fileExtension = "");
        string GetImageUrl(string filename, string pathType = "");
    }
    public class Utility : IUtility
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _WebHostEnvironment;
        public Utility(IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor)
        {
            _WebHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public string AutoIncrement(string Entity, bool IsUpdate = false)
        {
            string nextNumber = string.Empty;
            using (IDbConnection db = new DbFactory(Config.DefaultConnectionString).db)
            {
                nextNumber = db.QueryFirstOrDefault<string>("SP_AutoIncrement", new { Entity, IsUpdate }, null, null, CommandType.StoredProcedure) ?? string.Empty; ;
            }
            return nextNumber;
        }
        /// <summary>
        /// Upload files to wwwroot/upload/*
        /// </summary>
        /// <param name="formFile">File to upload</param>
        /// <param name="filePath">File upload path</param>
        /// <param name="prefixName"></param>
        /// <param name="fileExtension"></param>
        /// <returns></returns>

        public string UploadFile(IFormFile formFile, string filePath, string prefixName = "", string fileExtension = "")
        {
            if (formFile != null)
            {
                string fileUploadPath = _WebHostEnvironment.WebRootPath + filePath;
                if (string.IsNullOrEmpty(fileExtension))
                {
                    fileExtension = Path.GetExtension(formFile.FileName).ToLower();
                }

                if (!Directory.Exists(fileUploadPath))
                    Directory.CreateDirectory(fileUploadPath);
                Guid guid = Guid.NewGuid();
                string newFileName = prefixName + "_" + guid.ToString() + fileExtension;
                string fullPath = fileUploadPath + newFileName;
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    formFile.CopyTo(stream);
                }
                return newFileName;
            }
            else
            {
                return string.Empty;
            }
        }

        public string GetFileUrl(string filename, string fileType = "")
        {
            if (!string.IsNullOrWhiteSpace(filename))
            {
                string filePath = string.Empty;
                switch (fileType)
                {
                    case FileType.ProfileImage:
                        filePath = FileType.GetPath(FileType.ProfileImage);
                        break;
                    case FileType.Attachment:
                        filePath = FileType.GetPath(FileType.Attachment);
                        break;
                    case FileType.Student:
                        filePath = FileType.GetPath(FileType.Student);
                        break;
                    default:
                        filePath = FilePath.Default;
                        break;
                }
                return _httpContextAccessor.HttpContext.Request.Scheme + "://" + _httpContextAccessor.HttpContext.Request.Host.Value + filePath + filename;
            }
            else
                return string.Empty;
        }

        #region Image And Document
        public string UploadImages(IFormFile formFile, string path, string prefixName = "", string fileExtension = "")
        {
                if (formFile != null)
                {
                    string fileUploadPath = path;
                    if (string.IsNullOrEmpty(fileExtension))
                    {
                        fileExtension = Path.GetExtension(formFile.FileName).ToLower();
                    }
                    if (!Directory.Exists(fileUploadPath))
                        Directory.CreateDirectory(fileUploadPath);
                    Guid guid = Guid.NewGuid();
                    string newFileName = prefixName + "_" + guid.ToString() + fileExtension;
                    string fullPath = fileUploadPath + newFileName;
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        formFile.CopyTo(stream);
                    }
                    return newFileName;
                }
                else
                {
                    return string.Empty;
                }
         
        }
        public string GetImageUrl(string filename, string pathType = "")
        {
            if (!string.IsNullOrWhiteSpace(filename))
            {
                string filePath = string.Empty;
                switch (pathType)
                {
                    case GlobalConstant.FilePathType.Company:
                        filePath = GlobalConstant.CompanyLogoPath;
                        break;
                    case GlobalConstant.FilePathType.StudentImage:
                        filePath = GlobalConstant.FilePath.Student;
                        break;
                }
                return _httpContextAccessor.HttpContext.Request.Scheme + "://" + _httpContextAccessor.HttpContext.Request.Host.Value + filePath + filename;
            }
            else
                return string.Empty;
        }
        #endregion

    }
}
