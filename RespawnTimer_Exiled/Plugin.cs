namespace RespawnTimer
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Net;
    using API.Features;
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.API.Interfaces;
    using Exiled.Loader;


    public class RespawnTimer : Plugin<Configs.Config>

    {
        public static RespawnTimer Singleton;
        public static string RespawnTimerDirectoryPath { get; private set; }


        public EventHandler EventHandler;



        public override void OnEnabled()

        {

            Singleton = this;

            RespawnTimerDirectoryPath = Path.Combine(Paths.Configs, "RespawnTimer");
            EventHandler = new EventHandler();


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


            Exiled.Events.Handlers.Map.Generated += EventHandler.OnGenerated;
            Exiled.Events.Handlers.Server.RoundStarted += EventHandler.OnRoundStart;
            Exiled.Events.Handlers.Player.Dying += EventHandler.OnDying;
            Exiled.Events.Handlers.Server.ReloadedConfigs += OnReloaded;

            foreach (IPlugin<IConfig> plugin in Loader.Plugins)
            {
                switch (plugin.Name)
                {
                    case "Serpents Hand" when plugin.Config.IsEnabled:
                        API.API.SerpentsHandTeam.Init(plugin.Assembly);
                        Log.Debug("Serpents Hand plugin detected!");
                        break;

                    case "UIURescueSquad" when plugin.Config.IsEnabled:
                        API.API.UiuTeam.Init(plugin.Assembly);
                        Log.Debug("UIURescueSquad plugin detected!");
                        break;
                }
            }

            if (!Config.ReloadTimerEachRound)
                OnReloaded();

            base.OnEnabled();

        }

        private void DownloadExampleTimer(string exampleTimerDirectory)
        {
            string exampleTimerZip = exampleTimerDirectory + ".zip";
            string exampleTimerTemp = exampleTimerDirectory + "_Temp";

            using WebClient client = new();

            // Log.Warn("Downloading ExampleTimer.zip...");
            Log.Info("Downloading ExampleTimer.zip...");

            string url = $"https://github.com/Michal78900/RespawnTimer/releases/download/v{Version}/ExampleTimer.zip";
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


        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Map.Generated -= EventHandler.OnGenerated;
            Exiled.Events.Handlers.Server.RoundStarted -= EventHandler.OnRoundStart;
            Exiled.Events.Handlers.Player.Dying -= EventHandler.OnDying;
            Exiled.Events.Handlers.Server.ReloadedConfigs -= OnReloaded;

            EventHandler = null;
            Singleton = null;

            base.OnDisabled();
        }

        public override void OnReloaded()
        {
            if (Config.Timers.IsEmpty())
            {
                Log.Error("Timer list is empty!");
                return;
            }

            TimerView.CachedTimers.Clear();

            foreach (string name in Config.Timers.Values)
                TimerView.AddTimer(name);
        }

        public override string Name => "RespawnTimer";
        public override string Author => "Michal78900";
        public override Version Version => new(4, 0, 4);
        public override Version RequiredExiledVersion => new(8, 9, 6);
        public override PluginPriority Priority => PluginPriority.Last;

    }
}