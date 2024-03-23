using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace FlexMatrix.Api.Attributes.Validation
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SafeForSqlAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var stringValue = value as string;

            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return new ValidationResult("Value cannot be empty.");
            }

            // Przykładowa prosta walidacja, sprawdzająca czy ciąg nie zawiera znaków specjalnych
            // Można rozszerzyć o dodatkowe warunki bezpieczeństwa
            if (!Regex.IsMatch(stringValue, @"^[a-zA-Z0-9_]+$"))
            {
                return new ValidationResult("Value contains invalid characters.");
            }

            return ValidationResult.Success;
        }
    }
}
