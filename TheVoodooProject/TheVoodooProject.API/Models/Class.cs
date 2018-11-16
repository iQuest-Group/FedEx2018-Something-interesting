using Microsoft.Rest;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TheVoodooProject.API.Models
{
    public class ApiKeyServiceClientCredentials : ServiceClientCredentials
    {
        private const string SubscriptionKey = "5968da83bbb54726b00119c219b4f2ff";

        public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add("Ocp-Apim-Subscription-Key", SubscriptionKey);
            return base.ProcessHttpRequestAsync(request, cancellationToken);
        }
    }
}
