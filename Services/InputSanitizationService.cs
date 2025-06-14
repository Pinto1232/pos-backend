using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace PosBackend.Services
{
    /// <summary>
    /// Service for sanitizing user inputs to prevent security vulnerabilities
    /// </summary>
    public interface IInputSanitizationService
    {
        string SanitizeHtml(string input);
        string SanitizeString(string input);
        string SanitizeEmail(string email);
        string SanitizeFilename(string filename);
        string SanitizeUrl(string url);
        string SanitizeNumericString(string input);
        string SanitizeAlphanumeric(string input);
        Dictionary<string, object> SanitizeObject(Dictionary<string, object> obj);
        bool IsValidInput(string input, InputType type);
        string RemoveDangerousCharacters(string input);
    }

    public enum InputType
    {
        Email,
        Username,
        Password,
        Html,
        PlainText,
        Url,
        Filename,
        Numeric,
        Alphanumeric,
        Json
    }

    public class InputSanitizationService : IInputSanitizationService
    {
        private readonly ILogger<InputSanitizationService> _logger;
        
        // Common dangerous patterns
        private static readonly string[] DangerousPatterns = new[]
        {
            @"<script[\s\S]*?</script>",
            @"javascript:",
            @"vbscript:",
            @"onload\s*=",
            @"onerror\s*=",
            @"onclick\s*=",
            @"onmouseover\s*=",
            @"onfocus\s*=",
            @"onblur\s*=",
            @"onchange\s*=",
            @"onsubmit\s*=",
            @"<iframe[\s\S]*?</iframe>",
            @"<object[\s\S]*?</object>",
            @"<embed[\s\S]*?</embed>",
            @"<link[\s\S]*?>",
            @"<meta[\s\S]*?>",
            @"<style[\s\S]*?</style>",
            @"expression\s*\(",
            @"@import",
            @"eval\s*\(",
            @"setTimeout\s*\(",
            @"setInterval\s*\(",
            @"Function\s*\(",
            @"document\.",
            @"window\.",
            @"alert\s*\(",
            @"confirm\s*\(",
            @"prompt\s*\("
        };

        // SQL injection patterns
        private static readonly string[] SqlInjectionPatterns = new[]
        {
            @"(\b(ALTER|CREATE|DELETE|DROP|EXEC(UTE)?|INSERT|MERGE|SELECT|UPDATE|UNION)\b)",
            @"(\b(sp_|xp_)\w+)",
            @"(\b(WAITFOR|DELAY)\s+)",
            @"(--|/\*|\*/|;)",
            @"(\bOR\s+\d+\s*=\s*\d+)",
            @"(\bAND\s+\d+\s*=\s*\d+)",
            @"(\b1\s*=\s*1\b)",
            @"(\'\s*OR\s*\'\d+\'\s*=\s*\'\d+)",
            @"(\'\s*AND\s*\'\d+\'\s*=\s*\'\d+)",
            @"(\'\s*;\s*--)",
            @"(\'\s*;\s*DROP\s+TABLE)",
            @"(\'\s*;\s*INSERT\s+INTO)",
            @"(\'\s*;\s*UPDATE\s+)",
            @"(\'\s*;\s*DELETE\s+FROM)"
        };

        private static readonly Regex EmailRegex = new(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.Compiled);
        private static readonly Regex UrlRegex = new(@"^https?://[a-zA-Z0-9.-]+(/.*)?$", RegexOptions.Compiled);
        private static readonly Regex AlphanumericRegex = new(@"^[a-zA-Z0-9_-]+$", RegexOptions.Compiled);
        private static readonly Regex NumericRegex = new(@"^[0-9.-]+$", RegexOptions.Compiled);

        public InputSanitizationService(ILogger<InputSanitizationService> logger)
        {
            _logger = logger;
        }

        public string SanitizeHtml(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(input);

                // Remove script tags and their content
                var scriptNodes = doc.DocumentNode.SelectNodes("//script");
                if (scriptNodes != null)
                {
                    foreach (var node in scriptNodes)
                        node.Remove();
                }

                // Remove dangerous attributes
                var allNodes = doc.DocumentNode.SelectNodes("//*[@*]");
                if (allNodes != null)
                {
                    foreach (var node in allNodes)
                    {
                        var attributesToRemove = new List<string>();
                        foreach (var attr in node.Attributes)
                        {
                            if (attr.Name.StartsWith("on", StringComparison.OrdinalIgnoreCase) ||
                                attr.Value.Contains("javascript:", StringComparison.OrdinalIgnoreCase) ||
                                attr.Value.Contains("vbscript:", StringComparison.OrdinalIgnoreCase))
                            {
                                attributesToRemove.Add(attr.Name);
                            }
                        }

                        foreach (var attrName in attributesToRemove)
                            node.Attributes.Remove(attrName);
                    }
                }

                return doc.DocumentNode.OuterHtml;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to sanitize HTML, falling back to plain text sanitization");
                return SanitizeString(input);
            }
        }

        public string SanitizeString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Remove dangerous patterns
            var sanitized = RemoveDangerousCharacters(input);
            
            // Remove potential SQL injection patterns
            foreach (var pattern in SqlInjectionPatterns)
            {
                sanitized = Regex.Replace(sanitized, pattern, "", RegexOptions.IgnoreCase);
            }

            // Trim and normalize whitespace
            sanitized = Regex.Replace(sanitized, @"\s+", " ").Trim();

            // Limit length to prevent memory exhaustion attacks
            if (sanitized.Length > 10000)
            {
                sanitized = sanitized.Substring(0, 10000);
                _logger.LogWarning("Input truncated due to excessive length");
            }

            return sanitized;
        }

        public string SanitizeEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return string.Empty;

            // Basic sanitization
            var sanitized = email.Trim().ToLowerInvariant();
            
            // Remove dangerous characters
            sanitized = RemoveDangerousCharacters(sanitized);
            
            // Validate email format
            if (!EmailRegex.IsMatch(sanitized))
            {
                _logger.LogWarning("Invalid email format detected: {Email}", email);
                return string.Empty;
            }

            return sanitized;
        }

        public string SanitizeFilename(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return string.Empty;

            // Remove path traversal characters
            var sanitized = filename.Replace("..", "").Replace("/", "").Replace("\\", "");
            
            // Remove dangerous characters
            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (var c in invalidChars)
            {
                sanitized = sanitized.Replace(c.ToString(), "");
            }

            // Remove additional dangerous characters
            sanitized = Regex.Replace(sanitized, @"[<>:""|?*]", "");
            
            // Limit length
            if (sanitized.Length > 255)
                sanitized = sanitized.Substring(0, 255);

            return sanitized;
        }

        public string SanitizeUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            var sanitized = url.Trim();
            
            // Only allow HTTP and HTTPS protocols
            if (!sanitized.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !sanitized.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            // Remove dangerous patterns
            sanitized = RemoveDangerousCharacters(sanitized);

            // Validate URL format
            if (!UrlRegex.IsMatch(sanitized))
            {
                _logger.LogWarning("Invalid URL format detected: {Url}", url);
                return string.Empty;
            }

            return sanitized;
        }

        public string SanitizeNumericString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var sanitized = Regex.Replace(input, @"[^0-9.-]", "");
            
            // Validate numeric format
            if (!NumericRegex.IsMatch(sanitized))
            {
                _logger.LogWarning("Invalid numeric format detected: {Input}", input);
                return "0";
            }

            return sanitized;
        }

        public string SanitizeAlphanumeric(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var sanitized = Regex.Replace(input, @"[^a-zA-Z0-9_-]", "");
            
            if (sanitized.Length > 100)
                sanitized = sanitized.Substring(0, 100);

            return sanitized;
        }

        public Dictionary<string, object> SanitizeObject(Dictionary<string, object> obj)
        {
            if (obj == null)
                return new Dictionary<string, object>();

            var sanitized = new Dictionary<string, object>();

            foreach (var kvp in obj)
            {
                var key = SanitizeString(kvp.Key);
                var value = kvp.Value;

                if (value is string strValue)
                {
                    sanitized[key] = SanitizeString(strValue);
                }
                else if (value is Dictionary<string, object> dictValue)
                {
                    sanitized[key] = SanitizeObject(dictValue);
                }
                else
                {
                    sanitized[key] = value;
                }
            }

            return sanitized;
        }

        public bool IsValidInput(string input, InputType type)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            return type switch
            {
                InputType.Email => EmailRegex.IsMatch(input),
                InputType.Username => AlphanumericRegex.IsMatch(input) && input.Length >= 3 && input.Length <= 50,
                InputType.Password => input.Length >= 8 && input.Length <= 128,
                InputType.Url => UrlRegex.IsMatch(input),
                InputType.Numeric => NumericRegex.IsMatch(input),
                InputType.Alphanumeric => AlphanumericRegex.IsMatch(input),
                InputType.PlainText => !ContainsDangerousPatterns(input),
                InputType.Html => true, // HTML will be sanitized rather than rejected
                InputType.Json => IsValidJson(input),
                InputType.Filename => !input.Contains("..") && !input.Contains("/") && !input.Contains("\\"),
                _ => false
            };
        }

        public string RemoveDangerousCharacters(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var sanitized = input;

            // Remove dangerous patterns
            foreach (var pattern in DangerousPatterns)
            {
                sanitized = Regex.Replace(sanitized, pattern, "", RegexOptions.IgnoreCase);
            }

            // Remove control characters
            sanitized = Regex.Replace(sanitized, @"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]", "");

            return sanitized;
        }

        private bool ContainsDangerousPatterns(string input)
        {
            return DangerousPatterns.Any(pattern => 
                Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase));
        }

        private bool IsValidJson(string input)
        {
            try
            {
                System.Text.Json.JsonDocument.Parse(input);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}