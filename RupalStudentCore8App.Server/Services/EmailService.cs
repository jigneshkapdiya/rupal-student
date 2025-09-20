using RupalStudentCore8App.Server.Class.Configuration;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Logging;

namespace RupalStudentCore8App.Server.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string mailText, string subject, string email, string name, Stream? uploadFiles = null);
        Task<bool> SendPasswordResetLinkAsync(string toEmail, string toName, string link);
        Task<bool> SendContactSupportEmailAsync(string Email, string toEmail, string description, string tokan, string name, string supportType, Stream uploadFiles);

    }


    public class EmailService : IEmailService
    {
        #region Init
        public readonly IWebHostEnvironment _webHostEnvironment;
        private readonly AppSettings _appSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IOptionsSnapshot<AppSettings> appSettings,
            ILogger<EmailService> logger,
            IWebHostEnvironment webHostEnvironment
            )
        {
            _appSettings = appSettings.Value;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }
        #endregion

        public async Task<bool> SendEmailAsync(
            string mailText,
            string subject,
            string email, 
            string name, 
            Stream? uploadFiles = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    _logger.LogWarning("SendEmailAsync called with empty recipient email.");
                    return false;
                }
                if (string.IsNullOrWhiteSpace(subject))
                {
                    _logger.LogWarning("SendEmailAsync called with empty subject.");
                    return false;
                }
                if (string.IsNullOrWhiteSpace(mailText))
                {
                    _logger.LogWarning("SendEmailAsync called with empty body.");
                    return false;
                }

                // Addresses
                var toMailAddress = GetMailAddress(email, name);
                var fromEmailAddress = _appSettings.Email.DefaultFrom.Address ?? string.Empty;
                var fromDisplayName = _appSettings.Email.DefaultFrom.DisplayName ?? string.Empty;
                var fromMailAddress = GetMailAddress(fromEmailAddress, fromDisplayName);

                // SMTP
                using var smtp = GetSMTPSettings();

                // Message
                using var mail = new MailMessage
                {
                    From = fromMailAddress,
                    Subject = subject,
                    SubjectEncoding = Encoding.UTF8,
                    BodyEncoding = Encoding.UTF8,
                    HeadersEncoding = Encoding.UTF8,
                    IsBodyHtml = true,
                    Body = mailText
                };
                mail.To.Add(toMailAddress);

                // Attachment (optional)
                if (uploadFiles != null)
                {
                    if (uploadFiles.CanSeek) uploadFiles.Position = 0;
                    mail.Attachments.Add(new System.Net.Mail.Attachment(uploadFiles, "attachment"));
                }

                if (_appSettings.Email.AllowSending)
                    await smtp.SendMailAsync(mail);
                else
                    _logger.LogWarning("Email sending is disabled in configuration");

                mail.Dispose();
                return true;
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "SMTP error sending email");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email");
                return false;
            }
        }

        public async Task<bool> SendPasswordResetLinkAsync(string toEmail, string toName, string resetUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(toEmail) || string.IsNullOrWhiteSpace(resetUrl))
                {
                    _logger.LogWarning("SendPasswordResetLinkAsync called with invalid parameters.");
                    return false;
                }

                const string subject = "Password Reset Request";
                const string templateName = "ForgetPasswordTemplate.html";
                var mailText = GetHtmlTemplate(templateName);

                if (mailText != null)
                {
                    mailText = mailText
                        .Replace("##NAME##", toName ?? string.Empty)
                        .Replace("##RESET_URL##", resetUrl)
                        .Replace("##RESET_URL##", resetUrl);

                    return await SendEmailAsync(mailText, subject, toEmail, toName);
                }
                else
                {
                    _logger.LogInformation("Template '{Template}' not found. Falling back to inline body.", templateName);

                    var safeName = System.Net.WebUtility.HtmlEncode(toName ?? string.Empty);
                    var safeUrl = resetUrl; // Assuming resetUrl is already a safe absolute URL
                    var emailBody =
                        $"Hello {safeName},<br/>" +
                        $"Please reset your password by clicking <a href='{safeUrl}'>here</a>.<br/>" +
                        $"If you did not request this reset, please ignore this email.";

                    // Note: keep correct parameter order (mailText, subject, email, name)
                    return await SendEmailAsync(emailBody, subject, toEmail, toName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Forget Password Mail Send Error");
                return false;
            }
        }


        public async Task<bool> SendContactSupportEmailAsync(
            string Email,
            string toEmail,
            string description,
            string token,
            string name,
            string supportType,
            Stream uploadFiles)
        {
            try
            {
                // Basic validation
                if (string.IsNullOrWhiteSpace(toEmail))
                {
                    _logger.LogWarning("SendContactSupportEmailAsync called with empty recipient email.");
                    return false;
                }

                const string subject = "Support";
                const string templateName = "ContactSupportTemplate.html";
                var mailText = GetHtmlTemplate(templateName);

                // Encode user inputs for HTML safety
                var safeEmail = System.Net.WebUtility.HtmlEncode(Email ?? string.Empty);
                var safeSupportType = System.Net.WebUtility.HtmlEncode(supportType ?? string.Empty);
                var safeDescription = System.Net.WebUtility.HtmlEncode(description ?? string.Empty);
                var safeToken = System.Net.WebUtility.HtmlEncode(token ?? string.Empty);
                var safeName = System.Net.WebUtility.HtmlEncode(name ?? string.Empty);

                if (mailText != null)
                {
                    mailText = mailText
                        .Replace("##EMAIL##", safeEmail)
                        .Replace("##SUPPORT_TYPE##", safeSupportType)
                        .Replace("##DESCRIPTION##", safeDescription)
                        .Replace("##TOKEN##", safeToken);

                    // Delegate sending to common method
                    return await SendEmailAsync(mailText, subject, toEmail, name, uploadFiles);
                }
                else
                {
                    _logger.LogInformation("Template '{Template}' not found. Falling back to inline body.", templateName);

                    var fallbackBody =
                        $"Hello {safeName},<br/>" +
                        $"Support request received.<br/>" +
                        $"Type: {safeSupportType}<br/>" +
                        $"From: {safeEmail}<br/>" +
                        $"Token: {safeToken}<br/>" +
                        $"Description:<br/>{safeDescription.Replace("\n", "<br/>")}";

                    return await SendEmailAsync(fallbackBody, subject, toEmail, name, uploadFiles);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Support Mail Send Error");
                return false;
            }
        }

        #region private methods
        private static MailAddress GetMailAddress(string emailAddress, string? displayName)
        {
            if (string.IsNullOrWhiteSpace(emailAddress))
                throw new ArgumentException("Email address is required.", nameof(emailAddress));

            // Treat whitespace as empty and trim
            var name = string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim();

            // Prevent header injection in display name
            if (name is not null)
            {
                name = name.Replace("\r", string.Empty)
                           .Replace("\n", string.Empty);
            }

            return name is null
                ? new MailAddress(emailAddress)
                : new MailAddress(emailAddress, name);
        }


        private SmtpClient GetSMTPSettings()
        {
            var smtpCfg = _appSettings.Email.Smtp;

            if (string.IsNullOrWhiteSpace(smtpCfg.Host))
                throw new InvalidOperationException("SMTP Host is not configured.");

            if (smtpCfg.Port <= 0 || smtpCfg.Port > 65535)
                throw new InvalidOperationException($"Invalid SMTP port: {smtpCfg.Port}");

            var smtp = new SmtpClient(smtpCfg.Host, smtpCfg.Port)
            {
                EnableSsl = smtpCfg.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 10000
            };

            if (!string.IsNullOrWhiteSpace(smtpCfg.UserName) &&
                !string.IsNullOrWhiteSpace(smtpCfg.Password))
            {
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential(smtpCfg.UserName, smtpCfg.Password);
            }
            else
            {
                // Use the process/domain account
                smtp.UseDefaultCredentials = true;
                // Do NOT set smtp.Credentials here; it's ignored when UseDefaultCredentials = true
            }

            return smtp;
        }


        private static readonly ConcurrentDictionary<string, string> _templateCache = new();
        private string? GetHtmlTemplate(string templateName)
        {
            // Validate input to avoid path traversal
            if (string.IsNullOrWhiteSpace(templateName) || templateName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                _logger.LogWarning("Invalid template name: {TemplateName}", templateName);
                return null;
            }

            // Cache hit
            if (_templateCache.TryGetValue(templateName, out var cached))
                return cached;

            try
            {
                // Prefer WebRootFileProvider to resolve files
                var provider = _webHostEnvironment.WebRootFileProvider;
                var filePath = Path.Combine("EmailTemplate", templateName);
                var fileInfo = provider.GetFileInfo(filePath);

                if (!fileInfo.Exists)
                {
                    _logger.LogError("Email template not found at path: {Path}", filePath);
                    return null;
                }

                using var stream = fileInfo.CreateReadStream();
                using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
                var html = reader.ReadToEnd();

                // Cache for future requests
                _templateCache[templateName] = html;
                return html;
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogError(ex, "Email template file not found: {TemplateName}", templateName);
                return null;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Access denied when reading email template: {TemplateName}", templateName);
                return null;
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "I/O error when reading email template: {TemplateName}", templateName);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error reading email template: {TemplateName}", templateName);
                return null;
            }
        }


        //private string GetHtmlTemplate(string TemplateName)
        //{
        //    var HTMLstring = new BodyBuilder();
        //    try
        //    {
        //        string ContentRootPath = _webHostEnvironment.WebRootPath;
        //        string FolderName = "\\EmailTemplate\\";
        //        string NewPath = ContentRootPath + FolderName;
        //        var PathToFile = NewPath + TemplateName;

        //        using (StreamReader SourceReader = File.OpenText(PathToFile))
        //        {
        //            HTMLstring.HtmlBody = SourceReader.ReadToEnd();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error reading email template {TemplateName}", TemplateName);
        //        return "0";
        //    }
        //    return HTMLstring.HtmlBody;
        //}
        #endregion
    }
}
