namespace LeagueSharp.Data.Updater
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    using Newtonsoft.Json.Linq;

    internal class Updater
    {
        #region Fields

        private ProgressBar progressBar;

        private WebClient webclient;

        #endregion

        #region Public Methods and Operators

        public async Task Run()
        {
            this.webclient = new WebClient();
            this.webclient.DownloadProgressChanged += this.WebclientDownloadProgressChanged;

            Console.Write("Downloading versions.json ");
            this.progressBar = new ProgressBar();

            var versionData =
                await this.webclient.DownloadStringTaskAsync("https://ddragon.leagueoflegends.com/api/versions.json");

            var versionJson = JArray.Parse(versionData);
            var version = versionJson.First.ToObject<string>();

            this.ResetProgressBar();
            Console.Write("Downloading champion.json ");

            var championData =
                await
                this.webclient.DownloadStringTaskAsync(
                    $"http://ddragon.leagueoflegends.com/cdn/{version}/data/en_US/champion.json");

            var champions = new List<string>();
            var championJson = JObject.Parse(championData)["data"].ToObject<JObject>();

            foreach (var champion in championJson)
            {
                champions.Add(champion.Key);
            }

            if (!Directory.Exists("Resources"))
            {
                Directory.CreateDirectory("Resources");
            }

            if (!Directory.Exists("Resources\\ChampionData"))
            {
                Directory.CreateDirectory("Resources\\ChampionData");
            }

            foreach (var fileName in champions.Select(x => $"{x}.json"))
            {
                this.ResetProgressBar();
                Console.Write("Downloading {0} ", fileName);

                var url = $"http://ddragon.leagueoflegends.com/cdn/{version}/data/en_US/champion/{fileName}";

                await this.webclient.DownloadFileTaskAsync(url, $"Resources\\ChampionData\\{fileName}");
            }

            this.ResetProgressBar();
            Console.Write("Downloading item.json ");

            await
                this.webclient.DownloadFileTaskAsync(
                    $"http://ddragon.leagueoflegends.com/cdn/{version}/data/en_US/item.json",
                    "Resources\\item.json");

            this.ResetProgressBar(false);

            Console.Write("Writing champion.json ");
            File.WriteAllText("Resources\\champion.json", championData);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Done!");
            Console.ResetColor();

            Console.Write("Finished updating LeagueSharp.Data. Press any key to quit. ");
            Console.ReadKey(true);
        }

        #endregion

        #region Methods

        private void ResetProgressBar(bool createProgressBar = true)
        {
            this.progressBar.Dispose();

            if (createProgressBar)
            {
                this.progressBar = new ProgressBar();
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Done!");
            Console.ResetColor();
        }

        private void WebclientDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.progressBar.Report((double)e.BytesReceived / e.TotalBytesToReceive);
        }

        #endregion
    }
}