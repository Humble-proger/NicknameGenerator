using NicknameGenerator;
using NicknameGenerator.Options;

// In a real app: load from environment / Key Vault, never hardcode.
const string _salt = "0123456789abcdef0123456789abcdef";

HmacNicknameGenerator _generator = new(new NicknameOptions { SecretSalt = _salt });

Guid _userId = Guid.Parse("550e8400-e29b-41d4-a716-446655440000");

Console.WriteLine($"Default:        {_generator.Generate(_userId)}");
Console.WriteLine($"attempt = 1:    {_generator.Generate(_userId, 1)}");
Console.WriteLine($"attempt = 2:    {_generator.Generate(_userId, 2)}");
Console.WriteLine($"Parts:          {_generator.GenerateParts(_userId, 0).Adjective} | " +
				  $"{_generator.GenerateParts(_userId, 0).Noun} | " +
				  $"{_generator.GenerateParts(_userId, 0).Suffix}");
