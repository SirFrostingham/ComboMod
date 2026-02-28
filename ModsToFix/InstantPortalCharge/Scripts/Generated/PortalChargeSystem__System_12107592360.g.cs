#pragma warning disable 0219
#line 1 "C:/Projects/Unity/Projects/CoreKeeperMods/SDK Mods/Temp/GeneratedCode/InstantPortalCharge//PortalChargeSystem__System_12107592360.g.cs"
using Unity.Entities;
using Unity.NetCode;
namespace InstantPortalCharge
{
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    public partial class PortalChargeSystem
    {
        [global::Unity.Entities.DOTSCompilerPatchedMethod("OnUpdate_T0")]
        void __OnUpdate_450AADF4()
        {
            #line 12 "C:/Projects/Unity/Projects/CoreKeeperMods/SDK Mods/Assets/Mods/InstantPortalCharge/PortalChargeSystem.cs"

            PortalCharge_Execute();
            #line 29 "C:/Projects/Unity/Projects/CoreKeeperMods/SDK Mods/Assets/Mods/InstantPortalCharge/PortalChargeSystem.cs"

            WayPointCharge_Execute();
            #line 46 "C:/Projects/Unity/Projects/CoreKeeperMods/SDK Mods/Assets/Mods/InstantPortalCharge/PortalChargeSystem.cs"

            base.OnUpdate();
#line hidden
        }

        #line 26 "C:/Projects/Unity/Projects/CoreKeeperMods/SDK Mods/Temp/GeneratedCode/InstantPortalCharge//PortalChargeSystem__System_12107592360.g.cs"
        [global::Unity.Burst.NoAlias]
        [global::Unity.Burst.BurstCompile]
        struct PortalCharge_Job : global::Unity.Entities.IJobChunk
        {
            public global::Unity.Entities.ComponentTypeHandle<global::ObjectDataCD> __objectDataCdTypeHandle;
            
            [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            void OriginalLambdaBody([Unity.Burst.NoAlias] ref global::ObjectDataCD objectDataCd)
            {
#line 16 "C:\Projects\Unity\Projects\CoreKeeperMods\SDK Mods\Assets/Mods/InstantPortalCharge/PortalChargeSystem.cs"
if (objectDataCd.amount < 1200)
                    {
#line 18 "C:\Projects\Unity\Projects\CoreKeeperMods\SDK Mods\Assets/Mods/InstantPortalCharge/PortalChargeSystem.cs"
objectDataCd.amount = 1200;
                    }
                }
            #line 43 "C:/Projects/Unity/Projects/CoreKeeperMods/SDK Mods/Temp/GeneratedCode/InstantPortalCharge//PortalChargeSystem__System_12107592360.g.cs"
            [global::System.Runtime.CompilerServices.CompilerGenerated]
            public void Execute(in global::Unity.Entities.ArchetypeChunk chunk, int batchIndex, bool useEnabledMask, in global::Unity.Burst.Intrinsics.v128 chunkEnabledMask)
            {
                #line 47 "C:/Projects/Unity/Projects/CoreKeeperMods/SDK Mods/Temp/GeneratedCode/InstantPortalCharge//PortalChargeSystem__System_12107592360.g.cs"
                var objectDataCdArrayPtr = global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetChunkNativeArrayIntPtr<global::ObjectDataCD>(chunk, ref __objectDataCdTypeHandle);
                int chunkEntityCount = chunk.Count;
                if (!useEnabledMask)
                {
                    for(var entityIndex = 0; entityIndex < chunkEntityCount; ++entityIndex)
                    {
                        OriginalLambdaBody(ref global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::ObjectDataCD>(objectDataCdArrayPtr, entityIndex));
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
                                OriginalLambdaBody(ref global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::ObjectDataCD>(objectDataCdArrayPtr, entityIndex));
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
                                OriginalLambdaBody(ref global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::ObjectDataCD>(objectDataCdArrayPtr, entityIndex));
                            }
                            mask64 >>= 1;
                        }
                        mask64 = chunkEnabledMask.ULong1;
                        for (var entityIndex = 64; entityIndex < chunkEntityCount; ++entityIndex)
                        {
                            if ((mask64 & 1) != 0)
                            {
                                OriginalLambdaBody(ref global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::ObjectDataCD>(objectDataCdArrayPtr, entityIndex));
                            }
                            mask64 >>= 1;
                        }
                    }
                }
            }
        }
        void PortalCharge_Execute()
        {
            __TypeHandle.__ObjectDataCD_RW_ComponentTypeHandle.Update(ref this.CheckedStateRef);
            var __job = new PortalCharge_Job
            {
                __objectDataCdTypeHandle = __TypeHandle.__ObjectDataCD_RW_ComponentTypeHandle
            };
            
            this.CheckedStateRef.Dependency = global::Unity.Entities.Internal.InternalCompilerInterface.JobChunkInterface.Schedule(__job, __query_1216395462_0, this.CheckedStateRef.Dependency);
        }
        #line 109 "C:/Projects/Unity/Projects/CoreKeeperMods/SDK Mods/Temp/GeneratedCode/InstantPortalCharge//PortalChargeSystem__System_12107592360.g.cs"
        [global::Unity.Burst.NoAlias]
        [global::Unity.Burst.BurstCompile]
        struct WayPointCharge_Job : global::Unity.Entities.IJobChunk
        {
            public global::Unity.Entities.ComponentTypeHandle<global::ObjectDataCD> __objectDataCdTypeHandle;
            [global::Unity.Collections.ReadOnly] public global::Unity.Entities.ComponentTypeHandle<global::WayPointCD> __wayPointTypeHandle;
            [global::Unity.Collections.ReadOnly] public global::Unity.Entities.ComponentTypeHandle<global::DistanceToPlayerCD> __distanceTypeHandle;
            
