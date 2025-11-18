using Colossal.Entities;
using Game.Areas;
using Game.Buildings;
using Game.Common;
using Game.Prefabs;
using Game.Tools;
using Game.UI;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using ServiceUpgrade = Game.Buildings.ServiceUpgrade;
using TransportStation = Game.Prefabs.TransportStation;

namespace SubwayFreedomAssetPack.System
{
	public partial class ModuleUpgradeRaycastSystem : ToolBaseSystem
	{
		private Entity m_PreviousRaycastedEntity;
		private Entity m_PreviousSelectedEntity;
		private EntityQuery m_HighlightedQuery;
		private ToolOutputBarrier m_Barrier;
		private PrefabSystem prefabSystem;
		private NameSystem nameSystem;

		public override string toolID => "ModuleUpgradeRaycastPickerTool";
		private Entity m_TargetModuleEntity;

		protected override void OnCreate()
		{
			base.OnCreate();
			Enabled = false;
			m_Barrier = World.GetOrCreateSystemManaged<ToolOutputBarrier>();
			prefabSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<PrefabSystem>();
			m_HighlightedQuery = GetEntityQuery(ComponentType.ReadOnly<Highlighted>());
			nameSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<NameSystem>();
		}

		public override void InitializeRaycast()
		{
			base.InitializeRaycast();
			m_ToolRaycastSystem.collisionMask = CollisionMask.Overground | CollisionMask.OnGround | CollisionMask.Underground;
			m_ToolRaycastSystem.typeMask |= TypeMask.StaticObjects;
			m_ToolRaycastSystem.areaTypeMask |= AreaTypeMask.Lots;
			m_ToolRaycastSystem.raycastFlags |= RaycastFlags.BuildingLots;
			m_ToolRaycastSystem.raycastFlags |= RaycastFlags.SubBuildings;
		}

		protected override void OnStartRunning()
		{
			base.OnStartRunning();
			applyAction.shouldBeEnabled = true;
		}

		protected override void OnStopRunning()
		{
			base.OnStopRunning();
			EntityManager.AddComponent<BatchesUpdated>(m_HighlightedQuery);
			EntityManager.RemoveComponent<Highlighted>(m_HighlightedQuery);
			m_PreviousRaycastedEntity = Entity.Null;
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
            EntityCommandBuffer buffer = m_Barrier.CreateCommandBuffer();
            
            if (!GetRaycastResult(out var currentRaycastEntity, out RaycastHit _))
            {
                buffer.AddComponent<BatchesUpdated>(m_HighlightedQuery, EntityQueryCaptureMode.AtPlayback);
                buffer.RemoveComponent<Highlighted>(m_HighlightedQuery, EntityQueryCaptureMode.AtPlayback);
                m_PreviousRaycastedEntity = Entity.Null;
                return inputDeps;
            }
            
            if (currentRaycastEntity != m_PreviousRaycastedEntity)
            {
                m_PreviousRaycastedEntity = currentRaycastEntity;
                NativeArray<Entity> entities = m_HighlightedQuery.ToEntityArray(Allocator.Temp);
                buffer.AddComponent<BatchesUpdated>(entities);
                buffer.RemoveComponent<Highlighted>(entities);
            }

            IsValidPrefab(currentRaycastEntity);
            
            if (m_HighlightedQuery.IsEmptyIgnoreFilter)
            {
                buffer.AddComponent<BatchesUpdated>(currentRaycastEntity);
                buffer.AddComponent<Highlighted>(currentRaycastEntity);
                m_PreviousRaycastedEntity = currentRaycastEntity;
            }
            
            if (!applyAction.WasReleasedThisFrame() || m_ToolSystem.selected == Entity.Null)
            {
                return inputDeps;
            }
            OnStationSelected(currentRaycastEntity, buffer);
            m_ToolSystem.activeTool = m_DefaultToolSystem;
            return base.OnUpdate(inputDeps);
		}

		private bool HasPedestrianAccess(Entity entity)
		{
			if (EntityManager.TryGetBuffer<SubObject>(entity, false, out var subObjects))
			{
				Mod.log.Info("HAS SUBOBJECTS ");
				foreach (var subObject in subObjects)
				{
					Mod.log.Info("SUB " + subObject.m_Prefab);
					if (nameSystem.GetDebugName(subObject.m_Prefab).Contains("Pedestrian Spawn Location"))
					{
						Mod.log.Info("PEDESTRIAN SPAWN ");
						return true;
					}
				}
			}

			return false;
		}

		private bool IsValidPrefab(Entity entity)
		{
			var owner = GetOwner(entity);
		
			if (EntityManager.HasComponent<TransportStationData>(owner))
			{
				Mod.log.Info("transportStationData ");
			}
			
			if (EntityManager.HasComponent<Building>(owner))
			{
				Mod.log.Info("building ");
			}
			
			return EntityManager.HasComponent<Building>(entity) &&
			       (HasPedestrianAccess(entity) || HasPedestrianAccess(owner));
		}

		private Entity GetOwner(Entity currentEntity)
		{
			return World.DefaultGameObjectInjectionWorld.EntityManager.TryGetComponent<Owner>(currentEntity,
				out var owner) ? GetOwner(owner.m_Owner) : currentEntity;
		}

		public override bool TrySetPrefab(PrefabBase prefab)
		{
			return false;
		}

		public override PrefabBase GetPrefab()
		{
			return null;
		}
		
		public void StartPicking(Entity pylonEntity)
		{
			m_TargetModuleEntity = pylonEntity;
			m_ToolSystem.activeTool = this;
		}
		
		private void OnStationSelected(Entity selectedStation, EntityCommandBuffer buffer)
		{
			if (m_TargetModuleEntity == Entity.Null)
			{
				return;
			}
			EntityManager.AddComponent<ServiceUpgrade>(selectedStation);
			EntityManager.AddComponent<Owner>(selectedStation);
			EntityManager.SetComponentData(
				selectedStation,
				new Owner
				{
					m_Owner = m_TargetModuleEntity,
				}
			);
			
			if (EntityManager.HasBuffer<InstalledUpgrade>(m_TargetModuleEntity))
			{
				var installedUpgrade = World.DefaultGameObjectInjectionWorld.EntityManager.GetBuffer<InstalledUpgrade>(m_TargetModuleEntity);
				installedUpgrade.Add(
					new InstalledUpgrade
					{
						m_Upgrade = selectedStation,
					}
				);
			}
			else
			{
				var installedUpgrade = World.DefaultGameObjectInjectionWorld.EntityManager.AddBuffer<InstalledUpgrade>(m_TargetModuleEntity);
				installedUpgrade.Add(
					new InstalledUpgrade
					{
						m_Upgrade = selectedStation,
					}
				);
			}

			
			
			// if (EntityManager.HasBuffer<ServiceUpgradeBuilding>(selectedStation))
			// {
			// 	var installedUpgrade = World.DefaultGameObjectInjectionWorld.EntityManager.GetBuffer<ServiceUpgradeBuilding>(selectedStation);
			// 	installedUpgrade.Add(
			// 		new ServiceUpgradeBuilding
			// 		{
			// 			m_Building = m_TargetModuleEntity,
			// 		}
			// 	);
			// }
			// else
			// {
			// 	var installedUpgrade = World.DefaultGameObjectInjectionWorld.EntityManager.AddBuffer<ServiceUpgradeBuilding>(selectedStation);
			// 	installedUpgrade.Add(
			// 		new ServiceUpgradeBuilding
			// 		{
			// 			m_Building = m_TargetModuleEntity,
			// 		}
			// 	);
			// }
		}
	}
}