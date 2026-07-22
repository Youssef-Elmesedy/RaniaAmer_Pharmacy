using System.ComponentModel.DataAnnotations;

namespace Awlad_Zamzam.MVC.Models.Enums;

// How a product is sold/priced: per single item, or per kilogram (weight-based, e.g. fresh meat)
public enum SaleUnit
{
    [Display(Name = "بالقطعة")]
    Piece = 0,

    [Display(Name = "بالكيلو")]
    Kilogram = 1
}
