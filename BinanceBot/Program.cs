using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System;
using System.Threading.Tasks;

namespace BinanceBot
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create config access
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddCommandLine(args)
                .Build();

            // create log
            ILogger log = LoggerFactory.Create(builder => 
            {
                builder.AddConsole(); 
            }).CreateLogger("Binance Bot");

            try
            {
                // create binance client
                BinanceClient bc = new BinanceClient(config, log);

                // try and connect.
                bc.Connect();
                
                // try and get the exchange information. Use args to force refresh.
                if (bc.TryGetExchangeInfo(out string filename, config["refresh"] == "y"))
                {                    

                }

            }
            catch (Exception e)
            {
                log.LogError(e.Message);
            }

            Console.ReadKey();
        }
    }
}
