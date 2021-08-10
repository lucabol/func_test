using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace func_test
{
    struct Result
    {
        public long ElapsedMilliseconds;
        public string ErrorMessage;
    }
    class Program
    {
        static Random rnd = new Random();

        static async Task<Result> GetUrlAsync(HttpClient client, string url)
        {
            Result result;
            try
            {
                // Adds a random parameter to the url to avoid
                // caches along the way. Not sure if it is needed, but can't hurt.
                var rndParam = url[url.Length - 1] != '=' ? "?blach=" + rnd.NextDouble().ToString() :
                                      "&blach=" + rnd.NextDouble().ToString();

                var watch = System.Diagnostics.Stopwatch.StartNew();
                var s = await client.GetStringAsync(url + rndParam);
                watch.Stop();
                result = new Result { ElapsedMilliseconds = watch.ElapsedMilliseconds, ErrorMessage = null };
            }
            catch (Exception e)
            {
                result = new Result { ElapsedMilliseconds = 0, ErrorMessage = e.Message };
            }
            Console.WriteLine($"{url.Substring(0, 30)} \t {(result.ErrorMessage == null ? result.ElapsedMilliseconds : result.ErrorMessage)}");
            return result;
        }
        static async Task Main(string[] args)
        {
            int nreqs = 100;

            var urls = new string[] {
              "https://wrustfunc.azurewebsites.net/api/mandelbrot",
              "https://csharpfunc1.azurewebsites.net/api/mandelbrot?code=DY8IcX3OQiEHLaOdUVChnsnY5Rri6ai5VbyuE5RE2BusOZZHF2teog=="
            };

            HttpClient client = new();
            client.Timeout = new TimeSpan(0, 10, 0);

            // Warmup: as it turns out, this is not needed most times as the VM stays up
            // between one run and the next.
            foreach (var url in urls) await GetUrlAsync(client, url);
            Thread.Sleep(6000);

            var results = urls.Select(url => new List<Result>()).ToArray();
            for (var i = 0; i < nreqs; i++)
            {
                // When I increased the parallelism I started seeing errors in the
                // network calls, so I resigned myself to run the tests more slowly
                // but not getting errors.
                var res = await Task.WhenAll(urls.Select(url => GetUrlAsync(client, url)));
                for (var j = 0; j < urls.Length; j++)
                {
                    //var r = await GetUrlAsync(client, urls[j]);
                    var r = res[j];
                    results[j].Add(r);
                }
            }
            // Nothing is done with the results, because I found it simpler to just
            // copy and paste the outoput of the program to an Excel file for statistical
            // analysis instead of doing it here as I orginally planned. I left the code in
            // in case I change my mind.
        }
    }
}
