using System.ComponentModel.DataAnnotations;
using RaniaAmer_Pharmacy.MVC.Models.Enums;

namespace RaniaAmer_Pharmacy.MVC.Common;

public static class SaleUnitExtensions
{
    /// <summary>
    /// Returns the Arabic display label for a sale unit (from its [Display] attribute),
    /// e.g. SaleUnit.Strip -> "شريط". Falls back to the enum name if no attribute is found.
    /// </summary>
    public static string GetLabel(this SaleUnit saleUnit)
    {
        var member = typeof(SaleUnit).GetMember(saleUnit.ToString())[0];
        var display = (DisplayAttribute?)Attribute.GetCustomAttribute(member, typeof(DisplayAttribute));
        return display?.Name ?? saleUnit.ToString();
    }
}