            [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            void OriginalLambdaBody([Unity.Burst.NoAlias] ref global::ObjectDataCD objectDataCd, [Unity.Burst.NoAlias] in global::WayPointCD wayPoint, [Unity.Burst.NoAlias] in global::DistanceToPlayerCD distance)
            {
#line 35 "C:\Projects\Unity\Projects\CoreKeeperMods\SDK Mods\Assets/Mods/InstantPortalCharge/PortalChargeSystem.cs"
if (objectDataCd.amount >= 600) 
#line 35 "C:\Projects\Unity\Projects\CoreKeeperMods\SDK Mods\Assets/Mods/InstantPortalCharge/PortalChargeSystem.cs"
return;
#line 37 "C:\Projects\Unity\Projects\CoreKeeperMods\SDK Mods\Assets/Mods/InstantPortalCharge/PortalChargeSystem.cs"
float minDis = distance.minDistanceSq;
#line 38 "C:\Projects\Unity\Projects\CoreKeeperMods\SDK Mods\Assets/Mods/InstantPortalCharge/PortalChargeSystem.cs"
if (!(minDis > 0) || !(minDis <= wayPoint.distanceToActivateSQ)) 
#line 38 "C:\Projects\Unity\Projects\CoreKeeperMods\SDK Mods\Assets/Mods/InstantPortalCharge/PortalChargeSystem.cs"
return;
#line 40 "C:\Projects\Unity\Projects\CoreKeeperMods\SDK Mods\Assets/Mods/InstantPortalCharge/PortalChargeSystem.cs"
objectDataCd.amount = 600;
                }
            #line 132 "C:/Projects/Unity/Projects/CoreKeeperMods/SDK Mods/Temp/GeneratedCode/InstantPortalCharge//PortalChargeSystem__System_12107592360.g.cs"
            [global::System.Runtime.CompilerServices.CompilerGenerated]
            public void Execute(in global::Unity.Entities.ArchetypeChunk chunk, int batchIndex, bool useEnabledMask, in global::Unity.Burst.Intrinsics.v128 chunkEnabledMask)
            {
                #line 136 "C:/Projects/Unity/Projects/CoreKeeperMods/SDK Mods/Temp/GeneratedCode/InstantPortalCharge//PortalChargeSystem__System_12107592360.g.cs"
                var objectDataCdArrayPtr = global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetChunkNativeArrayIntPtr<global::ObjectDataCD>(chunk, ref __objectDataCdTypeHandle);
                var wayPointArrayPtr = global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetChunkNativeArrayReadOnlyIntPtr<global::WayPointCD>(chunk, ref __wayPointTypeHandle);
                var distanceArrayPtr = global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetChunkNativeArrayReadOnlyIntPtr<global::DistanceToPlayerCD>(chunk, ref __distanceTypeHandle);
                int chunkEntityCount = chunk.Count;
                if (!useEnabledMask)
                {
                    for(var entityIndex = 0; entityIndex < chunkEntityCount; ++entityIndex)
                    {
                        OriginalLambdaBody(ref global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::ObjectDataCD>(objectDataCdArrayPtr, entityIndex), in global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::WayPointCD>(wayPointArrayPtr, entityIndex), in global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::DistanceToPlayerCD>(distanceArrayPtr, entityIndex));
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
                                OriginalLambdaBody(ref global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::ObjectDataCD>(objectDataCdArrayPtr, entityIndex), in global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::WayPointCD>(wayPointArrayPtr, entityIndex), in global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::DistanceToPlayerCD>(distanceArrayPtr, entityIndex));
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
                                OriginalLambdaBody(ref global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::ObjectDataCD>(objectDataCdArrayPtr, entityIndex), in global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::WayPointCD>(wayPointArrayPtr, entityIndex), in global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::DistanceToPlayerCD>(distanceArrayPtr, entityIndex));
                            }
                            mask64 >>= 1;
                        }
                        mask64 = chunkEnabledMask.ULong1;
                        for (var entityIndex = 64; entityIndex < chunkEntityCount; ++entityIndex)
                        {
                            if ((mask64 & 1) != 0)
                            {
                                OriginalLambdaBody(ref global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::ObjectDataCD>(objectDataCdArrayPtr, entityIndex), in global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::WayPointCD>(wayPointArrayPtr, entityIndex), in global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::DistanceToPlayerCD>(distanceArrayPtr, entityIndex));
                            }
                            mask64 >>= 1;
                        }
                    }
                }
            }
        }
        void WayPointCharge_Execute()
        {
            __TypeHandle.__ObjectDataCD_RW_ComponentTypeHandle.Update(ref this.CheckedStateRef);
            __TypeHandle.__WayPointCD_RO_ComponentTypeHandle.Update(ref this.CheckedStateRef);
            __TypeHandle.__DistanceToPlayerCD_RO_ComponentTypeHandle.Update(ref this.CheckedStateRef);
            var __job = new WayPointCharge_Job
            {
                __objectDataCdTypeHandle = __TypeHandle.__ObjectDataCD_RW_ComponentTypeHandle,
                __wayPointTypeHandle = __TypeHandle.__WayPointCD_RO_ComponentTypeHandle,
                __distanceTypeHandle = __TypeHandle.__DistanceToPlayerCD_RO_ComponentTypeHandle
            };
            
            this.CheckedStateRef.Dependency = global::Unity.Entities.Internal.InternalCompilerInterface.JobChunkInterface.Schedule(__job, __query_1216395462_1, this.CheckedStateRef.Dependency);
        }
        
        TypeHandle __TypeHandle;
        global::Unity.Entities.EntityQuery __query_1216395462_0;
        global::Unity.Entities.EntityQuery __query_1216395462_1;
        struct TypeHandle
        {
            public Unity.Entities.ComponentTypeHandle<global::ObjectDataCD> __ObjectDataCD_RW_ComponentTypeHandle;
            [global::Unity.Collections.ReadOnly] public Unity.Entities.ComponentTypeHandle<global::WayPointCD> __WayPointCD_RO_ComponentTypeHandle;
            [global::Unity.Collections.ReadOnly] public Unity.Entities.ComponentTypeHandle<global::DistanceToPlayerCD> __DistanceToPlayerCD_RO_ComponentTypeHandle;
            [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            public void __AssignHandles(ref global::Unity.Entities.SystemState state)
            {
                __ObjectDataCD_RW_ComponentTypeHandle = state.GetComponentTypeHandle<global::ObjectDataCD>(false);
                __WayPointCD_RO_ComponentTypeHandle = state.GetComponentTypeHandle<global::WayPointCD>(true);
                __DistanceToPlayerCD_RO_ComponentTypeHandle = state.GetComponentTypeHandle<global::DistanceToPlayerCD>(true);
            }
            
        }
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        void __AssignQueries(ref global::Unity.Entities.SystemState state)
        {
            var entityQueryBuilder = new global::Unity.Entities.EntityQueryBuilder(global::Unity.Collections.Allocator.Temp);
            __query_1216395462_0 = 
                entityQueryBuilder
                    .WithNone<global::WayPointCD>()
                    .WithNone<global::EntityDestroyedCD>()
                    .WithAll<global::PortalCD>()
                    .WithAllRW<global::ObjectDataCD>()
                    .WithOptions(global::Unity.Entities.EntityQueryOptions.IncludeDisabledEntities)
                    .Build(ref state);
            entityQueryBuilder.Reset();
            __query_1216395462_1 = 
                entityQueryBuilder
                    .WithAll<global::WayPointCD>()
                    .WithAll<global::DistanceToPlayerCD>()
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
}
