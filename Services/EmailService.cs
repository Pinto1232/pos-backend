using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PosBackend.Models;

namespace PosBackend.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);
        Task<bool> SendCardExpirationWarningAsync(string userEmail, string userName, DateTime expirationDate, int daysUntilExpiration);
        Task<bool> SendUpcomingPaymentReminderAsync(string userEmail, string userName, decimal amount, DateTime nextBillingDate, int daysUntilBilling);
        Task<bool> SendPaymentFailedNotificationAsync(string userEmail, string userName, decimal amount, string failureReason, DateTime nextRetryDate);
        Task<bool> SendPaymentRetrySuccessAsync(string userEmail, string userName, decimal amount, DateTime paidDate);
        Task<bool> SendSubscriptionCancellationWarningAsync(string userEmail, string userName, DateTime cancellationDate);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly EmailConfiguration _emailConfig;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _emailConfig = new EmailConfiguration();
            _configuration.GetSection("Email").Bind(_emailConfig);
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                if (!_emailConfig.EnableEmailNotifications)
                {
                    _logger.LogInformation("Email notifications are disabled. Skipping email to {To}", to);
                    return true;
                }

                using var client = new SmtpClient(_emailConfig.SmtpServer, _emailConfig.SmtpPort);
                client.EnableSsl = _emailConfig.EnableSsl;
                client.Credentials = new NetworkCredential(_emailConfig.SmtpUsername, _emailConfig.SmtpPassword);

                using var message = new MailMessage();
                message.From = new MailAddress(_emailConfig.FromEmail, _emailConfig.FromName);
                message.To.Add(to);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = isHtml;

                await client.SendMailAsync(message);
                _logger.LogInformation("Email sent successfully to {To} with subject: {Subject}", to, subject);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To} with subject: {Subject}", to, subject);
                return false;
            }
        }

        public async Task<bool> SendCardExpirationWarningAsync(string userEmail, string userName, DateTime expirationDate, int daysUntilExpiration)
        {
            var subject = $"Payment Card Expiring Soon - Action Required";
            var body = GenerateCardExpirationWarningEmail(userName, expirationDate, daysUntilExpiration);
            return await SendEmailAsync(userEmail, subject, body);
        }

        public async Task<bool> SendUpcomingPaymentReminderAsync(string userEmail, string userName, decimal amount, DateTime nextBillingDate, int daysUntilBilling)
        {
            var subject = $"Upcoming Payment Reminder - ${amount:F2}";
            var body = GenerateUpcomingPaymentReminderEmail(userName, amount, nextBillingDate, daysUntilBilling);
            return await SendEmailAsync(userEmail, subject, body);
        }

        public async Task<bool> SendPaymentFailedNotificationAsync(string userEmail, string userName, decimal amount, string failureReason, DateTime nextRetryDate)
        {
            var subject = "Payment Failed - Action Required";
            var body = GeneratePaymentFailedNotificationEmail(userName, amount, failureReason, nextRetryDate);
            return await SendEmailAsync(userEmail, subject, body);
        }

        public async Task<bool> SendPaymentRetrySuccessAsync(string userEmail, string userName, decimal amount, DateTime paidDate)
        {
            var subject = "Payment Successful - Thank You";
            var body = GeneratePaymentRetrySuccessEmail(userName, amount, paidDate);
            return await SendEmailAsync(userEmail, subject, body);
        }

        public async Task<bool> SendSubscriptionCancellationWarningAsync(string userEmail, string userName, DateTime cancellationDate)
        {
            var subject = "Subscription Cancellation Warning";
            var body = GenerateSubscriptionCancellationWarningEmail(userName, cancellationDate);
            return await SendEmailAsync(userEmail, subject, body);
        }

        private static string GenerateCardExpirationWarningEmail(string userName, DateTime expirationDate, int daysUntilExpiration)
        {
            var urgencyMessage = daysUntilExpiration switch
            {
                1 => "Your payment card expires tomorrow!",
                <= 7 => $"Your payment card expires in {daysUntilExpiration} days.",
                _ => $"Your payment card expires in {daysUntilExpiration} days."
            };

            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #f8f9fa; padding: 20px; text-align: center; border-radius: 5px; }}
        .content {{ padding: 20px 0; }}
        .warning {{ background-color: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; border-radius: 5px; margin: 20px 0; }}
        .button {{ display: inline-block; background-color: #007bff; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; margin: 10px 0; }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>Payment Card Expiration Notice</h2>
        </div>
        <div class='content'>
            <p>Dear {userName},</p>

            <div class='warning'>
                <strong>⚠️ {urgencyMessage}</strong>
            </div>

            <p>To ensure uninterrupted service, please update your payment method before <strong>{expirationDate:MMMM dd, yyyy}</strong>.</p>

            <p>You can update your payment information by:</p>
            <ul>
                <li>Logging into your account dashboard</li>
                <li>Going to the billing section</li>
                <li>Adding a new payment method</li>
            </ul>

            <a href='#' class='button'>Update Payment Method</a>

            <p>If you have any questions, please don't hesitate to contact our support team.</p>

            <p>Best regards,<br>The PisVal POS Team</p>
        </div>
        <div class='footer'>
            <p>This is an automated message. Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";
        }

        private static string GenerateUpcomingPaymentReminderEmail(string userName, decimal amount, DateTime nextBillingDate, int daysUntilBilling)
        {
            var reminderMessage = daysUntilBilling switch
            {
                1 => "Your subscription will be charged tomorrow.",
                _ => $"Your subscription will be charged in {daysUntilBilling} days."
            };

            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #f8f9fa; padding: 20px; text-align: center; border-radius: 5px; }}
        .content {{ padding: 20px 0; }}
        .info {{ background-color: #d1ecf1; border: 1px solid #bee5eb; padding: 15px; border-radius: 5px; margin: 20px 0; }}
        .amount {{ font-size: 24px; font-weight: bold; color: #007bff; }}
        .button {{ display: inline-block; background-color: #28a745; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; margin: 10px 0; }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>Upcoming Payment Reminder</h2>
        </div>
        <div class='content'>
            <p>Dear {userName},</p>

            <div class='info'>
                <p><strong>📅 {reminderMessage}</strong></p>
                <p>Amount: <span class='amount'>${amount:F2}</span></p>
                <p>Billing Date: <strong>{nextBillingDate:MMMM dd, yyyy}</strong></p>
            </div>

            <p>Please ensure your payment method is up to date to avoid any service interruptions.</p>

            <a href='#' class='button'>View Billing Details</a>

            <p>Thank you for being a valued customer!</p>

            <p>Best regards,<br>The PisVal POS Team</p>
        </div>
        <div class='footer'>
            <p>This is an automated message. Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";
        }

        private static string GeneratePaymentFailedNotificationEmail(string userName, decimal amount, string failureReason, DateTime nextRetryDate)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #f8f9fa; padding: 20px; text-align: center; border-radius: 5px; }}
        .content {{ padding: 20px 0; }}
        .error {{ background-color: #f8d7da; border: 1px solid #f5c6cb; padding: 15px; border-radius: 5px; margin: 20px 0; }}
        .button {{ display: inline-block; background-color: #dc3545; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; margin: 10px 0; }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>Payment Failed - Action Required</h2>
        </div>
        <div class='content'>
            <p>Dear {userName},</p>

            <div class='error'>
                <p><strong>❌ Your recent payment of ${amount:F2} was unsuccessful.</strong></p>
                <p>Reason: {failureReason}</p>
            </div>

            <p>We will automatically retry your payment on <strong>{nextRetryDate:MMMM dd, yyyy}</strong>.</p>

            <p>To resolve this issue immediately:</p>
            <ul>
                <li>Check that your payment method is valid and has sufficient funds</li>
                <li>Update your payment information if needed</li>
                <li>Contact your bank if you suspect the payment was blocked</li>
            </ul>

            <a href='#' class='button'>Update Payment Method</a>

            <p>If you need assistance, please contact our support team.</p>

            <p>Best regards,<br>The PisVal POS Team</p>
        </div>
        <div class='footer'>
            <p>This is an automated message. Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";
        }

        private static string GeneratePaymentRetrySuccessEmail(string userName, decimal amount, DateTime paidDate)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #f8f9fa; padding: 20px; text-align: center; border-radius: 5px; }}
        .content {{ padding: 20px 0; }}
        .success {{ background-color: #d4edda; border: 1px solid #c3e6cb; padding: 15px; border-radius: 5px; margin: 20px 0; }}
        .button {{ display: inline-block; background-color: #28a745; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; margin: 10px 0; }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>Payment Successful</h2>
        </div>
        <div class='content'>
            <p>Dear {userName},</p>

            <div class='success'>
                <p><strong>✅ Your payment of ${amount:F2} was processed successfully!</strong></p>
                <p>Payment Date: {paidDate:MMMM dd, yyyy}</p>
            </div>

            <p>Thank you for resolving the payment issue. Your subscription is now active and up to date.</p>

            <a href='#' class='button'>View Receipt</a>

            <p>We appreciate your business!</p>

            <p>Best regards,<br>The PisVal POS Team</p>
        </div>
        <div class='footer'>
            <p>This is an automated message. Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";
        }

        private static string GenerateSubscriptionCancellationWarningEmail(string userName, DateTime cancellationDate)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #f8f9fa; padding: 20px; text-align: center; border-radius: 5px; }}
        .content {{ padding: 20px 0; }}
        .warning {{ background-color: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; border-radius: 5px; margin: 20px 0; }}
        .button {{ display: inline-block; background-color: #ffc107; color: #212529; padding: 12px 24px; text-decoration: none; border-radius: 5px; margin: 10px 0; }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>Subscription Cancellation Warning</h2>
        </div>
        <div class='content'>
            <p>Dear {userName},</p>

            <div class='warning'>
                <p><strong>⚠️ Your subscription will be cancelled on {cancellationDate:MMMM dd, yyyy} due to repeated payment failures.</strong></p>
            </div>

            <p>To prevent cancellation and maintain access to your POS system:</p>
            <ul>
                <li>Update your payment method immediately</li>
                <li>Ensure sufficient funds are available</li>
                <li>Contact us if you need assistance</li>
            </ul>

            <a href='#' class='button'>Prevent Cancellation</a>

            <p>We value your business and want to help resolve this issue.</p>

            <p>Best regards,<br>The PisVal POS Team</p>
        </div>
        <div class='footer'>
            <p>This is an automated message. Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";
        }
    }

    public class EmailConfiguration
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string SmtpUsername { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
        public bool EnableEmailNotifications { get; set; } = true;
    }
}
