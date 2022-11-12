using Newtonsoft.Json;
using System.Collections.Generic;

namespace Supabase.Functions
{

    public partial class Client
    {
        /// <summary>
        /// Options that can be supplied to a function invocation.
        /// 
        /// Note: If Headers.Authorization is set, it can be later overriden if a token is supplied in the method call.
        /// </summary>
        public class InvokeFunctionOptions
        {
            /// <summary>
            /// Headers to be included on the request.
            /// </summary>
            public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();


            /// <summary>
            /// Body of the Request
            /// </summary>
            [JsonProperty("body")]
            public Dictionary<string, object> Body { get; set; } = new Dictionary<string, object>();
        }
    }
}
