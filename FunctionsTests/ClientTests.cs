using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Supabase.Functions;
using static Supabase.Functions.Client;

namespace FunctionsTests
{
    [TestClass]
    public class ClientTests
    {
        Client client;
        string token;

        [TestInitialize]
        public void Initialize()
        {
            var endpoint = Environment.GetEnvironmentVariable("FUNCTION_ENDPOINT");

            token = Environment.GetEnvironmentVariable("TOKEN");
            client = new Client(endpoint);
        }

        [TestMethod("Invokes a function.")]
        public async Task Invokes()
        {
            var function = "hello";

            var result = await client.Invoke(function, token, new InvokeFunctionOptions
            {
                Body = new Dictionary<string, object>
                {
                    {"name", "supabase" }
                }
            });

            Assert.IsTrue(result.Contains("supabase"));


            var result2 = await client.Invoke<Dictionary<string, string>>(function, token, new InvokeFunctionOptions
            {
                Body = new Dictionary<string, object>
                {
                    { "name", "functions" }
                }
            });

            Assert.IsInstanceOfType(result2, typeof(Dictionary<string, string>));
            Assert.IsTrue(result2.ContainsKey("message"));
            Assert.IsTrue(result2["message"].Contains("functions"));


            var result3 = await client.RawInvoke(function, token, new InvokeFunctionOptions
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
