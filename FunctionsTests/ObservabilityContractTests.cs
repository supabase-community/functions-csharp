using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Supabase.Functions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using static Supabase.Functions.Client;

namespace FunctionsTests
{
    /// <summary>
    /// Contract tests for the diagnostics the SDK emits through System.Diagnostics
    /// (ActivitySource/Meter "Supabase.Functions") and for the sanitization rule: telemetry must
    /// never contain a query string, the request body, a token, or other secret.
    /// </summary>
    [TestClass]
    public class ObservabilityContractTests
    {
        private const string FunctionName = "hello";
        private const string SecretBodyValue = "secret-body-value-42";

        private readonly List<Activity> activities = new();
        private readonly List<KeyValuePair<double, Dictionary<string, object?>>> measurements = new();
        private ActivityListener activityListener = null!;
        private MeterListener meterListener = null!;
        private WireMockServer server = null!;
        private Client client = null!;

        [TestInitialize]
        public void TestInitializer()
        {
            server = WireMockServer.Start();
            client = new Client($"{server.Url}/functions/v1");

            activityListener = new ActivityListener
            {
                ShouldListenTo = source => source.Name == FunctionsDiagnostics.SourceName,
                Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
                ActivityStopped = activity => activities.Add(activity)
            };
            ActivitySource.AddActivityListener(activityListener);

            meterListener = new MeterListener
            {
                InstrumentPublished = (instrument, listener) =>
                {
                    if (instrument.Meter.Name == FunctionsDiagnostics.SourceName)
                        listener.EnableMeasurementEvents(instrument);
                }
            };
            meterListener.SetMeasurementEventCallback<double>((_, value, tags, _) =>
            {
                var tagValues = new Dictionary<string, object?>();
                foreach (var tag in tags)
                    tagValues[tag.Key] = tag.Value;
                measurements.Add(new KeyValuePair<double, Dictionary<string, object?>>(value, tagValues));
            });
            meterListener.Start();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            activityListener.Dispose();
            meterListener.Dispose();
            server.Stop();
        }

        [TestMethod(DisplayName = "The invoke span records the request URL without its query string")]
        public async Task InvokeSpanRecordsSanitizedUrl()
        {
            MockInvokeOk();
            await Invoke();
            var span = SingleInvokeSpan();
            Assert.AreEqual($"{server.Url}/functions/v1/{FunctionName}", span.GetTagItem("url.full"),
                "the query string must never be recorded");
        }

        [TestMethod(DisplayName = "The invoke span follows OpenTelemetry conventions and tags the function name")]
        public async Task InvokeSpanRecordsMethodStatusAndFunctionName()
        {
            MockInvokeOk();
            await Invoke();
            var span = SingleInvokeSpan();
            Assert.AreEqual(ActivityKind.Client, span.Kind);
            Assert.AreEqual("POST", span.GetTagItem("http.request.method"));
            Assert.AreEqual(200, span.GetTagItem("http.response.status_code"));
            Assert.AreEqual(FunctionName, span.GetTagItem("faas.invoked_name"));
        }

        [TestMethod(DisplayName = "A failed invocation marks the span as an error")]
        public async Task FailedInvocationMarksTheSpanAsError()
        {
            server.Given(Request.Create().WithPath($"/functions/v1/{FunctionName}").UsingPost())
                .RespondWith(Response.Create().WithStatusCode(500).WithBody("boom"));
            await Assert.ThrowsAsync<Supabase.Functions.Exceptions.FunctionsException>(Invoke);
            var span = SingleInvokeSpan();
            Assert.AreEqual(ActivityStatusCode.Error, span.Status);
            Assert.AreEqual(500, span.GetTagItem("http.response.status_code"));
        }

        [TestMethod(DisplayName = "A relay error marks the span as an error even on a 2xx status")]
        public async Task RelayErrorOnSuccessStatusMarksTheSpanAsError()
        {
            server.Given(Request.Create().WithPath($"/functions/v1/{FunctionName}").UsingPost())
                .RespondWith(Response.Create().WithStatusCode(200).WithHeader("x-relay-error", "true").WithBody("relayed"));
            await Assert.ThrowsAsync<Supabase.Functions.Exceptions.FunctionsException>(Invoke);
            var span = SingleInvokeSpan();
            Assert.AreEqual(ActivityStatusCode.Error, span.Status);
            Assert.AreEqual("x-relay-error", span.GetTagItem("error.type"));
        }

        [TestMethod(DisplayName = "The invoke duration histogram is recorded per request")]
        public async Task InvokeDurationMetricIsRecorded()
        {
            MockInvokeOk();
            await Invoke();
            meterListener.RecordObservableInstruments();
            Assert.AreEqual(1, measurements.Count);
            var measurement = measurements.Single();
            Assert.IsTrue(measurement.Key > 0);
            Assert.AreEqual(200, measurement.Value["http.response.status_code"]);
            Assert.AreEqual($"/functions/v1/{FunctionName}", measurement.Value["url.path"]);
            Assert.AreEqual(FunctionName, measurement.Value["faas.invoked_name"]);
        }

        [TestMethod(DisplayName = "Telemetry never contains the request body")]
        public async Task TelemetryDoesNotLeakTheBody()
        {
            MockInvokeOk();
            await Invoke();
            var recorded = activities
                .SelectMany(a => a.TagObjects)
                .Select(tag => tag.Value?.ToString() ?? "")
                .Concat(measurements.SelectMany(m => m.Value.Values).Select(v => v?.ToString() ?? ""))
                .Concat(activities.Select(a => a.DisplayName));
            Assert.IsFalse(recorded.Any(value => value.Contains(SecretBodyValue)),
                "no span name, tag, or metric dimension may contain the request body");
        }

        private Task<string> Invoke() =>
            client.Invoke(FunctionName, options: new InvokeFunctionOptions
            {
                Body = new Dictionary<string, object> { { "name", SecretBodyValue } }
            });

        private void MockInvokeOk() =>
            server.Given(Request.Create().WithPath($"/functions/v1/{FunctionName}").UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody("{\"message\":\"ok\"}"));

        private Activity SingleInvokeSpan() =>
            activities.Single(a => a.OperationName == $"POST /functions/v1/{FunctionName}");
    }
}
