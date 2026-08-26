using System.ComponentModel.DataAnnotations;

namespace RaniaAmer_Pharmacy.MVC.Models.Enums;

// How a product is sold/priced in the pharmacy (all are discrete, whole-number units)
public enum SaleUnit
{
    [Display(Name = "قطعة")]
    Piece = 0,

    [Display(Name = "علبة")]
    Box = 1,

    [Display(Name = "شريط")]
    Strip = 2,

    [Display(Name = "قرص")]
    Tablet = 3,

    [Display(Name = "زجاجة")]
    Bottle = 4,

    [Display(Name = "أنبوبة")]
    Tube = 5,

    [Display(Name = "سرنجة")]
    Syringe = 6,

    [Display(Name = "كيس")]
    Sachet = 7
}
