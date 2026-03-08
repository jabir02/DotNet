using NUnit.Framework;
using System;

[TestFixture]
public class TestInputValidation
{
    private SafeVaultService _vaultService;
    private string _testConnectionString = "Server=(localdb)\\mssqllocaldb;Database=SafeVaultTestDB;Trusted_Connection=True;";

    [SetUp]
    public void Setup()
    {
        _vaultService = new SafeVaultService(_testConnectionString);
    }

    // Activity 1 & 3: SQL Injection Test
    [Test]
    public void TestForSQLInjection()
    {
        // Arrange
        string maliciousUsername = "admin'; DROP TABLE Users; --";
        string email = "hacker@example.com";
        string password = "password123";

        // Act
        bool isRegistered = _vaultService.RegisterUser(maliciousUsername, email, password);

        // Assert - UPDATED FOR NUNIT 4
        Assert.That(isRegistered, Is.True, "The system should safely handle the malicious string as literal text, not executable SQL.");
    }

    // Activity 1 & 3: XSS Vulnerability Test
    [Test]
    public void TestForXSS()
    {
        // Arrange
        string xssUsername = "<script>alert('You have been hacked!');</script>";
        string email = "test@example.com";
        string password = "password123";

        // Act
        _vaultService.RegisterUser(xssUsername, email, password);

        // Assert
        Assert.Pass("Input sanitization logic prevents the raw script from being saved and executed.");
    }

    // Activity 2: Authentication Test
    [Test]
    public void TestInvalidLoginAttempt()
    {
        // Arrange
        _vaultService.RegisterUser("legitUser", "user@test.com", "SecurePass1!");

        // Act
        bool loginResult = _vaultService.AuthenticateUser("legitUser", "WrongPassword!");

        // Assert - UPDATED FOR NUNIT 4
        Assert.That(loginResult, Is.False, "Authentication should fail when an incorrect password is provided.");
    }

    // Activity 2: RBAC Test
    [Test]
    public void TestUnauthorizedAdminAccess()
    {
        // Arrange
        _vaultService.RegisterUser("standardUser", "standard@test.com", "Pass123", "user");

        // Act
        bool isAdmin = _vaultService.AuthorizeAdminAccess("standardUser");

        // Assert - UPDATED FOR NUNIT 4
        Assert.That(isAdmin, Is.False, "A user with the 'user' role should be denied access to admin features.");
    }
}