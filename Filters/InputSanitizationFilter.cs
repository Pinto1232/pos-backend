using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PosBackend.Services;
using System.Collections;
using System.Reflection;

namespace PosBackend.Filters
{
    /// <summary>
    /// Action filter that automatically sanitizes model properties marked with sanitization attributes
    /// </summary>
    public class InputSanitizationFilter : IActionFilter
    {
        private readonly IInputSanitizationService _sanitizationService;
        private readonly ILogger<InputSanitizationFilter> _logger;

        public InputSanitizationFilter(IInputSanitizationService sanitizationService, ILogger<InputSanitizationFilter> logger)
        {
            _sanitizationService = sanitizationService;
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Sanitize action parameters
            foreach (var parameter in context.ActionArguments.ToList())
            {
                if (parameter.Value != null)
                {
                    var sanitizedValue = SanitizeObject(parameter.Value);
                    context.ActionArguments[parameter.Key] = sanitizedValue;
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Post-action cleanup if needed
        }

        private object? SanitizeObject(object? obj)
        {
            if (obj == null)
                return null;

            var type = obj.GetType();

            // Handle primitive types and strings
            if (type.IsPrimitive || type == typeof(string))
            {
                if (obj is string str)
                    return _sanitizationService.SanitizeString(str);
                return obj;
            }

            // Handle collections
            if (obj is IEnumerable enumerable && !(obj is string))
            {
                var sanitizedList = new List<object?>();
                foreach (var item in enumerable)
                {
                    sanitizedList.Add(SanitizeObject(item));
                }
                return sanitizedList;
            }

            // Handle complex objects
            return SanitizeComplexObject(obj);
        }

        private object SanitizeComplexObject(object obj)
        {
            var type = obj.GetType();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (!property.CanRead || !property.CanWrite)
                    continue;

                try
                {
                    var value = property.GetValue(obj);
                    if (value == null)
                        continue;

                    if (property.PropertyType == typeof(string))
                    {
                        var stringValue = (string)value;
                        var sanitizedValue = DetermineSanitizationMethod(property, stringValue);
                        if (sanitizedValue != stringValue)
                        {
                            property.SetValue(obj, sanitizedValue);
                            _logger.LogDebug("Sanitized property {PropertyName} in {TypeName}", 
                                property.Name, type.Name);
                        }
                    }
                    else if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                    {
                        var sanitizedValue = SanitizeObject(value);
                        property.SetValue(obj, sanitizedValue);
                    }
                    else if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType) && 
                             property.PropertyType != typeof(string))
                    {
                        var sanitizedValue = SanitizeObject(value);
                        property.SetValue(obj, sanitizedValue);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to sanitize property {PropertyName} in {TypeName}", 
                        property.Name, type.Name);
                }
            }

            return obj;
        }

        private string DetermineSanitizationMethod(PropertyInfo property, string value)
        {
            // Check for custom sanitization attributes
            var attributes = property.GetCustomAttributes(true);

            foreach (var attr in attributes)
            {
                switch (attr)
                {
                    case PosBackend.Attributes.SanitizeStringAttribute sanitizeAttr:
                        return ApplySanitizationByType(value, sanitizeAttr);
                    case PosBackend.Attributes.SafeEmailAttribute:
                        return _sanitizationService.SanitizeEmail(value);
                    case PosBackend.Attributes.SafeUsernameAttribute:
                        return _sanitizationService.SanitizeAlphanumeric(value);
                    case PosBackend.Attributes.SafeUrlAttribute:
                        return _sanitizationService.SanitizeUrl(value);
                    case PosBackend.Attributes.SafeFilenameAttribute:
                        return _sanitizationService.SanitizeFilename(value);
                    case PosBackend.Attributes.SafeNumericAttribute:
                        return _sanitizationService.SanitizeNumericString(value);
                }
            }

            // Default sanitization based on property name patterns
            var propertyName = property.Name.ToLowerInvariant();
            
            if (propertyName.Contains("email"))
                return _sanitizationService.SanitizeEmail(value);
            else if (propertyName.Contains("url") || propertyName.Contains("link"))
                return _sanitizationService.SanitizeUrl(value);
            else if (propertyName.Contains("filename") || propertyName.Contains("file"))
                return _sanitizationService.SanitizeFilename(value);
            else if (propertyName.Contains("html") || propertyName.Contains("content"))
                return _sanitizationService.SanitizeHtml(value);
            else
                return _sanitizationService.SanitizeString(value);
        }

        private string ApplySanitizationByType(string value, PosBackend.Attributes.SanitizeStringAttribute attr)
        {
            // Extract InputType from the attribute (would need to expose it as a property)
            // For now, apply general sanitization
            return _sanitizationService.SanitizeString(value);
        }
    }

    /// <summary>
    /// Attribute to mark controllers or actions for automatic input sanitization
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AutoSanitizeInputAttribute : Attribute
    {
        public bool Enabled { get; set; } = true;
    }
}