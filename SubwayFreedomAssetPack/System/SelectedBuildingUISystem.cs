using Colossal.UI.Binding;
using Game.UI;
using Unity.Entities;

namespace SubwayFreedomAssetPack.System
{
    public partial class SelectedBuildingUISystem : ExtendedInfoSectionBase
    {
        
        private NameSystem _nameSystem;
        private ModuleUpgradeRaycastSystem _buildingPickerToolSystem;
        
        protected override void OnCreate()
        {
            base.OnCreate();
            m_InfoUISystem.AddMiddleSection(this);
            _nameSystem ??= World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<NameSystem>();
            CreateTrigger("OnOpenPicker", OnOpenPicker);
        }

        protected override string group => "SubwayFreedomAssetPack";

        public override void OnWriteProperties(IJsonWriter writer)
        {
        }

        protected override void OnProcess()
        {
        }

        protected override void Reset()
        {
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            visible = false;
            var buildingName = _nameSystem.GetDebugName(selectedEntity);
            if (buildingName.Contains("FreedomSubwayStationBuilding"))
            {
                visible = true;
            }
      
            RequestUpdate();
        }

        private void OnOpenPicker()
        {
            _buildingPickerToolSystem ??= World.GetOrCreateSystemManaged<ModuleUpgradeRaycastSystem>();
            _buildingPickerToolSystem.StartPicking(selectedEntity);
        }
    }
}