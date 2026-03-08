using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Web; // Used for XSS protection
using BCrypt.Net; // Used for secure password hashing

public class SafeVaultService
{
    private readonly string _connectionString;

    public SafeVaultService(string connectionString)
    {
        _connectionString = connectionString;
    }

    // Activity 1 & 3: Input Validation and SQL Injection Prevention
    public bool RegisterUser(string username, string email, string rawPassword, string role = "user")
    {
        // XSS Prevention: Sanitize inputs before they ever hit the database
        string sanitizedUsername = HttpUtility.HtmlEncode(username);
        string sanitizedEmail = HttpUtility.HtmlEncode(email);

        // Activity 2: Secure Password Hashing
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword);

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            // SQLi Prevention: Parameterized queries instead of unsafe string concatenation
            string query = "INSERT INTO Users (Username, Email, PasswordHash, Role) VALUES (@Username, @Email, @PasswordHash, @Role)";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Username", sanitizedUsername);
                cmd.Parameters.AddWithValue("@Email", sanitizedEmail);
                cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                cmd.Parameters.AddWithValue("@Role", role);

                conn.Open();
                int result = cmd.ExecuteNonQuery();
                return result > 0;
            }
        }
    }

    // Activity 2: Authentication
    public bool AuthenticateUser(string username, string rawPassword)
    {
        string sanitizedUsername = HttpUtility.HtmlEncode(username);
        string storedHash = string.Empty;

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            string query = "SELECT PasswordHash FROM Users WHERE Username = @Username";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Username", sanitizedUsername);
                conn.Open();
                object? result = cmd.ExecuteScalar();
                if (result != null)
                {
                    storedHash = result.ToString();
                }
            }
        }

        if (string.IsNullOrEmpty(storedHash)) return false;

        // Verify the provided password against the stored bcrypt hash
        return BCrypt.Net.BCrypt.Verify(rawPassword, storedHash);
    }

    // Activity 2: Role-Based Access Control (RBAC)
    public bool AuthorizeAdminAccess(string username)
    {
        string sanitizedUsername = HttpUtility.HtmlEncode(username);
        string role = string.Empty;

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            string query = "SELECT Role FROM Users WHERE Username = @Username";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Username", sanitizedUsername);
                conn.Open();
                object? result = cmd.ExecuteScalar();
                if (result != null)
                {
                    role = result.ToString();
                }
            }
        }

        return role == "admin";
    }
}