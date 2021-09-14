using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace prnt_sc_crawler
{
    internal sealed class PrntScCrawlerHostedService : IHostedService
    {
        private const String ScUrlBase = "https://prnt.sc";
        private HttpClient httpClient;
        private IConfiguration configuration;
        private CancellationTokenSource cancellationTokenSource;
        public PrntScCrawlerHostedService(IConfiguration configuration)
        {
            httpClient = new HttpClient();
            this.configuration = configuration;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            cancellationTokenSource = new CancellationTokenSource();
            for(Int32 i = 0; i < configuration.GetSection("threads").Get<Int32>(); i++)
            {
                CrawlAsync(
                    configuration.GetSection("output").Get<String>(),
                    configuration.GetSection("sleep").Get<Int32>(),
                    cancellationTokenSource.Token
                );
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if(cancellationTokenSource == null)
                throw new System.Exception("Service has never been started!");
            if(cancellationTokenSource.IsCancellationRequested)
                throw new System.Exception("Service has already been stopped");
            cancellationTokenSource.Cancel();
        }
        private async Task CrawlAsync(String OutputFolder, Int32 WaitIntervallMs, CancellationToken cancellationToken)
        {
            StringBuilder ScUrlBuilder = new StringBuilder();
            String ScUrl, ScUrlSuffix, Website;
            Random UrlGenerator = new Random();
            while(!cancellationToken.IsCancellationRequested)
            {
                do
                {
                    ScUrlBuilder.Clear();
                    for(int i = 0; i < 2; i++)
                    {
                        ScUrlBuilder.Append((char)(97 + UrlGenerator.Next(0, 26)));
                    }
                    for(int i = 0; i < 4; i++)
                    {
                        ScUrlBuilder.Append(UrlGenerator.Next(0, 10).ToString());
                    }
                    ScUrlSuffix = ScUrlBuilder.ToString();
                }while(Directory.GetFiles(OutputFolder, String.Format("{0}.*", ScUrlSuffix)).Length > 0);
                
                ScUrl = String.Format("{0}/{1}", ScUrlBase, ScUrlSuffix);
                System.Diagnostics.Debug.WriteLine(String.Format("Calculated Url: {0}", ScUrl));

                try
                {
                    Website = await httpClient.GetStringAsync(ScUrl, cancellationToken); 
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(String.Format("Error Fetching Website: {0}", ex.Message));
                    continue;
                }

                if(Website != null)
                {
                    Website = Website.Substring(Website.IndexOf("id=\"screenshot-image\""));
                    Website = Website.Substring(Website.IndexOf("src=\"") + 5);
                    Website = Website.Substring(0, Website.IndexOf("\""));
                }

                try
                {
                    using(Stream ImageStream = await httpClient.GetStreamAsync(Website))
                    {
                        using(FileStream OutputStream = new FileStream(Path.Combine(OutputFolder, String.Format("{0}.{1}", ScUrlSuffix, Website.Substring(Website.LastIndexOf(".")))), FileMode.OpenOrCreate))
                        {
                            await ImageStream.CopyToAsync(OutputStream);
                        }
                    }
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(String.Format("Error Saving Image: {0}", ex.Message));
                    continue;
                }

                await Task.Delay(WaitIntervallMs, cancellationToken);
            }
        }
    }
}