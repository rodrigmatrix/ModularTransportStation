using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;
using SubwayFreedomAssetPack.System;

namespace SubwayFreedomAssetPack
{
    public class Mod : IMod
    {
        public static ILog log = LogManager.GetLogger($"{nameof(SubwayFreedomAssetPack)}.{nameof(Mod)}").SetShowsErrorsInUI(false);
        public const string Id = "SubwayFreedomAssetPack";
        
        public void OnLoad(UpdateSystem updateSystem)
        {
            log.Info(nameof(OnLoad));

            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
                log.Info($"Current mod asset at {asset.path}");
            updateSystem.UpdateAt<SelectedBuildingUISystem>(SystemUpdatePhase.UIUpdate);
            updateSystem.UpdateAt<ModuleUpgradeRaycastSystem>(SystemUpdatePhase.ToolUpdate);
        }

        public void OnDispose()
        {
            log.Info(nameof(OnDispose));
        }
    }
}
