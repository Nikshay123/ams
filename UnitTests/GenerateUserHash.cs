using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TenantManagement.Common;
using WebApp.Common.Exceptions;
using WebApp.Models;

namespace UnitTests.StandardSamples
{
    [TestClass]
    public class GenerateUserHash : TestBase
    {
        protected void OutputCredentials(string email, string pass, string salt, string token)
        {
            Console.WriteLine($"email: {email}\npass: {pass}\nsalt:{salt}\ntoken:{token}");
        }

        [TestMethod]
        public void GetUserHash()
        {
            var username = "G7Farm@yopmail.com";
            var password = "Test@12345678";
            var salt = "e757d2ab-c489-400d-88e8-2f00af7c5158";
            OutputCredentials($"{username}", $"{password}", $"{salt}", CryptoUtils.HashPassword($"{username}", $"{password}", $"{salt}"));
        }
    }
}