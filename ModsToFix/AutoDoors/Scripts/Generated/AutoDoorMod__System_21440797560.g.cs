#pragma warning disable 0219
#line 1 "D:/ModSdkVersions/Version1.0/CoreKeeperModSDK/Temp/GeneratedCode/AutoDoor//AutoDoorMod__System_21440797560.g.cs"
using System.Collections;
using System.Collections.Generic;
using PugMod;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
[global::System.Runtime.CompilerServices.CompilerGenerated]
public partial class DoorSwitchSystem
{
    [global::Unity.Entities.DOTSCompilerPatchedMethod("OnUpdate_T0")]

    void __OnUpdate_450AADF4()
    {
        #line 54 "D:/ModSdkVersions/Version1.0/CoreKeeperModSDK/Assets/AutoDoor/AutoDoorMod.cs"
        if (Manager.main.player == null) 
#line 54 "D:/ModSdkVersions/Version1.0/CoreKeeperModSDK/Assets/AutoDoor/AutoDoorMod.cs"
return;
        #line 56 "D:/ModSdkVersions/Version1.0/CoreKeeperModSDK/Assets/AutoDoor/AutoDoorMod.cs"

        var distanceToTriggerLocal = 0.95;
        #line 58 "D:/ModSdkVersions/Version1.0/CoreKeeperModSDK/Assets/AutoDoor/AutoDoorMod.cs"

        var playerPosition = Manager.main.player.WorldPosition;
        #line 60 "D:/ModSdkVersions/Version1.0/CoreKeeperModSDK/Assets/AutoDoor/AutoDoorMod.cs"

        var switchingQueues = __query_283678828_1.GetSingletonRW<GhostPredictionSwitchingQueues>().ValueRW;
        #line 62 "D:/ModSdkVersions/Version1.0/CoreKeeperModSDK/Assets/AutoDoor/AutoDoorMod.cs"

        DoorSwitchSystem_7967A976_LambdaJob_0_Execute(distanceToTriggerLocal, playerPosition, switchingQueues);
        #line 98 "D:/ModSdkVersions/Version1.0/CoreKeeperModSDK/Assets/AutoDoor/AutoDoorMod.cs"

        base.OnUpdate();
#line hidden
    }

    #line 41 "D:/ModSdkVersions/Version1.0/CoreKeeperModSDK/Temp/GeneratedCode/AutoDoor//AutoDoorMod__System_21440797560.g.cs"
    struct DoorSwitchSystem_7967A976_LambdaJob_0_Job : global::Unity.Entities.IJobChunk
    {
        public double distanceToTriggerLocal;
        public global::UnityEngine.Vector3 playerPosition;
        public global::Unity.NetCode.GhostPredictionSwitchingQueues switchingQueues;
        [global::Unity.Collections.ReadOnly] public global::Unity.Entities.EntityTypeHandle __entityTypeHandle;
        public global::Unity.Entities.ComponentTypeHandle<global::ObjectDataCD> __objectDataTypeHandle;
        [global::Unity.Collections.ReadOnly] public global::Unity.Entities.ComponentTypeHandle<global::Unity.Transforms.LocalTransform> __translationTypeHandle;
        [global::Unity.Collections.ReadOnly] public global::Unity.Entities.ComponentTypeHandle<global::Unity.NetCode.GhostInstance> __ghostInstanceTypeHandle;
        [global::Unity.Collections.ReadOnly] public global::Unity.Entities.ComponentLookup<global::Unity.NetCode.PredictedGhost> __Unity_NetCode_PredictedGhost_ComponentLookup;
        public static readonly global::Unity.Profiling.ProfilerMarker s_ProfilerMarker = new global::Unity.Profiling.ProfilerMarker("DoorSwitchSystem_7967A976_LambdaJob_0.Schedule");
        
