using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Supabase.Functions.Client;

namespace FunctionsTests
{
    [TestClass]
    public class Client
    {
        [TestMethod("Invokes a function.")]
        public async Task Invokes()
        {
            var token = Environment.GetEnvironmentVariable("TOKEN");
            var endpoint = Environment.GetEnvironmentVariable("ENDPOINT");

            var result = await Invoke(endpoint, token, new InvokeFunctionOptions
            {
                Body = new Dictionary<string, object>
                {
                    {"name", "supabase" }
                }
            });

            Assert.IsTrue(result.Contains("supabase"));


            var result2 = await Invoke<Dictionary<string, string>>(endpoint, token, new InvokeFunctionOptions
            {
                Body = new Dictionary<string, object>
                {
                    { "name", "functions" }
                }
            });

            Assert.IsInstanceOfType(result2, typeof(Dictionary<string, string>));
            Assert.IsTrue(result2.ContainsKey("message"));
            Assert.IsTrue(result2["message"].Contains("functions"));


            var result3 = await RawInvoke(endpoint, token, new InvokeFunctionOptions
            {
                Body = new Dictionary<string, object>
                {
                    { "name", "functions" }
                }
            });

            var bytes = await result3.ReadAsByteArrayAsync();

            Assert.IsInstanceOfType(bytes, typeof(byte[]));
        }
    }
}
