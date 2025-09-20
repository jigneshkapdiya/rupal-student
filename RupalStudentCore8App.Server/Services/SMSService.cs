using RupalStudentCore8App.Server.Models;
using RupalStudentCore8App.Server.ServiceModels;
using Newtonsoft.Json;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using System.Reflection;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.Json;
using RupalStudentCore8App.Server.Class.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace RupalStudentCore8App.Server.Services
{
    public interface ISMSService
    {
        Task<SMSResponseModel> SendSmsAsync(string smsText, string phoneNumber, string phoneCode = "966");
        Task<SMSResponseModel> SendSmsAsync(SMSBodyModel req);
        Task<SMSResponseModel> SendPhoneCodeVerification(string phoneNumber, string otp, bool isPhoneNumberConfirmed);
        Task<SMSResponseModel> SendResetPasswordLink(string phoneNumber, string link);
        Task<SMSResponseModel> SendConfirmSMS(string phoneNumber, string otp);
        Task<SMSResponseModel> SendStudentRegistration(SMSRequestModel req);
        Task<SMSResponseModel> SendOTPForRegistration(string phoneNumber, string otp);
    }

    public class SMSService : ISMSService
    {
        private readonly AppSettings _appSettings;
        private readonly ILogger<SMSService> _logger;
        private readonly IConfiguration _configuration;

        public SMSService(IOptionsSnapshot<AppSettings> appSettings, ILogger<SMSService> logger, IConfiguration configuration)
        {
            _appSettings = appSettings.Value;
            _logger = logger;
            _configuration = configuration;
        }

        // implement SendSmsAsync
        // Send SMS asynchronously
        public async Task<SMSResponseModel> SendSmsAsync(string smsText, string phoneNumber, string phoneCode = "966")
        {
            SMSResponseModel res = new SMSResponseModel();
            try
            {
                SMSBodyModel req = new SMSBodyModel();
                req.sender = Convert.ToString(_configuration["SMS:SenderName"]) ?? string.Empty;
                req.recipients = new List<long>() { Convert.ToInt64(phoneCode + phoneNumber) };
                req.body = smsText;
                res = await SendSmsAsync(req);
            }
            catch (Exception ex)
            {
                res.isException = true;
                res.message = ex.Message;
                _logger.LogError(ex, "Fail to send SMS");
            }
            return res;
        }

        public async Task<SMSResponseModel> SendSmsAsync(SMSBodyModel req)
        {
            SMSResponseModel res = new SMSResponseModel
            {
                isException = false
            };
            string url = _appSettings.SMS.ApiUrl ?? string.Empty;
            string token = _appSettings.SMS.Token ?? string.Empty;

            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(token))
            {

                res.isException = true;
                res.message = "SMS API URL or Token is not configured properly";
                _logger.LogError("SMS API URL or Token is not configured properly");
                return res;
            }

            if (!_appSettings.SMS.AllowSending)
            {
                _logger.LogWarning("SMS sending is disabled in configuration");
                res.message = "SMS sent successfully";
                return res;
            }

            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(url);
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                StringContent content = new StringContent(JsonConvert.SerializeObject(req), Encoding.UTF8, "application/json");
                using (var response = await httpClient.PostAsync("", content))
                {
                    string apiRes = await response.Content.ReadAsStringAsync();

                    // With this null-safe version:
                    var deserializedRes = JsonConvert.DeserializeObject<SMSResponseModel>(apiRes);
                    if (deserializedRes != null)
                    {
                        // statusCode=201 :- SMS send successfully
                        // statusCode=400 :- its mean the request has not submitted successfully
                        // statusCode=401 :- you are using wrong bearer Tokens
                        // statusCode=405 :- you are using RESTFull API and have used unauthorized method
                        // statusCode=500 :- its mean the server has some issue

                        if (deserializedRes.statusCode == 201)
                        {
                            res.isException = false;
                            res.message = "SMS sent successfully";
                        }
                        else
                        {
                            _logger.LogError("SMS API Error, Error : {apiRes}", apiRes);
                            res.isException = true;
                            res.message = "Failed to send SMS";
                        }
                    }
                    else
                    {
                        _logger.LogError("Failed to deserialize SMS API response. Response: " + apiRes);
                        res.isException = true;
                        res.message = "Failed to send SMS";
                    }
                }
            }
            return res;
        }

        public async Task<SMSResponseModel> SendPhoneCodeVerification(string phoneNumber, string otp, bool isPhoneNumberConfirmed)
        {
            SMSResponseModel res = new SMSResponseModel();
            try
            {
                SMSBodyModel req = new SMSBodyModel
                {
                    sender = Convert.ToString(_configuration["SMS:SenderName"]) ?? string.Empty,
                    recipients = new List<long>() { Convert.ToInt64(phoneNumber) }
                };

                #region SMS Body In Arabic: New
                if (isPhoneNumberConfirmed)
                {
                    req.body = "Your Otp Code is" + otp;
                }
                else
                {
                    req.body = "Your Otp Code is" + otp;
                }
                #endregion

                res = await SendSmsAsync(req);
            }
            catch (Exception ex)
            {
                res.isException = true;
                res.message = ex.Message;
                _logger.LogError(ex, "Fail to send SMS");
            }
            return res;
        }


        public async Task<SMSResponseModel> SendOTPForRegistration(string phoneNumber, string otp)
        {
            SMSResponseModel res = new SMSResponseModel();
            try
            {
                SMSBodyModel req = new SMSBodyModel
                {
                    sender = Convert.ToString(_configuration["SMS:SenderName"]) ?? string.Empty,
                    recipients = new List<long>() { Convert.ToInt64(phoneNumber) }
                };

                #region SMS Body In Arabic: New

                req.body = "Your Otp Code is" + otp;
                #endregion

                res = await SendSmsAsync(req);
            }
            catch (Exception ex)
            {
                res.isException = true;
                res.message = ex.Message;
                _logger.LogError(ex, "Fail to send SMS");
            }
            return res;
        }

        //Send Reset Password Link
        public async Task<SMSResponseModel> SendResetPasswordLink(string phoneNumber, string link)
        {
            SMSResponseModel res = new SMSResponseModel();
            try
            {
                SMSBodyModel req = new SMSBodyModel
                {
                    sender = Convert.ToString(_configuration["SMS:SenderName"]) ?? string.Empty,
                    recipients = new List<long>() { Convert.ToInt64(phoneNumber) },

                    body = "Click on the link to reset your password: " + link
                };

                res = await SendSmsAsync(req);
            }
            catch (Exception ex)
            {
                res.isException = true;
                res.message = ex.Message;
                _logger.LogError(ex, "Fail to send SMS");
            }
            return res;
        }

        public async Task<SMSResponseModel> SendConfirmSMS(string phoneNumber, string otp)
        {
            SMSResponseModel res = new SMSResponseModel();
            try
            {
                SMSBodyModel req = new SMSBodyModel();
                req.sender = Convert.ToString(_configuration["SMS:SenderName"]) ?? string.Empty;
                req.recipients = new List<long>() { Convert.ToInt64("966" + phoneNumber) };

                #region SMS Body In Arabic: New
                req.body = otp + " is your CashApp login OTP.The code will be valid for 10 min.Do not share this OTP with anyone.";
                #endregion
                res = await SendSmsAsync(req);
            }
            catch (Exception ex)
            {
                res.isException = true;
                res.message = ex.Message;
                _logger.LogError(ex, "Fail to send SMS");
            }
            return res;
        }

        public async Task<SMSResponseModel> SendStudentRegistration(SMSRequestModel model)
        {
            SMSResponseModel response = new SMSResponseModel();
            try
            {
                SMSBodyModel req = new SMSBodyModel();
                req.sender = Convert.ToString(_configuration["SMS:SenderName"]) ?? string.Empty;
                req.recipients = new List<long>() { Convert.ToInt64("966" + model.SenderContactNo) };
                req.body = "Dear Parent, Your Child " + (model.StudentName) + "contract for the academoc year" + (model.AcademicYear) + "is ready requires your approval. To electronically approve the contract, please click on the link below:";
                string webAppUrl = _appSettings.Authentication.WebAppUrl ?? string.Empty;

                // Ensure it always ends with a slash
                if (!webAppUrl.EndsWith("/"))
                {
                    webAppUrl += "/";
                }

                string fullUrl = $"{webAppUrl}parent/sign-in/{model.guid}";
                string link = fullUrl;
                response = await SendSmsAsync(req);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error to sending SMS");
                response.statusCode = 400;
                response.message = "Error in sending SMS.";
            }
            return response;
        }
    }
}
