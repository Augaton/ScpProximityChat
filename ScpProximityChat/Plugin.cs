using System;
using System.Collections.Generic;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Core.UserSettings;
using PlayerRoles;
using ScpProximityChat.Enums;
using ScpProximityChat.Handlers;

namespace ScpProximityChat
{
    public sealed class Plugin : Plugin<Config>
    {
        public override string Name => "ScpProximityChat";

        public override string Author => "Zone-Shilari (base: Bolton)";

        public override string Prefix => "scpproximitychat";

        public override Version Version => new Version(2, 0, 0);

        public override Version RequiredExiledVersion => new Version(9, 14, 2);

        private ProximityChatHandlers proximityChatHandlers;
        private List<SettingBase> registeredSettings;

        public override void OnEnabled()
        {
            ValidateConfig();

            ScpProximityChat.API.HintBridge.YCoordinate = Config.HintYCoordinate;
            ScpProximityChat.API.HintBridge.FontSize = Config.HintFontSize;

            proximityChatHandlers = new ProximityChatHandlers(Config);
            proximityChatHandlers.RegisterEvents();

            if (Config.ActivationType == ActivationType.ServerSpecificSettings)
                RegisterSettings();

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            proximityChatHandlers?.UnregisterEvents();
            ScpProximityChat.API.HintBridge.Clear();

            UnregisterSettings();

            proximityChatHandlers = null;

            base.OnDisabled();
        }

        private void ValidateConfig()
        {
            Config.ScpRoles.RemoveWhere(role => !role.IsScp() || role == RoleTypeId.Scp079);

            if (Config.ScpRoles.Count == 0)
                Log.Warn("Aucun role SCP valide dans ScpRoles, le chat de proximite ne sera jamais activable.");

            if (Config.MaxVolume <= 0f)
            {
                Log.Warn($"MaxVolume ({Config.MaxVolume}) invalide, remis a 25.");
                Config.MaxVolume = 25f;
            }

            if (Config.Volume <= 0f)
            {
                Log.Warn($"Volume ({Config.Volume}) invalide, remis a 1.");
                Config.Volume = 1f;
            }

            if (Config.Volume > Config.MaxVolume)
                Log.Warn($"Volume ({Config.Volume}) depasse MaxVolume ({Config.MaxVolume}), il sera borne.");

            if (Config.MinDistance < 0f)
            {
                Log.Warn($"MinDistance ({Config.MinDistance}) negatif, remis a 0.");
                Config.MinDistance = 0f;
            }

            if (Config.MaxDistance <= Config.MinDistance)
            {
                Log.Warn($"MaxDistance ({Config.MaxDistance}) doit etre superieur a MinDistance ({Config.MinDistance}), remis a MinDistance + 10.");
                Config.MaxDistance = Config.MinDistance + 10f;
            }

            if (Config.ToggleCooldownSeconds < 0f)
            {
                Log.Warn($"ToggleCooldownSeconds ({Config.ToggleCooldownSeconds}) negatif, remis a 0.");
                Config.ToggleCooldownSeconds = 0f;
            }
        }

        private void RegisterSettings()
        {
            registeredSettings = new List<SettingBase>
            {
                new HeaderSetting(Config.KeybindId - 1, Config.SettingHeaderLabel, string.Empty, false),
                new KeybindSetting(Config.KeybindId, Config.KeybindLabel, default, hintDescription: Config.KeybindHint),
            };

            SettingBase.Register(registeredSettings);
            SettingBase.SendToAll();
        }

        private void UnregisterSettings()
        {
            if (registeredSettings is null)
                return;

            SettingBase.Unregister((System.Func<Player, bool>)null, registeredSettings);
            SettingBase.SendToAll();

            registeredSettings = null;
        }
    }
}
