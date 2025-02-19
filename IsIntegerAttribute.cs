using System;
using System.ComponentModel.DataAnnotations;

public class IsIntegerAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return new ValidationResult("Value is required.");
        }

        if (int.TryParse(value.ToString(), out int result))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult("Please enter a valid integer.");
    }
}
