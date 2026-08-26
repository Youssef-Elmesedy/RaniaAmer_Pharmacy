namespace RaniaAmer_Pharmacy.MVC.Common;

public static class AuthSchemes
{
    public const string Admin = "Identity.Application";
    public const string Customer = "CustomerScheme";
    public const string AdminOrCustomer = Admin + "," + Customer;
}