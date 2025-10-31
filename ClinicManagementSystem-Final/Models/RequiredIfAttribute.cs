using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;

namespace ClinicManagementSystem_Final.Models
{
    /// <summary>
    /// Conditional required attribute: the target property is required when another property equals the given value.
    /// Implements server-side validation and emits client validation attributes for unobtrusive validation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class RequiredIfAttribute : ValidationAttribute, IClientModelValidator
    {
        private readonly string _otherPropertyName;
        private readonly object _desiredValue;

        public RequiredIfAttribute(string otherPropertyName, object desiredValue)
        {
            _otherPropertyName = otherPropertyName;
            _desiredValue = desiredValue;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            PropertyInfo? otherProperty = validationContext.ObjectType.GetProperty(_otherPropertyName);
            if (otherProperty == null)
            {
                return new ValidationResult($"Unknown property: {_otherPropertyName}");
            }

            object? otherValue = otherProperty.GetValue(validationContext.ObjectInstance);

            if (object.Equals(otherValue, _desiredValue))
            {
                if (value == null || (value is string s && string.IsNullOrWhiteSpace(s)))
                {
                    return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} is required.", new[] { validationContext.MemberName! });
                }
            }

            return ValidationResult.Success;
        }

        // Adds HTML attributes for client-side unobtrusive validation.
        public void AddValidation(ClientModelValidationContext context)
        {
            if (context == null) return;

            MergeAttribute(context.Attributes, "data-val", "true");
            MergeAttribute(context.Attributes, "data-val-requiredif", ErrorMessage ?? $"{context.ModelMetadata.DisplayName} is required.");
            MergeAttribute(context.Attributes, "data-val-requiredif-other", _otherPropertyName);
            MergeAttribute(context.Attributes, "data-val-requiredif-value", _desiredValue?.ToString() ?? string.Empty);
        }

        private bool MergeAttribute(IDictionary<string, string> attributes, string key, string value)
        {
            if (attributes.ContainsKey(key)) return false;
            attributes.Add(key, value);
            return true;
        }
    }
}