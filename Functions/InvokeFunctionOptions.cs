using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;

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
            public Dictionary<string, string> Headers { get; set; } =
                new Dictionary<string, string>();

            /// <summary>
            /// Body of the Request
            /// </summary>
            [JsonProperty("body")]
            public Dictionary<string, object> Body { get; set; } = new Dictionary<string, object>();

            /// <summary>
            /// Timout value for HttpClient Requests, defaults to 100s.
            /// https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclient.timeout?view=net-8.0#remarks
            /// </summary>
            public TimeSpan HttpTimeout { get; set; } = TimeSpan.FromSeconds(100);

            /// <summary>
            /// Http method of the Request
            /// </summary>
            public HttpMethod HttpMethod { get; set; } = HttpMethod.Post;

            /// <summary>
            /// Region of the request
            /// </summary>
            public FunctionRegion FunctionRegion { get; set; } = FunctionRegion.Any;
        }

        /// <summary>
        /// Define the region for requests
        /// </summary>
        public class FunctionRegion : IEquatable<FunctionRegion>
        {
            private string _region;

            public static FunctionRegion Any { get; } = new FunctionRegion("any");

            public static FunctionRegion ApNortheast1 { get; } =
                new FunctionRegion("ap-northeast-1");

            public static FunctionRegion ApNortheast2 { get; } =
                new FunctionRegion("ap-northeast-2");

            public static FunctionRegion ApSouth1 { get; } = new FunctionRegion("ap-south-1");

            public static FunctionRegion ApSoutheast1 { get; } =
                new FunctionRegion("ap-southeast-1");

            public static FunctionRegion ApSoutheast2 { get; } =
                new FunctionRegion("ap-southeast-2");

            public static FunctionRegion CaCentral1 { get; } = new FunctionRegion("ca-central-1");

            public static FunctionRegion EuCentral1 { get; } = new FunctionRegion("eu-central-1");

            public static FunctionRegion EuWest1 { get; } = new FunctionRegion("eu-west-1");

            public static FunctionRegion EuWest2 { get; } = new FunctionRegion("eu-west-2");

            public static FunctionRegion EuWest3 { get; } = new FunctionRegion("eu-west-3");

            public static FunctionRegion SaEast1 { get; } = new FunctionRegion("sa-east-1");

            public static FunctionRegion UsEast1 { get; } = new FunctionRegion("us-east-1");

            public static FunctionRegion UsWest1 { get; } = new FunctionRegion("us-west-1");

            public static FunctionRegion UsWest2 { get; } = new FunctionRegion("us-west-2");

            /// <summary>
            /// Define the region for requests
            /// </summary>
            public FunctionRegion(string region)
            {
                _region = region;
            }

            /// <summary>
            /// Check if the object is identical to the reference passed
            /// </summary>
            public override bool Equals(object obj)
            {
                return obj is FunctionRegion r && Equals(r);
            }

            /// <summary>
            /// Generate Hash code
            /// </summary>
            public override int GetHashCode()
            {
                return _region.GetHashCode();
            }

            /// <summary>
            /// Check if the object is identical to the reference passed
            /// </summary>
            public bool Equals(FunctionRegion other)
            {
                return _region == other._region;
            }

            /// <summary>
            /// Overloading the operator ==
            /// </summary>
            public static bool operator ==(FunctionRegion left, FunctionRegion right) =>
                Equals(left, right);

            /// <summary>
            /// Overloading the operator !=
            /// </summary>
            public static bool operator !=(FunctionRegion left, FunctionRegion right) =>
                !Equals(left, right);

            public static explicit operator string(FunctionRegion region) => region.ToString();

            public static explicit operator FunctionRegion(string region) =>
                new FunctionRegion(region);

            public override string ToString() => _region;
        }
    }
}
