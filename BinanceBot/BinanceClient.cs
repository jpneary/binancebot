using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BinanceBot
{
    public class BinanceClient
    {
        IConfiguration m_Config;

        ILogger m_Log;

        HttpClient m_Client;

        string m_EndPoint;

        /// <summary>
        /// Constructor
        /// </summary>       
        public BinanceClient(IConfiguration cfg, ILogger log)
        {
            m_Config = cfg;
            m_Log = log;

            log.Log(LogLevel.Information,  "Creating Binance Client");

            m_Client = new HttpClient();
            
        }


        /// <summary>
        /// Try and connecto to server
        /// </summary>
        public void Connect()
        {
            // check api key set
            if (string.IsNullOrEmpty(m_Config["apikey"]))
            {
                throw new ArgumentException("API Key not set");
            }

            // check secret set
            if (string.IsNullOrEmpty(m_Config["apisecretkey"]))
            {
                throw new ArgumentException("API Secret not set");
            }

            // check end point.
            if (string.IsNullOrEmpty(m_Config["binanceendpoint"]))
            {
                throw new ArgumentException("Binance End point not set");
            }

            // set the end point.
            m_EndPoint = m_Config["binanceendpoint"];


            // see if online and check for time. No credentials required for this so a good test.
            HttpResponseMessage responsemsg = m_Client.GetAsync(GetAPIUrl("time")).Result;

            if (responsemsg.IsSuccessStatusCode)
            {
                m_Log.LogInformation("Connected to binance");
                m_Log.LogInformation("Time : " + responsemsg.Content.ReadAsStringAsync().Result);
            }
            else
            {
                throw new InvalidOperationException("Could not connect to binance. Status code = " + responsemsg.StatusCode.ToString());
            }

            // just try and get BTC price
            //string result = m_Client.GetStringAsync(string.Format("{0}/api/v3/ticker/price?symbol=USDTBTC", m_Config["binanceendpoint"])).Result;
        }

        /// <summary>
        /// TryGetExchangeInfo
        /// </summary>
        public bool TryGetExchangeInfo(out string filename, bool forcerefresh)
        {
            bool success = false;
            filename = string.Empty;

            try
            {
                filename = filename = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\exchangeinfo.json";

                // if we don't already have the exchange information or we are force refresh
                if (!File.Exists(filename) || forcerefresh)
                {
                    // get exchange information
                    HttpResponseMessage responsemsg = m_Client.GetAsync(GetAPIUrl("exchangeInfo")).Result;

                    if (responsemsg.IsSuccessStatusCode)
                    {
                        m_Log.LogInformation("Successfully read exchange information");

                        byte[] exchangeinfo = responsemsg.Content.ReadAsByteArrayAsync().Result;

                        // write exhange info
                        File.WriteAllBytes(filename, exchangeinfo);
                    }
                    else
                    {
                        throw new InvalidOperationException("Could not connect to binance. Status code = " + responsemsg.StatusCode.ToString());
                    }

                }

            }
            catch (Exception e)
            {
                m_Log.LogError(e.Message);
            }

            return success;
        }


        /// <summary>
        /// Method to construct API url
        /// </summary>
        private string GetAPIUrl(string apimethod)
        {
            return string.Format("{0}/api/v3/{1}", m_EndPoint, apimethod);
        }


    }
}
