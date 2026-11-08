using System;
using BepInEx;
using BepInEx.Configuration;
using Marketplace.Modules.CMS;
using Marketplace.Modules.MainMarketplace;

namespace Dorn.MarketplaceMailBridge
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("MarketplaceAndServerNPCs")]
    public sealed class MarketplaceMailBridge : BaseUnityPlugin
    {
        public const string PluginGuid = "dorn.marketplace-mail-bridge";
        public const string PluginName = "Marketplace Mail Bridge";
        public const string PluginVersion = "1.0.0";

        private ConfigEntry<int> _expiryMinutes;
        private ConfigEntry<int> _maximumStack;
        private ConfigEntry<string> _senderName;

        private void Awake()
        {
            _expiryMinutes = Config.Bind(
                "Mail",
                "ExpiryMinutes",
                43200,
                "Minutes before an automated mail expires. Default: 30 days.");
            _maximumStack = Config.Bind(
                "Safety",
                "MaximumStack",
                100,
                "Maximum quantity accepted by one mpmail command.");
            _senderName = Config.Bind(
                "Mail",
                "SenderName",
                "Glitnir",
                "Name displayed as the sender in Marketplace mail.");

            new Terminal.ConsoleCommand(
                "mpmail",
                "Marketplace mail: mpmail send <steamId> <prefab> <quantity> [quality]",
                (Terminal.ConsoleEvent)OnMailCommand,
                false,
                false,
                true,
                false,
                false,
                false,
                null,
                false,
                false,
                true);

            Logger.LogInfo("Marketplace Mail Bridge loaded. Command: mpmail send <steamId> <prefab> <quantity> [quality]");
        }

        private void OnMailCommand(Terminal.ConsoleEventArgs args)
        {
            if (args.Length < 2 || !string.Equals(args[1], "send", StringComparison.OrdinalIgnoreCase))
            {
                PrintUsage(args.Context);
                return;
            }

            if (args.Length < 5)
            {
                PrintUsage(args.Context);
                return;
            }

            var steamId = args[2];
            var prefab = args[3];
            int quantity;
            var quality = 1;

            if (!IsSteamId(steamId) || string.IsNullOrWhiteSpace(prefab))
            {
                args.Context.AddString("mpmail: SteamID ou prefab inválido.");
                return;
            }

            if (!int.TryParse(args[4], out quantity) || quantity < 1 || quantity > _maximumStack.Value)
            {
                args.Context.AddString("mpmail: quantidade deve estar entre 1 e " + _maximumStack.Value + ".");
                return;
            }

            if (args.Length >= 6 && (!int.TryParse(args[5], out quality) || quality < 1 || quality > 4))
            {
                args.Context.AddString("mpmail: qualidade deve estar entre 1 e 4.");
                return;
            }

            var attachment = new Marketplace_DataTypes.ClientMarketSendData
            {
                ItemPrefab = prefab,
                Count = quantity,
                Price = 0,
                SellerName = _senderName.Value,
                ItemCategory = Marketplace_DataTypes.ItemData_ItemCategory.ALL,
                Quality = quality,
                Variant = 0,
                // Marketplace deserializes this field as Dictionary<string, string>.
                // An empty string crashes its mail UI; an empty JSON object represents no custom data.
                CUSTOMdata = "{}",
                CrafterName = _senderName.Value,
                CrafterID = 0,
                DurabilityPercent = 100,
                Currency = string.Empty
            };

            try
            {
                CentralizedMailSystem_Server.AddMail(
                    _senderName.Value,
                    "Entrega do servidor",
                    "Você recebeu um item do servidor.",
                    steamId,
                    new[] { attachment },
                    new Marketplace_DataTypes.ClientMarketSendData[0],
                    _expiryMinutes.Value);

                args.Context.AddString("mpmail: envio criado para " + steamId + ": " + quantity + "x " + prefab + ".");
                Logger.LogInfo("Mail created: sender=" + _senderName.Value + ", recipient=" + steamId + ", prefab=" + prefab + ", quantity=" + quantity + ".");
            }
            catch (Exception ex)
            {
                args.Context.AddString("mpmail: falha ao criar o correio. Veja o LogOutput.log.");
                Logger.LogError("Could not create marketplace mail: " + ex);
            }
        }

        private static bool IsSteamId(string value)
        {
            ulong parsed;
            return value != null && value.Length == 17 && ulong.TryParse(value, out parsed);
        }

        private static void PrintUsage(Terminal terminal)
        {
            terminal.AddString("Uso: mpmail send <steamId> <prefab> <quantidade> [qualidade]");
            terminal.AddString("Exemplo: mpmail send 76561198000000000 ArmorBloodcallerChest_DO 1 1");
        }
    }
}
