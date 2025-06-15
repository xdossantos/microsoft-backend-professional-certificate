using System;
using Newtonsoft.Json;

namespace MyApp
{

    internal class Program
    {
        public async Task GetDataFromMultipleSourcesAsync()
        {
            // Simulate fetching data from multiple sources asynchronously
            await Task.Delay(1000); // Simulating delay
            Console.WriteLine("Data fetched from multiple sources.");
        }

        public async Task DownloadDataAsync()
        {
            Console.WriteLine("Downloading data...");
            await Task.Delay(5000); // Simulating delay
            Console.WriteLine("Data downloaded successfully.");
        }

        static async Task Main(string[] args)
        {
            Program program = new Program();
            Task.WhenAll(
                program.GetDataFromMultipleSourcesAsync(),
                program.DownloadDataAsync()
            ).Wait(); // Wait for both tasks to complete
            // await program.GetDataFromMultipleSourcesAsync();
            // await program.DownloadDataAsync();
        }
    }
}
