using System;
using System.Security.Cryptography;
using System.Text;

namespace QLKAHYTOON.Helpers
{
    /// <summary>
    /// Tool test hash password - Chạy để xem mật khẩu sau khi hash
    /// </summary>
    public class PasswordHelper
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== PASSWORD HASH TESTER ===");
            Console.WriteLine();

            while (true)
            {
                Console.Write("Nhập mật khẩu cần hash (hoặc 'exit' để thoát): ");
                string password = Console.ReadLine();

                if (string.IsNullOrEmpty(password) || password.ToLower() == "exit")
                {
                    break;
                }

                string hashed = HashPassword(password);
                Console.WriteLine($"Mật khẩu gốc: {password}");
                Console.WriteLine($"Mật khẩu đã hash: {hashed}");
                Console.WriteLine($"Độ dài: {hashed.Length} ký tự");
                Console.WriteLine();

                // Test verify
                Console.Write("Nhập mật khẩu để verify: ");
                string testPassword = Console.ReadLine();
                bool isValid = VerifyPassword(testPassword, hashed);
                Console.WriteLine($"Kết quả verify: {(isValid ? "✓ ĐÚNG" : "✗ SAI")}");
                Console.WriteLine();
                Console.WriteLine("-----------------------------------");
                Console.WriteLine();
            }

            Console.WriteLine("Đã thoát!");
        }

        public static string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            string hashOfInput = HashPassword(password);
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;
            return comparer.Compare(hashOfInput, hashedPassword) == 0;
        }
    }
}