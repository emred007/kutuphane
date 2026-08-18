namespace LibraryManagement.Services;

public static class PhoneNumberValidator
{
    public static string NormalizeAndValidate(string? phoneNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(phoneNumber);

        var trimmed = phoneNumber.Trim();
        if (trimmed.Any(char.IsLetter))
        {
            throw new InvalidOperationException("Telefon numarası harf içeremez.");
        }

        if (trimmed.Any(c => !char.IsDigit(c) && !char.IsWhiteSpace(c) && c is not '-' and not '(' and not ')'))
        {
            throw new InvalidOperationException("Telefon numarası yalnızca rakam içermelidir.");
        }

        var digits = new string(trimmed.Where(char.IsDigit).ToArray());
        if (digits.Length != 11)
        {
            throw new InvalidOperationException("Telefon numarası tam 11 rakam olmalıdır (05XXXXXXXXX).");
        }

        if (!digits.StartsWith("05", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Telefon numarası 05 ile başlamalıdır.");
        }

        return digits;
    }

    public static bool IsValid(string? phoneNumber)
    {
        try
        {
            _ = NormalizeAndValidate(phoneNumber);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
