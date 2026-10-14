using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WorkConnect.Models
{
    /// <summary>Bắt buộc checkbox phải được tick (kiểm tra cả server và client).</summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class MustBeTrueAttribute : ValidationAttribute, IClientModelValidator
    {
        public override bool IsValid(object? value) => value is bool b && b;

        public void AddValidation(ClientModelValidationContext context)
        {
            Merge(context.Attributes, "data-val", "true");
            Merge(context.Attributes, "data-val-mustbetrue",
                FormatErrorMessage(context.ModelMetadata.GetDisplayName()));
        }

        private static void Merge(IDictionary<string, string> attrs, string key, string value)
        {
            if (!attrs.ContainsKey(key)) attrs.Add(key, value);
        }
    }
}
