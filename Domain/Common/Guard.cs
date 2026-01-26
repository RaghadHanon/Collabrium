namespace Domain.Common;

public static class Guard
{
    public static string NotNullOrWhiteSpace(string? value, string name)
    {
        return string.IsNullOrWhiteSpace(value) ? throw new DomainException($"{name} is required.") : value.Trim();
    }

    public static void Against(bool condition, string message)
    {
        if (condition) throw new DomainException(message);
    }

    public static int NonNegative(int value, string name)
    {
        return value < 0 ? throw new DomainException($"{name} cannot be negative.") : value;
    }

    public static int Positive(int value, string name)
    {
        return value <= 0 ? throw new DomainException($"{name} must be positive.") : value;
    }
}
