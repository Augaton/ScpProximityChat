using CommandSystem;
using AugatonLib.Commands;

namespace ScpProximityChat.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public sealed class ProximityCommand : StaffParentCommand
    {
        public ProximityCommand() => LoadGeneratedCommands();

        public override string Command => "scpproximity";

        public override string[] Aliases => new[] { "prox" };

        public override string Description => "Etat du chat de proximite SCP.";

        public override string Permission => "scpproximitychat.manage";

        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new StatusCommand(
                "ScpProximityChat",
                typeof(Plugin), Permission, builder =>
            {
                Config config = Plugin.Instance.Config;
                builder.AppendLine($"  activation : {config.ActivationType}");
                builder.AppendLine($"  roles autorises : {config.ScpRoles.Count}");
                builder.AppendLine($"  volume {config.Volume:0.#} (max {config.MaxVolume:0.#}), portee {config.MinDistance:0.#} a {config.MaxDistance:0.#}");
                builder.AppendLine($"  chat SCP par defaut : {(config.UseDefaultScpChat ? "conserve" : "remplace")}");
                return true;
            }));
        }
    }
}
