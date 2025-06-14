using System.ComponentModel.DataAnnotations;
using PosBackend.Services;

namespace PosBackend.Attributes
{
    /// <summary>
    /// Validates and sanitizes string input to prevent XSS and injection attacks
    /// </summary>
    public class SanitizeStringAttribute : ValidationAttribute
    {
        private readonly InputType _inputType;
        private readonly int _maxLength;
        private readonly bool _allowEmpty;

        public SanitizeStringAttribute(InputType inputType = InputType.PlainText, int maxLength = 1000, bool allowEmpty = false)
        {
            _inputType = inputType;
            _maxLength = maxLength;
            _allowEmpty = allowEmpty;
        }

        public override bool IsValid(object? value)
        {
            if (value == null || (value is string str && string.IsNullOrEmpty(str)))
                return _allowEmpty;

            if (value is not string stringValue)
                return false;

            // Check length
            if (stringValue.Length > _maxLength)
                return false;

            // Check for dangerous patterns
            var sanitizationService = new InputSanitizationService(
                Microsoft.Extensions.Logging.Abstractions.NullLogger<InputSanitizationService>.Instance);
            
            return sanitizationService.IsValidInput(stringValue, _inputType);
        }

        public override string FormatErrorMessage(string name)
        {
            return $"The {name} field contains invalid or potentially dangerous content.";
        }
    }

    /// <summary>
    /// Validates email format and prevents injection
    /// </summary>
    public class SafeEmailAttribute : RegularExpressionAttribute
    {
        public SafeEmailAttribute() : base(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")
        {
            ErrorMessage = "Please enter a valid email address.";
        }
    }

    /// <summary>
    /// Validates username format
    /// </summary>
    public class SafeUsernameAttribute : RegularExpressionAttribute
    {
        public SafeUsernameAttribute() : base(@"^[a-zA-Z0-9_-]{3,50}$")
        {
            ErrorMessage = "Username must be 3-50 characters long and contain only letters, numbers, underscores, and hyphens.";
        }
    }

    /// <summary>
    /// Validates URL format and prevents dangerous protocols
    /// </summary>
    public class SafeUrlAttribute : RegularExpressionAttribute
    {
        public SafeUrlAttribute() : base(@"^https?://[a-zA-Z0-9.-]+(/.*)?$")
        {
            ErrorMessage = "Please enter a valid HTTP or HTTPS URL.";
        }
    }

    /// <summary>
    /// Validates filename to prevent path traversal
    /// </summary>
    public class SafeFilenameAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null)
                return true;

            if (value is not string filename)
                return false;

            // Check for path traversal
            if (filename.Contains("..") || filename.Contains("/") || filename.Contains("\\"))
                return false;

            // Check for invalid characters
            var invalidChars = Path.GetInvalidFileNameChars();
            if (filename.Any(c => invalidChars.Contains(c)))
                return false;

            // Check length
            if (filename.Length > 255)
                return false;

            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"The {name} field contains invalid filename characters.";
        }
    }

    /// <summary>
    /// Prevents SQL injection in numeric fields
    /// </summary>
    public class SafeNumericAttribute : RegularExpressionAttribute
    {
        public SafeNumericAttribute() : base(@"^[0-9.-]+$")
        {
            ErrorMessage = "Field must contain only numbers, dots, and hyphens.";
        }
    }

    /// <summary>
    /// Validates JSON format
    /// </summary>
    public class ValidJsonAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null)
                return true;

            if (value is not string jsonString)
                return false;

            try
            {
                System.Text.Json.JsonDocument.Parse(jsonString);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public override string FormatErrorMessage(string name)
        {
            return $"The {name} field must contain valid JSON.";
        }
    }

    /// <summary>
    /// Validates password strength
    /// </summary>
    public class StrongPasswordAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is not string password)
                return false;

            // Minimum 8 characters, maximum 128
            if (password.Length < 8 || password.Length > 128)
                return false;

            // Must contain at least one uppercase, lowercase, digit, and special character
            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

            return hasUpper && hasLower && hasDigit && hasSpecial;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"The {name} field must be 8-128 characters long and contain at least one uppercase letter, lowercase letter, digit, and special character.";
        }
    }
}