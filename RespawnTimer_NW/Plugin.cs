namespace RespawnTimer
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Net;
    using API.Features;
    using PluginAPI.Core;
    using PluginAPI.Core.Attributes;
    using PluginAPI.Enums;
    using PluginAPI.Events;

    public class RespawnTimer
    {
        public static RespawnTimer Singleton;
        public static string RespawnTimerDirectoryPath { get; private set; }

        [PluginConfig]
        public Configs.Config Config;


        [PluginAPI.Core.Attributes.PluginPriority(LoadPriority.Medium)]
        [PluginEntryPoint("RespawnTimer", "4.0.4", "RespawnTimer", "Michal78900")]
        private void LoadPlugin()
        {
            if (!Config.IsEnabled)
                return;

            Singleton = this;
            RespawnTimerDirectoryPath = PluginHandler.Get(this).PluginDirectoryPath;
            EventManager.RegisterEvents<EventHandler>(this);

            if (!Directory.Exists(RespawnTimerDirectoryPath))
            {
                // Log.Warn("RespawnTimer directory does not exist. Creating...");
                Log.Info("RespawnTimer directory does not exist. Creating...");
                Directory.CreateDirectory(RespawnTimerDirectoryPath);
            }

            string exampleTimerDirectory = Path.Combine(RespawnTimerDirectoryPath, "ExampleTimer");
            if (!Directory.Exists(exampleTimerDirectory))
                DownloadExampleTimer(exampleTimerDirectory);

            RueiHelper.Refresh();

        }

        private void DownloadExampleTimer(string exampleTimerDirectory)
        {
            string exampleTimerZip = exampleTimerDirectory + ".zip";
            string exampleTimerTemp = exampleTimerDirectory + "_Temp";

            using WebClient client = new();

            // Log.Warn("Downloading ExampleTimer.zip...");
            Log.Info("Downloading ExampleTimer.zip...");
            string url = $"https://github.com/Michal78900/RespawnTimer/releases/download/v{PluginHandler.Get(this).PluginVersion}/ExampleTimer.zip";
            try
            {
                client.DownloadFile(url, exampleTimerZip);
            }
            catch (WebException e)
            {
                if (e.Response is HttpWebResponse response)
                    Log.Error($"Error while downloading ExampleTimer.zip: {(int)response.StatusCode} {response.StatusCode}");
                
                return;
            }

            Log.Info("ExampleTimer.zip has been downloaded!");

            // Log.Warn("Extracting...");
            Log.Info("Extracting...");
            ZipFile.ExtractToDirectory(exampleTimerZip, exampleTimerTemp);
            Directory.Move(Path.Combine(exampleTimerTemp, "ExampleTimer"), exampleTimerDirectory);

            Directory.Delete(exampleTimerTemp);
            File.Delete(exampleTimerZip);

            Log.Info("Done!");
        }

    }
}