        void OriginalLambdaBody(global::Unity.Entities.Entity entity, ref global::ObjectDataCD objectData, in global::Unity.Transforms.LocalTransform translation, in global::Unity.NetCode.GhostInstance ghostInstance)
        {
#line 68 "D:\ModSdkVersions\Version1.0\CoreKeeperModSDK\Assets/AutoDoor/AutoDoorMod.cs"
if (ghostInstance.ghostType < 0) 
#line 68 "D:\ModSdkVersions\Version1.0\CoreKeeperModSDK\Assets/AutoDoor/AutoDoorMod.cs"
return;
#line 69 "D:\ModSdkVersions\Version1.0\CoreKeeperModSDK\Assets/AutoDoor/AutoDoorMod.cs"
var distance = math.distancesq(translation.Position, playerPosition);
#line 70 "D:\ModSdkVersions\Version1.0\CoreKeeperModSDK\Assets/AutoDoor/AutoDoorMod.cs"
var playerNearby = distance <= distanceToTriggerLocal;
#line 72 "D:\ModSdkVersions\Version1.0\CoreKeeperModSDK\Assets/AutoDoor/AutoDoorMod.cs"
var isPredicted = __Unity_NetCode_PredictedGhost_ComponentLookup.HasComponent(entity);
#line 74 "D:\ModSdkVersions\Version1.0\CoreKeeperModSDK\Assets/AutoDoor/AutoDoorMod.cs"
if (playerNearby && !isPredicted)
                {
#line 76 "D:\ModSdkVersions\Version1.0\CoreKeeperModSDK\Assets/AutoDoor/AutoDoorMod.cs"
switchingQueues.ConvertToPredictedQueue.Enqueue(new ConvertPredictionEntry
                    {
                        TargetEntity = entity,
                        TransitionDurationSeconds = 1f,
                    });
#line 81 "D:\ModSdkVersions\Version1.0\CoreKeeperModSDK\Assets/AutoDoor/AutoDoorMod.cs"
return;
                }
#line 84 "D:\ModSdkVersions\Version1.0\CoreKeeperModSDK\Assets/AutoDoor/AutoDoorMod.cs"
if (!playerNearby && isPredicted)
                {
#line 86 "D:\ModSdkVersions\Version1.0\CoreKeeperModSDK\Assets/AutoDoor/AutoDoorMod.cs"
switchingQueues.ConvertToInterpolatedQueue.Enqueue(new ConvertPredictionEntry
                    {
                        TargetEntity = entity,
                        TransitionDurationSeconds = 1f,
                    });
#line 91 "D:\ModSdkVersions\Version1.0\CoreKeeperModSDK\Assets/AutoDoor/AutoDoorMod.cs"
return;
                }
            }
        #line 90 "D:/ModSdkVersions/Version1.0/CoreKeeperModSDK/Temp/GeneratedCode/AutoDoor//AutoDoorMod__System_21440797560.g.cs"
        [global::System.Runtime.CompilerServices.CompilerGenerated]
        public void Execute(in global::Unity.Entities.ArchetypeChunk chunk, int batchIndex, bool useEnabledMask, in global::Unity.Burst.Intrinsics.v128 chunkEnabledMask)
        {
            #line 94 "D:/ModSdkVersions/Version1.0/CoreKeeperModSDK/Temp/GeneratedCode/AutoDoor//AutoDoorMod__System_21440797560.g.cs"
            var __entityArrayPtr = global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetChunkEntityArrayIntPtr(chunk, __entityTypeHandle);
            var objectDataArrayPtr = global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetChunkNativeArrayIntPtr<global::ObjectDataCD>(chunk, ref __objectDataTypeHandle);
            var translationArrayPtr = global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetChunkNativeArrayReadOnlyIntPtr<global::Unity.Transforms.LocalTransform>(chunk, ref __translationTypeHandle);
            var ghostInstanceArrayPtr = global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetChunkNativeArrayReadOnlyIntPtr<global::Unity.NetCode.GhostInstance>(chunk, ref __ghostInstanceTypeHandle);
            int chunkEntityCount = chunk.Count;
            if (!useEnabledMask)
            {
                for(var entityIndex = 0; entityIndex < chunkEntityCount; ++entityIndex)
                {
                    OriginalLambdaBody(global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetCopyOfNativeArrayPtrElement<global::Unity.Entities.Entity>(__entityArrayPtr, entityIndex), ref global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::ObjectDataCD>(objectDataArrayPtr, entityIndex), in global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::Unity.Transforms.LocalTransform>(translationArrayPtr, entityIndex), in global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::Unity.NetCode.GhostInstance>(ghostInstanceArrayPtr, entityIndex));
                }
            }
            else
            {
                int edgeCount = global::Unity.Mathematics.math.countbits(chunkEnabledMask.ULong0 ^ (chunkEnabledMask.ULong0 << 1)) + global::Unity.Mathematics.math.countbits(chunkEnabledMask.ULong1 ^ (chunkEnabledMask.ULong1 << 1)) - 1;
                bool useRanges = edgeCount <= 4;
                if (useRanges)
                {
                    int entityIndex = 0;
                    int batchEndIndex = 0;
                    while (global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeTryGetNextEnabledBitRange(chunkEnabledMask, batchEndIndex, out entityIndex, out batchEndIndex))
                    {
                        while (entityIndex < batchEndIndex)
                        {
                            OriginalLambdaBody(global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetCopyOfNativeArrayPtrElement<global::Unity.Entities.Entity>(__entityArrayPtr, entityIndex), ref global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::ObjectDataCD>(objectDataArrayPtr, entityIndex), in global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::Unity.Transforms.LocalTransform>(translationArrayPtr, entityIndex), in global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::Unity.NetCode.GhostInstance>(ghostInstanceArrayPtr, entityIndex));
                            entityIndex++;
                        }
                    }
                }
                else
                {
                    ulong mask64 = chunkEnabledMask.ULong0;
                    int count = global::Unity.Mathematics.math.min(64, chunkEntityCount);
                    for (var entityIndex = 0; entityIndex < count; ++entityIndex)
                    {
                        if ((mask64 & 1) != 0)
                        {
                            OriginalLambdaBody(global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetCopyOfNativeArrayPtrElement<global::Unity.Entities.Entity>(__entityArrayPtr, entityIndex), ref global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::ObjectDataCD>(objectDataArrayPtr, entityIndex), in global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::Unity.Transforms.LocalTransform>(translationArrayPtr, entityIndex), in global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::Unity.NetCode.GhostInstance>(ghostInstanceArrayPtr, entityIndex));
                        }
                        mask64 >>= 1;
                    }
                    mask64 = chunkEnabledMask.ULong1;
                    for (var entityIndex = 64; entityIndex < chunkEntityCount; ++entityIndex)
                    {
                        if ((mask64 & 1) != 0)
                        {
                            OriginalLambdaBody(global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetCopyOfNativeArrayPtrElement<global::Unity.Entities.Entity>(__entityArrayPtr, entityIndex), ref global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::ObjectDataCD>(objectDataArrayPtr, entityIndex), in global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::Unity.Transforms.LocalTransform>(translationArrayPtr, entityIndex), in global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::Unity.NetCode.GhostInstance>(ghostInstanceArrayPtr, entityIndex));
                        }
                        mask64 >>= 1;
                    }
                }
            }
        }
    }
    void DoorSwitchSystem_7967A976_LambdaJob_0_Execute(double distanceToTriggerLocal,global::UnityEngine.Vector3 playerPosition,global::Unity.NetCode.GhostPredictionSwitchingQueues switchingQueues)
    {
        __TypeHandle.__Unity_Entities_Entity_TypeHandle.Update(ref this.CheckedStateRef);
        __TypeHandle.__ObjectDataCD_RW_ComponentTypeHandle.Update(ref this.CheckedStateRef);
        __TypeHandle.__Unity_Transforms_LocalTransform_RO_ComponentTypeHandle.Update(ref this.CheckedStateRef);
        __TypeHandle.__Unity_NetCode_GhostInstance_RO_ComponentTypeHandle.Update(ref this.CheckedStateRef);
        __TypeHandle.__Unity_NetCode_PredictedGhost_RO_ComponentLookup.Update(ref this.CheckedStateRef);;
        var __job = new DoorSwitchSystem_7967A976_LambdaJob_0_Job
        {
            distanceToTriggerLocal = distanceToTriggerLocal,
            playerPosition = playerPosition,
            switchingQueues = switchingQueues,
            __entityTypeHandle = __TypeHandle.__Unity_Entities_Entity_TypeHandle,
            __objectDataTypeHandle = __TypeHandle.__ObjectDataCD_RW_ComponentTypeHandle,
            __translationTypeHandle = __TypeHandle.__Unity_Transforms_LocalTransform_RO_ComponentTypeHandle,
            __ghostInstanceTypeHandle = __TypeHandle.__Unity_NetCode_GhostInstance_RO_ComponentTypeHandle,
            __Unity_NetCode_PredictedGhost_ComponentLookup = __TypeHandle.__Unity_NetCode_PredictedGhost_RO_ComponentLookup
        };
        
        using (DoorSwitchSystem_7967A976_LambdaJob_0_Job.s_ProfilerMarker.Auto())
        {
            this.CheckedStateRef.Dependency = global::Unity.Entities.Internal.InternalCompilerInterface.JobChunkInterface.Schedule(__job, __query_283678828_0, this.CheckedStateRef.Dependency);
        }
    }
    
