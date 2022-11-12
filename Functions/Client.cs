using Newtonsoft.Json;
using Supabase.Core;
using Supabase.Core.Extensions;
using Supabase.Functions.Interfaces;
using Supabase.Functions.Responses;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;

[assembly: InternalsVisibleTo("FunctionsTests")]
namespace Supabase.Functions
{

    public partial class Client : IFunctionsClient
    {
        private static readonly HttpClient client = new HttpClient();
        private string baseUrl;

        /// <summary>
        /// Function that can be set to return dynamic headers.
        /// 
        /// Headers specified in the method parameters will ALWAYS take precendece over headers returned by this function.
        /// </summary>
        public Func<Dictionary<string, string>>? GetHeaders { get; set; }

        public Client(string baseUrl)
        {
            this.baseUrl = baseUrl;
        }

        /// <summary>
        /// Returns an <see cref="HttpContent"/> response, allowing for coersion into Streams, Strings, and byte[]
        /// </summary>
        /// <param name="functionName">Function Name, will be appended to BaseUrl</param>
        /// <param name="token">Anon Key.</param>
        /// <param name="options">Options</param>
        /// <returns></returns>
        public async Task<HttpContent> RawInvoke(string functionName, string? token = null, InvokeFunctionOptions? options = null)
        {
            var url = $"{baseUrl}/{functionName}";

            return (await HandleRequest(url, token, options)).Content;
        }

        /// <summary>
        /// Invokes a function and returns the Text content of the response.
        /// </summary>
        /// <param name="functionName">Function Name, will be appended to BaseUrl</param>
        /// <param name="token">Anon Key.</param>
        /// <param name="options">Options</param>
        /// <returns></returns>
        public async Task<string> Invoke(string functionName, string? token = null, InvokeFunctionOptions? options = null)
        {
            var url = $"{baseUrl}/{functionName}";
            var response = await HandleRequest(url, token, options);

            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Invokes a function and returns a JSON Deserialized object according to the supplied generic Type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="functionsName">Function Name, will be appended to BaseUrl</param>
        /// <param name="token">Anon Key.</param>
        /// <param name="options">Options</param>
        /// <returns></returns>
        public async Task<T?> Invoke<T>(string functionName, string? token = null, InvokeFunctionOptions? options = null) where T : class
        {
            var url = $"{baseUrl}/{functionName}";
            var response = await HandleRequest(url, token, options);

            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(content);
        }

        /// <summary>
        /// Internal request handling
        /// </summary>
        /// <param name="url"></param>
        /// <param name="token"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <exception cref="RequestException"></exception>
        internal async Task<HttpResponseMessage> HandleRequest(string url, string? token = null, InvokeFunctionOptions? options = null)
        {
            if (options == null)
            {
                options = new InvokeFunctionOptions();
            }

            if (GetHeaders != null)
            {
                options.Headers = GetHeaders().MergeLeft(options.Headers);
            }

            if (!string.IsNullOrEmpty(token))
            {
                options.Headers["Authorization"] = $"Bearer {token}";
            }

            options.Headers["X-Client-Info"] = Util.GetAssemblyVersion(typeof(Client));

            var builder = new UriBuilder(url);
            var query = HttpUtility.ParseQueryString(builder.Query);

            builder.Query = query.ToString();

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, builder.Uri))
            {
                requestMessage.Content = new StringContent(JsonConvert.SerializeObject(options.Body), Encoding.UTF8, "application/json");

                if (options.Headers != null)
                {
                    foreach (var kvp in options.Headers)
                    {
                        requestMessage.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
                    }
                }

                var response = await client.SendAsync(requestMessage);

                if (!response.IsSuccessStatusCode || response.Headers.Contains("x-relay-error"))
                {
                    var content = await response.Content.ReadAsStringAsync();

                    var obj = new ErrorResponse
                    {
                        Content = content,
                        Message = content
                    };
                    throw new RequestException(response, obj);
                }

                return response;
            }
        }

        public class RequestException : Exception
        {
            public HttpResponseMessage Response { get; private set; }
            public ErrorResponse Error { get; private set; }

            public RequestException(HttpResponseMessage response, ErrorResponse error) : base(error.Message)
            {
                Response = response;
                Error = error;
            }
        }
    }
}
