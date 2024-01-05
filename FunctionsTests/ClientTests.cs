using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Supabase.Functions;
using static Supabase.Functions.Client;

namespace FunctionsTests
{
    [TestClass]
    public class ClientTests
    {
        private Client _client = null!;
        private string _token = null!;

        [TestInitialize]
        public void Initialize()
        {
            _token = GenerateToken("37c304f8-51aa-419a-a1af-06154e63707a");
            _client = new Client("http://localhost:9000");
        }

        [TestMethod("Invokes a function.")]
        public async Task Invokes()
        {
            const string function = "hello";

            var result = await _client.Invoke(function, _token, new InvokeFunctionOptions
            {
                Body = new Dictionary<string, object>
                {
                    {"name", "supabase" }
                }
            });

            Assert.IsTrue(result.Contains("supabase"));


            var result2 = await _client.Invoke<Dictionary<string, string>>(function, _token, new InvokeFunctionOptions
            {
                Body = new Dictionary<string, object>
                {
                    { "name", "functions" }
                }
            });

            Assert.IsInstanceOfType(result2, typeof(Dictionary<string, string>));
            Assert.IsTrue(result2.ContainsKey("message"));
            Assert.IsTrue(result2["message"].Contains("functions"));


            var result3 = await _client.RawInvoke(function, _token, new InvokeFunctionOptions
            {
                Body = new Dictionary<string, object>
                {
                    { "name", "functions" }
                }
            });

            var bytes = await result3.ReadAsByteArrayAsync();

            Assert.IsInstanceOfType(bytes, typeof(byte[]));
        }

        private static string GenerateToken(string secret)
        {
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(securityToken);
        }
    }
}