    TypeHandle __TypeHandle;
    global::Unity.Entities.EntityQuery __query_283678828_0;
    global::Unity.Entities.EntityQuery __query_283678828_1;
    struct TypeHandle
    {
        [global::Unity.Collections.ReadOnly] public global::Unity.Entities.EntityTypeHandle __Unity_Entities_Entity_TypeHandle;
        public Unity.Entities.ComponentTypeHandle<global::ObjectDataCD> __ObjectDataCD_RW_ComponentTypeHandle;
        [global::Unity.Collections.ReadOnly] public Unity.Entities.ComponentTypeHandle<global::Unity.Transforms.LocalTransform> __Unity_Transforms_LocalTransform_RO_ComponentTypeHandle;
        [global::Unity.Collections.ReadOnly] public Unity.Entities.ComponentTypeHandle<global::Unity.NetCode.GhostInstance> __Unity_NetCode_GhostInstance_RO_ComponentTypeHandle;
        [global::Unity.Collections.ReadOnly] public global::Unity.Entities.ComponentLookup<global::Unity.NetCode.PredictedGhost> __Unity_NetCode_PredictedGhost_RO_ComponentLookup;
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void __AssignHandles(ref global::Unity.Entities.SystemState state)
        {
            __Unity_Entities_Entity_TypeHandle = state.GetEntityTypeHandle();
            __ObjectDataCD_RW_ComponentTypeHandle = state.GetComponentTypeHandle<global::ObjectDataCD>(false);
            __Unity_Transforms_LocalTransform_RO_ComponentTypeHandle = state.GetComponentTypeHandle<global::Unity.Transforms.LocalTransform>(true);
            __Unity_NetCode_GhostInstance_RO_ComponentTypeHandle = state.GetComponentTypeHandle<global::Unity.NetCode.GhostInstance>(true);
            __Unity_NetCode_PredictedGhost_RO_ComponentLookup = state.GetComponentLookup<global::Unity.NetCode.PredictedGhost>(true);
        }
        
    }
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    void __AssignQueries(ref global::Unity.Entities.SystemState state)
    {
        var entityQueryBuilder = new global::Unity.Entities.EntityQueryBuilder(global::Unity.Collections.Allocator.Temp);
        __query_283678828_0 = 
            entityQueryBuilder
                .WithNone<global::Unity.NetCode.SwitchPredictionSmoothing>()
                .WithAll<global::Unity.Transforms.LocalTransform>()
                .WithAll<global::Unity.NetCode.GhostInstance>()
                .WithAll<global::DoorCD>()
                .WithAllRW<global::ObjectDataCD>()
                .Build(ref state);
        entityQueryBuilder.Reset();
        __query_283678828_1 = 
            entityQueryBuilder
                .WithAllRW<global::Unity.NetCode.GhostPredictionSwitchingQueues>()
                .WithOptions(global::Unity.Entities.EntityQueryOptions.IncludeSystems)
                .Build(ref state);
        entityQueryBuilder.Reset();
        entityQueryBuilder.Dispose();
    }
    
    protected override void OnCreateForCompiler()
    {
        base.OnCreateForCompiler();
        __AssignQueries(ref this.CheckedStateRef);
        __TypeHandle.__AssignHandles(ref this.CheckedStateRef);
    }
}
