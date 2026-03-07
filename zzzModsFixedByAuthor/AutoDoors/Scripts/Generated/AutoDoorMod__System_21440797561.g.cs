#pragma warning disable 0219
#line 1 "D:/ModSdkVersions/Version1.0/CoreKeeperModSDK/Temp/GeneratedCode/AutoDoor//AutoDoorMod__System_21440797561.g.cs"
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
public partial class DoorGateStateChecker
{
    [global::Unity.Entities.DOTSCompilerPatchedMethod("OnUpdate_T0")]

    void __OnUpdate_450AADF4()
    {
        #line 132 "D:/ModSdkVersions/Version1.0/CoreKeeperModSDK/Assets/AutoDoor/AutoDoorMod.cs"
        var distanceToTriggerLocal = 0.95;
                #line 135 "D:/ModSdkVersions/Version1.0/CoreKeeperModSDK/Assets/AutoDoor/AutoDoorMod.cs"

        
        var playerPositions = new NativeList<float3>(World.UpdateAllocator.ToAllocator);
        #line 136 "D:/ModSdkVersions/Version1.0/CoreKeeperModSDK/Assets/AutoDoor/AutoDoorMod.cs"

        DoorGateStateChecker_632FFA7C_LambdaJob_0_Execute(playerPositions);
        #line 143 "D:/ModSdkVersions/Version1.0/CoreKeeperModSDK/Assets/AutoDoor/AutoDoorMod.cs"

        DoorGateStateChecker_632FFA7C_LambdaJob_1_Execute(distanceToTriggerLocal, playerPositions);
        #line 163 "D:/ModSdkVersions/Version1.0/CoreKeeperModSDK/Assets/AutoDoor/AutoDoorMod.cs"

        base.OnUpdate();
#line hidden
    }

