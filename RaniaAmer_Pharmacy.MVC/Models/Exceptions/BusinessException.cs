namespace RaniaAmer_Pharmacy.MVC.Models.Exceptions;

public class BusinessException : Exception
{
    public BusinessException()
    {
    }

    public BusinessException(string message, string paramName)
        : base(message)
    {
    }

    public BusinessException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}