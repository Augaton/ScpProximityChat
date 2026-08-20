using System;
using System.Collections.Generic;
using AdminToys;
using Exiled.API.Enums;
using MessageType = ScpProximityChat.Enums.MessageType;
using Exiled.API.Features;
using Exiled.API.Features.Roles;
using Exiled.Events.EventArgs.Player;
using Mirror;
using PlayerRoles.FirstPersonControl;
using ScpProximityChat.Enums;
using UnityEngine;
using UserSettings.ServerSpecific;
using VoiceChat;
using VoiceChat.Networking;
using Object = UnityEngine.Object;

namespace ScpProximityChat.Handlers
{
    public sealed class ProximityChatHandlers
    {
        private readonly Config config;
        private readonly Dictionary<string, SpeakerToy> toggledPlayers = new Dictionary<string, SpeakerToy>();
        private readonly Dictionary<string, float> lastToggle = new Dictionary<string, float>();
        private readonly float effectiveVolume;

        public ProximityChatHandlers(Config config)
        {
            this.config = config;
            effectiveVolume = Mathf.Clamp(config.Volume, 0f, config.MaxVolume);
        }

        public void RegisterEvents()
        {
            Exiled.Events.Handlers.Server.RestartingRound += OnRoundRestarting;
            Exiled.Events.Handlers.Player.VoiceChatting += OnVoiceChatting;
            Exiled.Events.Handlers.Player.ChangingRole += OnChangingRole;
            Exiled.Events.Handlers.Player.Left += OnLeft;

            switch (config.ActivationType)
            {
                case ActivationType.ServerSpecificSettings:
                    ServerSpecificSettingsSync.ServerOnSettingValueReceived += OnSettingValueReceived;
                    Exiled.Events.Handlers.Player.Verified += OnVerified;
                    break;

                case ActivationType.NoClip:
                    Exiled.Events.Handlers.Player.TogglingNoClip += OnTogglingNoClip;
                    break;
            }
        }

        public void UnregisterEvents()
        {
            Exiled.Events.Handlers.Server.RestartingRound -= OnRoundRestarting;
            Exiled.Events.Handlers.Player.VoiceChatting -= OnVoiceChatting;
            Exiled.Events.Handlers.Player.ChangingRole -= OnChangingRole;
            Exiled.Events.Handlers.Player.Left -= OnLeft;

            switch (config.ActivationType)
            {
                case ActivationType.ServerSpecificSettings:
                    ServerSpecificSettingsSync.ServerOnSettingValueReceived -= OnSettingValueReceived;
                    Exiled.Events.Handlers.Player.Verified -= OnVerified;
                    break;

                case ActivationType.NoClip:
                    Exiled.Events.Handlers.Player.TogglingNoClip -= OnTogglingNoClip;
                    break;
            }

            ClearAll();
        }

        public void ClearAll()
        {
            foreach (KeyValuePair<string, SpeakerToy> entry in toggledPlayers)
                DestroySpeaker(entry.Value);

            toggledPlayers.Clear();
            lastToggle.Clear();
            OpusHandler.ClearAll();
            ScpProximityChat.API.HintBridge.Clear();
        }

        private void OnRoundRestarting() => ClearAll();

        private void OnVoiceChatting(VoiceChattingEventArgs ev)
        {
            try
            {
                if (ev?.Player is null || ev.VoiceMessage.Channel != VoiceChatChannel.ScpChat)
                    return;

                if (!toggledPlayers.TryGetValue(ev.Player.UserId, out SpeakerToy speaker) || speaker is null)
                    return;

                OpusHandler opusHandler = OpusHandler.Get(ev.Player);

                if (opusHandler is null)
                    return;

                float[] samples = opusHandler.SampleBuffer;
                opusHandler.Decoder.Decode(ev.VoiceMessage.Data, ev.VoiceMessage.DataLength, samples);

                for (int i = 0; i < samples.Length; i++)
                    samples[i] = Mathf.Clamp(samples[i] * effectiveVolume, -1f, 1f);

                byte[] encoded = opusHandler.EncodedBuffer;
                int dataLength = opusHandler.Encoder.Encode(samples, encoded);

                AudioMessage audioMessage = new AudioMessage(speaker.ControllerId, encoded, dataLength);

                foreach (Player target in Player.List)
                {
                    if (target is null || target.ReferenceHub is null)
                        continue;

                    if (config.UseDefaultScpChat && target.IsScp)
                        continue;

                    if (target.Role is not IVoiceRole voiceRole)
                        continue;

                    if (voiceRole.VoiceModule.ValidateReceive(ev.Player.ReferenceHub, VoiceChatChannel.Proximity) == VoiceChatChannel.None)
                        continue;

                    target.ReferenceHub.connectionToClient.Send(audioMessage);
                }

                ev.IsAllowed = config.UseDefaultScpChat;
            }
            catch (Exception e)
            {
                Log.Error($"OnVoiceChatting: {e}");
            }
        }

