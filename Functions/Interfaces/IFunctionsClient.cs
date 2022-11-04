using System.Net.Http;
using System.Threading.Tasks;

namespace Supabase.Functions.Interfaces
{
    public interface IFunctionsClient
    {
        Task<string> Invoke(string url, string? token = null, Client.InvokeFunctionOptions? options = null);
        Task<T?> Invoke<T>(string url, string? token = null, Client.InvokeFunctionOptions? options = null) where T : class;
        Task<HttpContent> RawInvoke(string url, string? token = null, Client.InvokeFunctionOptions? options = null);
    }
}