using Guardian.Shared.Services;

namespace Guardian.Shared.Tests;

public sealed class PasswordHasherTests
{
    [Fact]
    public void HashPassword_DoesNotStorePlainText()
    {
        var hasher = new PasswordHasher();
        var auth = hasher.HashPassword("secret");

        Assert.NotEqual("secret", auth.PasswordHash);
        Assert.NotEmpty(auth.PasswordSalt);
    }

    [Fact]
    public void Verify_ReturnsTrueForCorrectPassword()
    {
        var hasher = new PasswordHasher();
        var auth = hasher.HashPassword("secret");

        Assert.True(hasher.Verify("secret", auth));
    }

    [Fact]
    public void Verify_ReturnsFalseForWrongPassword()
    {
        var hasher = new PasswordHasher();
        var auth = hasher.HashPassword("secret");

        Assert.False(hasher.Verify("wrong", auth));
    }

    [Fact]
    public void HashPassword_UsesDifferentSaltEachTime()
    {
        var hasher = new PasswordHasher();

        Assert.NotEqual(hasher.HashPassword("secret").PasswordSalt, hasher.HashPassword("secret").PasswordSalt);
    }
}
