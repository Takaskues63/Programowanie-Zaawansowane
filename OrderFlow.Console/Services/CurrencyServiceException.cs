namespace OrderFlow.Console.Services;

public class CurrencyServiceException : Exception
{
    public CurrencyServiceException(string message) : base(message) { }
    public CurrencyServiceException(string message, Exception inner) : base(message, inner) { }
}