        private void OnChangingRole(ChangingRoleEventArgs ev)
        {
            try
            {
                if (ev?.Player is null)
                    return;

                if (config.DisableOnRoleChange && toggledPlayers.ContainsKey(ev.Player.UserId))
                    Disable(ev.Player, false);

                if (!config.ScpRoles.Contains(ev.NewRole))
                    return;

                Show(ev.Player, config.ProximityChatRole);
            }
            catch (Exception e)
            {
                Log.Error($"OnChangingRole: {e}");
            }
        }

        private void OnLeft(LeftEventArgs ev)
        {
            try
            {
                if (ev?.Player is null || string.IsNullOrEmpty(ev.Player.UserId))
                    return;

                string userId = ev.Player.UserId;

                if (toggledPlayers.TryGetValue(userId, out SpeakerToy speaker))
                {
                    DestroySpeaker(speaker);
                    toggledPlayers.Remove(userId);
                }

                lastToggle.Remove(userId);
                OpusHandler.Remove(userId);
                ScpProximityChat.API.HintBridge.Remove(ev.Player);
            }
            catch (Exception e)
            {
                Log.Error($"OnLeft: {e}");
            }
        }

        private void OnVerified(VerifiedEventArgs ev)
        {
            try
            {
                if (ev?.Player?.ReferenceHub is null)
                    return;

                ServerSpecificSettingsSync.SendToPlayer(ev.Player.ReferenceHub);
            }
            catch (Exception e)
            {
                Log.Error($"OnVerified: {e}");
            }
        }

        private void OnSettingValueReceived(ReferenceHub hub, ServerSpecificSettingBase settingBase)
        {
            try
            {
                if (settingBase is not SSKeybindSetting keybind
                    || keybind.SettingId != config.KeybindId
                    || !keybind.SyncIsPressed)
                {
                    return;
                }

                if (!Player.TryGet(hub, out Player player) || !config.ScpRoles.Contains(player.Role.Type))
                    return;

                Toggle(player);
            }
            catch (Exception e)
            {
                Log.Error($"OnSettingValueReceived: {e}");
            }
        }

        private void OnTogglingNoClip(TogglingNoClipEventArgs ev)
        {
            try
            {
                if (ev?.Player is null)
                    return;

                if (FpcNoclip.IsPermitted(ev.Player.ReferenceHub) || !config.ScpRoles.Contains(ev.Player.Role.Type))
                    return;

                ev.IsAllowed = false;
                Toggle(ev.Player);
            }
            catch (Exception e)
            {
                Log.Error($"OnTogglingNoClip: {e}");
            }
        }

        private void Toggle(Player player)
        {
            if (player is null || string.IsNullOrEmpty(player.UserId))
                return;

            if (IsOnCooldown(player.UserId))
                return;

            lastToggle[player.UserId] = Time.realtimeSinceStartup;

            if (toggledPlayers.ContainsKey(player.UserId))
                Disable(player, true);
            else
                Enable(player);
        }

        private bool IsOnCooldown(string userId)
        {
            if (config.ToggleCooldownSeconds <= 0f)
                return false;

            return lastToggle.TryGetValue(userId, out float last)
                && Time.realtimeSinceStartup - last < config.ToggleCooldownSeconds;
        }

        private void Enable(Player player)
        {
            SpeakerToy prefab = PrefabHelper.GetPrefab<SpeakerToy>(PrefabType.SpeakerToy);

            if (prefab is null)
            {
                Log.Error("Prefab SpeakerToy introuvable, activation impossible.");
                return;
            }

            SpeakerToy speaker = Object.Instantiate(prefab, player.Transform, true);
            speaker.transform.position = player.Position;

            NetworkServer.Spawn(speaker.gameObject);

            speaker.NetworkControllerId = (byte)player.Id;
            speaker.NetworkMinDistance = config.MinDistance;
            speaker.NetworkMaxDistance = config.MaxDistance;

            toggledPlayers[player.UserId] = speaker;

            Show(player, config.ProximityChatEnabled);
        }

        private void Disable(Player player, bool notify)
        {
            if (!toggledPlayers.TryGetValue(player.UserId, out SpeakerToy speaker))
                return;

            DestroySpeaker(speaker);
            toggledPlayers.Remove(player.UserId);
            OpusHandler.Remove(player);

            if (notify)
                Show(player, config.ProximityChatDisabled);
        }

        private static void DestroySpeaker(SpeakerToy speaker)
        {
            if (speaker is null || speaker.gameObject is null)
                return;

            NetworkServer.Destroy(speaker.gameObject);
        }

        private static void Show(Player player, Message message)
        {
            if (player is null || message is null || !message.Show || string.IsNullOrEmpty(message.Content))
                return;

            switch (message.Type)
            {
                case MessageType.Broadcast:
                    player.Broadcast(message.Duration, message.Content);
                    break;

                case MessageType.Hint:
                    ScpProximityChat.API.HintBridge.Show(player, message.Content, message.Duration);
                    break;
            }
        }
    }
}
