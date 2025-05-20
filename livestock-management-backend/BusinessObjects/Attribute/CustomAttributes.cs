using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Attribute
{
    public class GreaterThanZeroAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value is int intValue)
            {
                return intValue > 0;
            }
            if (value is double doubleValue)
            {
                return doubleValue > 0;
            }
            if (value is decimal decimalValue)
            {
                return decimalValue > 0;
            }

            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} phải lớn hơn 0.";
        }
       
    }
    public class FutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is DateTime ngay)
            {
                if (ngay < DateTime.Today)
                {
                    return new ValidationResult(ErrorMessage ?? "Ngày phải từ hôm nay trở đi.");
                }
            }
            return ValidationResult.Success;
        }
    }
}