    #line 38 "D:/ModSdkVersions/Version1.0/CoreKeeperModSDK/Temp/GeneratedCode/AutoDoor//AutoDoorMod__System_21440797561.g.cs"
    [global::Unity.Burst.NoAlias]
    [global::Unity.Burst.BurstCompile]
    struct DoorGateStateChecker_632FFA7C_LambdaJob_0_Job : global::Unity.Entities.IJobChunk
    {
        public global::Unity.Collections.NativeList<global::Unity.Mathematics.float3> playerPositions;
        [global::Unity.Collections.ReadOnly] public global::Unity.Entities.ComponentTypeHandle<global::Unity.Transforms.LocalTransform> __translationTypeHandle;
        public static readonly global::Unity.Profiling.ProfilerMarker s_ProfilerMarker = new global::Unity.Profiling.ProfilerMarker("DoorGateStateChecker_632FFA7C_LambdaJob_0.Schedule");
        
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        void OriginalLambdaBody([Unity.Burst.NoAlias] in global::Unity.Transforms.LocalTransform translation)
        {
#line 139 "D:\ModSdkVersions\Version1.0\CoreKeeperModSDK\Assets/AutoDoor/AutoDoorMod.cs"
playerPositions.Add(translation.Position);
            }
        #line 53 "D:/ModSdkVersions/Version1.0/CoreKeeperModSDK/Temp/GeneratedCode/AutoDoor//AutoDoorMod__System_21440797561.g.cs"
        [global::System.Runtime.CompilerServices.CompilerGenerated]
        public void Execute(in global::Unity.Entities.ArchetypeChunk chunk, int batchIndex, bool useEnabledMask, in global::Unity.Burst.Intrinsics.v128 chunkEnabledMask)
        {
            #line 57 "D:/ModSdkVersions/Version1.0/CoreKeeperModSDK/Temp/GeneratedCode/AutoDoor//AutoDoorMod__System_21440797561.g.cs"
            var translationArrayPtr = global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetChunkNativeArrayReadOnlyIntPtr<global::Unity.Transforms.LocalTransform>(chunk, ref __translationTypeHandle);
            int chunkEntityCount = chunk.Count;
            if (!useEnabledMask)
            {
                for(var entityIndex = 0; entityIndex < chunkEntityCount; ++entityIndex)
                {
                    OriginalLambdaBody(in global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::Unity.Transforms.LocalTransform>(translationArrayPtr, entityIndex));
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
                            OriginalLambdaBody(in global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::Unity.Transforms.LocalTransform>(translationArrayPtr, entityIndex));
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
                            OriginalLambdaBody(in global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::Unity.Transforms.LocalTransform>(translationArrayPtr, entityIndex));
                        }
                        mask64 >>= 1;
                    }
                    mask64 = chunkEnabledMask.ULong1;
                    for (var entityIndex = 64; entityIndex < chunkEntityCount; ++entityIndex)
                    {
                        if ((mask64 & 1) != 0)
                        {
                            OriginalLambdaBody(in global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::Unity.Transforms.LocalTransform>(translationArrayPtr, entityIndex));
                        }
                        mask64 >>= 1;
                    }
                }
            }
        }
    }
    void DoorGateStateChecker_632FFA7C_LambdaJob_0_Execute(global::Unity.Collections.NativeList<global::Unity.Mathematics.float3> playerPositions)
    {
        __TypeHandle.__Unity_Transforms_LocalTransform_RO_ComponentTypeHandle.Update(ref this.CheckedStateRef);
        var __job = new DoorGateStateChecker_632FFA7C_LambdaJob_0_Job
        {
            playerPositions = playerPositions,
            __translationTypeHandle = __TypeHandle.__Unity_Transforms_LocalTransform_RO_ComponentTypeHandle
        };
        
        using (DoorGateStateChecker_632FFA7C_LambdaJob_0_Job.s_ProfilerMarker.Auto())
        {
            this.CheckedStateRef.Dependency = global::Unity.Entities.Internal.InternalCompilerInterface.JobChunkInterface.Schedule(__job, __query_283678886_0, this.CheckedStateRef.Dependency);
        }
    }
    #line 123 "D:/ModSdkVersions/Version1.0/CoreKeeperModSDK/Temp/GeneratedCode/AutoDoor//AutoDoorMod__System_21440797561.g.cs"
    struct DoorGateStateChecker_632FFA7C_LambdaJob_1_Job : global::Unity.Entities.IJobChunk
    {
        public double distanceToTriggerLocal;
        public global::Unity.Collections.NativeList<global::Unity.Mathematics.float3> playerPositions;
        public global::Unity.Entities.ComponentTypeHandle<global::ObjectDataCD> __objectDataTypeHandle;
        [global::Unity.Collections.ReadOnly] public global::Unity.Entities.ComponentTypeHandle<global::Unity.Transforms.LocalTransform> __translationTypeHandle;
        public static readonly global::Unity.Profiling.ProfilerMarker s_ProfilerMarker = new global::Unity.Profiling.ProfilerMarker("DoorGateStateChecker_632FFA7C_LambdaJob_1.Schedule");
        
        void OriginalLambdaBody(ref global::ObjectDataCD objectData, in global::Unity.Transforms.LocalTransform translation)
        {
#line 147 "D:\ModSdkVersions\Version1.0\CoreKeeperModSDK\Assets/AutoDoor/AutoDoorMod.cs"
var anyPlayerNearby = false;
#line 148 "D:\ModSdkVersions\Version1.0\CoreKeeperModSDK\Assets/AutoDoor/AutoDoorMod.cs"
foreach (var playerPos in playerPositions)
                {
#line 150 "D:\ModSdkVersions\Version1.0\CoreKeeperModSDK\Assets/AutoDoor/AutoDoorMod.cs"
var distance = math.distancesq(translation.Position, playerPos);
#line 151 "D:\ModSdkVersions\Version1.0\CoreKeeperModSDK\Assets/AutoDoor/AutoDoorMod.cs"
if (distance > distanceToTriggerLocal) 
#line 151 "D:\ModSdkVersions\Version1.0\CoreKeeperModSDK\Assets/AutoDoor/AutoDoorMod.cs"
continue;
#line 152 "D:\ModSdkVersions\Version1.0\CoreKeeperModSDK\Assets/AutoDoor/AutoDoorMod.cs"
anyPlayerNearby = true;
#line 154 "D:\ModSdkVersions\Version1.0\CoreKeeperModSDK\Assets/AutoDoor/AutoDoorMod.cs"
Debug.Log($"Gate/Door ({objectData.objectID}) distance to player is {distance} and variation is {objectData.variation}");
                }
#line 157 "D:\ModSdkVersions\Version1.0\CoreKeeperModSDK\Assets/AutoDoor/AutoDoorMod.cs"
SetOpen(ref objectData, anyPlayerNearby);
            }
        #line 152 "D:/ModSdkVersions/Version1.0/CoreKeeperModSDK/Temp/GeneratedCode/AutoDoor//AutoDoorMod__System_21440797561.g.cs"
        [global::System.Runtime.CompilerServices.CompilerGenerated]
        public void Execute(in global::Unity.Entities.ArchetypeChunk chunk, int batchIndex, bool useEnabledMask, in global::Unity.Burst.Intrinsics.v128 chunkEnabledMask)
        {
            #line 156 "D:/ModSdkVersions/Version1.0/CoreKeeperModSDK/Temp/GeneratedCode/AutoDoor//AutoDoorMod__System_21440797561.g.cs"
            var objectDataArrayPtr = global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetChunkNativeArrayIntPtr<global::ObjectDataCD>(chunk, ref __objectDataTypeHandle);
            var translationArrayPtr = global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetChunkNativeArrayReadOnlyIntPtr<global::Unity.Transforms.LocalTransform>(chunk, ref __translationTypeHandle);
            int chunkEntityCount = chunk.Count;
            if (!useEnabledMask)
            {
                for(var entityIndex = 0; entityIndex < chunkEntityCount; ++entityIndex)
                {
                    OriginalLambdaBody(ref global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::ObjectDataCD>(objectDataArrayPtr, entityIndex), in global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::Unity.Transforms.LocalTransform>(translationArrayPtr, entityIndex));
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
                            OriginalLambdaBody(ref global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::ObjectDataCD>(objectDataArrayPtr, entityIndex), in global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::Unity.Transforms.LocalTransform>(translationArrayPtr, entityIndex));
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
                            OriginalLambdaBody(ref global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::ObjectDataCD>(objectDataArrayPtr, entityIndex), in global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::Unity.Transforms.LocalTransform>(translationArrayPtr, entityIndex));
                        }
                        mask64 >>= 1;
                    }
                    mask64 = chunkEnabledMask.ULong1;
                    for (var entityIndex = 64; entityIndex < chunkEntityCount; ++entityIndex)
                    {
                        if ((mask64 & 1) != 0)
                        {
                            OriginalLambdaBody(ref global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::ObjectDataCD>(objectDataArrayPtr, entityIndex), in global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::Unity.Transforms.LocalTransform>(translationArrayPtr, entityIndex));
                        }
                        mask64 >>= 1;
                    }
                }
            }
        }
        public global::Unity.Jobs.JobHandle DisposeOnCompletion(global::Unity.Jobs.JobHandle jobHandle)
        {
            jobHandle = playerPositions.Dispose(jobHandle);
            return jobHandle;
        }
    }
    void DoorGateStateChecker_632FFA7C_LambdaJob_1_Execute(double distanceToTriggerLocal,global::Unity.Collections.NativeList<global::Unity.Mathematics.float3> playerPositions)
    {
        __TypeHandle.__ObjectDataCD_RW_ComponentTypeHandle.Update(ref this.CheckedStateRef);
        __TypeHandle.__Unity_Transforms_LocalTransform_RO_ComponentTypeHandle.Update(ref this.CheckedStateRef);
        var __job = new DoorGateStateChecker_632FFA7C_LambdaJob_1_Job
        {
            distanceToTriggerLocal = distanceToTriggerLocal,
            playerPositions = playerPositions,
            __objectDataTypeHandle = __TypeHandle.__ObjectDataCD_RW_ComponentTypeHandle,
            __translationTypeHandle = __TypeHandle.__Unity_Transforms_LocalTransform_RO_ComponentTypeHandle
        };
        
        using (DoorGateStateChecker_632FFA7C_LambdaJob_1_Job.s_ProfilerMarker.Auto())
        {
            this.CheckedStateRef.Dependency = global::Unity.Entities.Internal.InternalCompilerInterface.JobChunkInterface.Schedule(__job, __query_283678886_1, this.CheckedStateRef.Dependency);
        }
        this.CheckedStateRef.Dependency = __job.DisposeOnCompletion(this.CheckedStateRef.Dependency);
    }
    
    TypeHandle __TypeHandle;
    global::Unity.Entities.EntityQuery __query_283678886_0;
    global::Unity.Entities.EntityQuery __query_283678886_1;
    struct TypeHandle
    {
        [global::Unity.Collections.ReadOnly] public Unity.Entities.ComponentTypeHandle<global::Unity.Transforms.LocalTransform> __Unity_Transforms_LocalTransform_RO_ComponentTypeHandle;
        public Unity.Entities.ComponentTypeHandle<global::ObjectDataCD> __ObjectDataCD_RW_ComponentTypeHandle;
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void __AssignHandles(ref global::Unity.Entities.SystemState state)
        {
            __Unity_Transforms_LocalTransform_RO_ComponentTypeHandle = state.GetComponentTypeHandle<global::Unity.Transforms.LocalTransform>(true);
            __ObjectDataCD_RW_ComponentTypeHandle = state.GetComponentTypeHandle<global::ObjectDataCD>(false);
        }
        
    }
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    void __AssignQueries(ref global::Unity.Entities.SystemState state)
    {
        var entityQueryBuilder = new global::Unity.Entities.EntityQueryBuilder(global::Unity.Collections.Allocator.Temp);
        __query_283678886_0 = 
            entityQueryBuilder
                .WithAll<global::Unity.Transforms.LocalTransform>()
                .WithAll<global::PlayerGhost>()
                .Build(ref state);
        entityQueryBuilder.Reset();
        __query_283678886_1 = 
            entityQueryBuilder
                .WithAll<global::Unity.Transforms.LocalTransform>()
                .WithAll<global::Unity.NetCode.PredictedGhost>()
                .WithAll<global::Unity.Entities.Simulate>()
                .WithAll<global::DoorCD>()
                .WithAllRW<global::ObjectDataCD>()
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
