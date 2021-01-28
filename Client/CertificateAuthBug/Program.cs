using System;
using System.Collections;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace CertificateAuthBug
{
    internal static class Program
    {
        private const string CertificatePath = "certificate.pfx";
        private const string Password = "qwerty";
        private const string ServerUrl = "https://localhost:8443";

        private static void Main(string[] args)
        {
            // AppContext.SetSwitch("System.Net.Http.UseSocketsHttpHandler", true);

            var certificate = new X509Certificate2(CertificatePath, Password);
            using var handler1 = new HttpClientHandler
            {
                ClientCertificates = { certificate },
                // Disabling domain name check
                ServerCertificateCustomValidationCallback = (a, b, c, d) => true
            };
            var client1 = new HttpClient(handler1)
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
            using var handler2 = new HttpClientHandler
            {
                ClientCertificates = { certificate },
                // Disabling domain name check
                ServerCertificateCustomValidationCallback = (a, b, c, d) => true
            };
            var client2 = new HttpClient(handler2)
            {
                Timeout = TimeSpan.FromSeconds(10)
            };

            for (var i = 0; i < 5; i++)
            {
                var requestNum = i;
                try
                {
                    MakeRequest(() =>
                    {
                        switch (requestNum)
                        {
                            case 0:
                                return client2;
                            case 1:
                                return client2;
                            case 2:
                                if (args.Any())
                                {
                                    ClearSslCache();
                                }
                                return client1;
                            case 3:
                                return client1;
                            case 4:
                                return client2;
                            default:
                                return client2;
                        }
                    });
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e);
                }
                Thread.Sleep(1000);
            }

            Console.Out.Flush();
        }

        // Using synchronous version for better external debug experience
        private static void MakeRequest(Func<HttpClient> clientFactory)
        {
            var cli = clientFactory();
            var response = cli.SendAsync(new HttpRequestMessage(HttpMethod.Get, ServerUrl)
            {
                Version = new Version(1, 1)
            }).Result;
            if (!response.IsSuccessStatusCode)
            {
                Console.Error.WriteLine($"Received error status code: {response.StatusCode}");
                return;
            }

            var responseString = response.Content.ReadAsStringAsync().Result.TrimEnd('\n');
            Console.WriteLine($"Received response: {responseString}");
        }

        private static void ClearSslCache()
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .First(x => x.FullName?.Contains("System.Net.Security") == true);
            var cacheType = assembly.GetTypes().First(x => x.Name == "SslSessionsCache");
            var field = cacheType.GetField("s_cachedCreds", BindingFlags.NonPublic | BindingFlags.Static);
            if (field != null)
            {
                var dic = (IDictionary?) field.GetValue(null);
                dic?.Clear();
            }
        }
    }
}
