namespace RespawnTimer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MEC;
    using API.Features;
    using Exiled.API.Features;
    using Exiled.Events.EventArgs.Player;

    public class EventHandler
    {
        private CoroutineHandle _timerCoroutine;
        private CoroutineHandle _hintsCoroutine;

        internal void OnGenerated()
        {
            if (RespawnTimer.Singleton.Config.ReloadTimerEachRound)
                RespawnTimer.Singleton.OnReloaded();


            if (_timerCoroutine.IsRunning)
                Timing.KillCoroutines(_timerCoroutine);

            if (_hintsCoroutine.IsRunning)
                Timing.KillCoroutines(_hintsCoroutine);
        }

        internal void OnRoundStart()
        {
            try
            {
                _timerCoroutine = Timing.RunCoroutine(TimerCoroutine());
                _hintsCoroutine = Timing.RunCoroutine(HintsCoroutine());
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }


            Log.Debug("RespawnTimer coroutine started successfully!");

        }


        internal void OnDying(DyingEventArgs ev)

        {
            if (RespawnTimer.Singleton.Config.TimerDelay < 0)
                return;

            if (PlayerDeathDictionary.ContainsKey(ev.Player))
            {
                Timing.KillCoroutines(PlayerDeathDictionary[ev.Player]);
                PlayerDeathDictionary.Remove(ev.Player);
            }

            PlayerDeathDictionary.Add(ev.Player, Timing.CallDelayed(RespawnTimer.Singleton.Config.TimerDelay, () => PlayerDeathDictionary.Remove(ev.Player)));
        }

        private IEnumerator<float> TimerCoroutine()
        {
            yield return Timing.WaitForSeconds(1f);

            while (true)
            {
                yield return Timing.WaitForSeconds(1f);
                int specNum = Player.List.Count(x => !x.IsAlive || x.SessionVariables.ContainsKey("IsGhost"));
                foreach (Player player in Player.List)
                {
                    try
                    {
                        if (player.IsAlive && !player.SessionVariables.ContainsKey("IsGhost"))
                            continue;

                        if (player.IsOverwatchEnabled && RespawnTimer.Singleton.Config.HideTimerForOverwatch)
                            continue;

                        if (API.API.TimerHidden.Contains(player.UserId))
                            continue;

                        if (PlayerDeathDictionary.ContainsKey(player))
                            continue;

                        if (!TimerView.TryGetTimerForPlayer(player, out TimerView timerView))
                            continue;

                        string text = timerView.GetText(specNum);

                        if (RueiHelper.IsActive)
                        {
                            RueiHelper.Show(player.ReferenceHub, text, TimeSpan.FromSeconds(1.25f));
                        }
                        else
                        {
                            player.ShowHint(text, 1.25f);
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e.ToString());
                    }
                }

                if (RoundSummary.singleton._roundEnded)
                    break;
            }
        }

        private IEnumerator<float> HintsCoroutine()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(1f);

                foreach (TimerView timerView in TimerView.CachedTimers.Values)
                    timerView.IncrementHintInterval();

                if (RoundSummary.singleton._roundEnded)
                    break;
            }
        }


        private readonly Dictionary<Player, CoroutineHandle> PlayerDeathDictionary = new(25);
    }
}