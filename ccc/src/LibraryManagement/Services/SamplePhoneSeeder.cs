namespace LibraryManagement.Services;

public static class SamplePhoneSeeder
{
    private static readonly Dictionary<string, string> PhonesByEmail = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ayse@example.com"] = "05321110001",
        ["mehmet@example.com"] = "05332220002",
        ["zeynep@example.com"] = "05343330003",
        ["ali@example.com"] = "05354440004",
        ["fatma@example.com"] = "05365550005",
        ["emre@gmail.com"] = "05376660006"
    };

    public static bool ApplyMissingPhones(IList<Models.User> users)
    {
        var updated = false;
        var usedPhones = new HashSet<string>(
            users.Where(u => PhoneNumberValidator.IsValid(u.PhoneNumber)).Select(u => u.PhoneNumber));

        foreach (var user in users)
        {
            if (PhoneNumberValidator.IsValid(user.PhoneNumber))
            {
                continue;
            }

            if (PhonesByEmail.TryGetValue(user.Email, out var phone) && !usedPhones.Contains(phone))
            {
                user.PhoneNumber = phone;
                usedPhones.Add(phone);
                updated = true;
                continue;
            }

            var generated = GenerateUniquePhone(usedPhones);
            user.PhoneNumber = generated;
            usedPhones.Add(generated);
            updated = true;
        }

        return updated;
    }

    private static string GenerateUniquePhone(HashSet<string> usedPhones)
    {
        for (var suffix = 21_110_001; suffix <= 21_999_999; suffix++)
        {
            var phone = $"053{suffix:D8}";
            if (phone.Length == 11 && usedPhones.Add(phone))
            {
                return phone;
            }
        }

        throw new InvalidOperationException("Benzersiz örnek telefon numarası üretilemedi.");
    }
}
