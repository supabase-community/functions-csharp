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
        Client _client;
        string _token;

        [TestInitialize]
        public void Initialize()
        {
            var endpoint = Environment.GetEnvironmentVariable("FUNCTION_ENDPOINT");

            _token = Environment.GetEnvironmentVariable("TOKEN");
            _client = new Client(endpoint);
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
    }
}
