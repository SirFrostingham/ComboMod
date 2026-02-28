#pragma warning disable 0219
#line 1 "C:/Projects/Unity/Projects/CoreKeeperMods/SDK Mods/Temp/GeneratedCode/InfiniteOreBoulder//InfiniteBoulderSystem__System_6589417270.g.cs"
using Unity.Entities;
using Unity.NetCode;
namespace InfiniteOreBoulder
{
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    public partial class InfiniteBoulderSystem
    {
        [global::Unity.Entities.DOTSCompilerPatchedMethod("OnUpdate_T0")]
        void __OnUpdate_450AADF4()
        {
            #line 13 "C:/Projects/Unity/Projects/CoreKeeperMods/SDK Mods/Assets/Mods/InfiniteOreBoulder/InfiniteBoulderSystem.cs"

            BoulderHeal_Execute();
            #line 29 "C:/Projects/Unity/Projects/CoreKeeperMods/SDK Mods/Assets/Mods/InfiniteOreBoulder/InfiniteBoulderSystem.cs"

            base.OnUpdate();
#line hidden
        }

        #line 24 "C:/Projects/Unity/Projects/CoreKeeperMods/SDK Mods/Temp/GeneratedCode/InfiniteOreBoulder//InfiniteBoulderSystem__System_6589417270.g.cs"
        [global::Unity.Burst.NoAlias]
        [global::Unity.Burst.BurstCompile]
        struct BoulderHeal_Job : global::Unity.Entities.IJobChunk
        {
            public global::Unity.Entities.ComponentTypeHandle<global::HealthCD> __healthCdTypeHandle;
            
            [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            void OriginalLambdaBody([Unity.Burst.NoAlias] ref global::HealthCD healthCd)
            {
#line 15 "C:\Projects\Unity\Projects\CoreKeeperMods\SDK Mods\Assets/Mods/InfiniteOreBoulder/InfiniteBoulderSystem.cs"
if (healthCd.health < healthCd.maxHealth / 2)
                    {
#line 17 "C:\Projects\Unity\Projects\CoreKeeperMods\SDK Mods\Assets/Mods/InfiniteOreBoulder/InfiniteBoulderSystem.cs"
healthCd.health = healthCd.maxHealth;
                    }
                }
            #line 41 "C:/Projects/Unity/Projects/CoreKeeperMods/SDK Mods/Temp/GeneratedCode/InfiniteOreBoulder//InfiniteBoulderSystem__System_6589417270.g.cs"
            [global::System.Runtime.CompilerServices.CompilerGenerated]
            public void Execute(in global::Unity.Entities.ArchetypeChunk chunk, int batchIndex, bool useEnabledMask, in global::Unity.Burst.Intrinsics.v128 chunkEnabledMask)
            {
                #line 45 "C:/Projects/Unity/Projects/CoreKeeperMods/SDK Mods/Temp/GeneratedCode/InfiniteOreBoulder//InfiniteBoulderSystem__System_6589417270.g.cs"
                var healthCdArrayPtr = global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetChunkNativeArrayIntPtr<global::HealthCD>(chunk, ref __healthCdTypeHandle);
                int chunkEntityCount = chunk.Count;
                if (!useEnabledMask)
                {
                    for(var entityIndex = 0; entityIndex < chunkEntityCount; ++entityIndex)
                    {
                        OriginalLambdaBody(ref global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::HealthCD>(healthCdArrayPtr, entityIndex));
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
                                OriginalLambdaBody(ref global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::HealthCD>(healthCdArrayPtr, entityIndex));
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
                                OriginalLambdaBody(ref global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::HealthCD>(healthCdArrayPtr, entityIndex));
                            }
                            mask64 >>= 1;
                        }
                        mask64 = chunkEnabledMask.ULong1;
                        for (var entityIndex = 64; entityIndex < chunkEntityCount; ++entityIndex)
                        {
                            if ((mask64 & 1) != 0)
                            {
                                OriginalLambdaBody(ref global::Unity.Entities.Internal.InternalCompilerInterface.UnsafeGetRefToNativeArrayPtrElement<global::HealthCD>(healthCdArrayPtr, entityIndex));
                            }
                            mask64 >>= 1;
                        }
                    }
                }
            }
        }
        void BoulderHeal_Execute()
        {
            __TypeHandle.__HealthCD_RW_ComponentTypeHandle.Update(ref this.CheckedStateRef);
            var __job = new BoulderHeal_Job
            {
                __healthCdTypeHandle = __TypeHandle.__HealthCD_RW_ComponentTypeHandle
            };
            
            this.CheckedStateRef.Dependency = global::Unity.Entities.Internal.InternalCompilerInterface.JobChunkInterface.Schedule(__job, __query_1717227304_0, this.CheckedStateRef.Dependency);
        }
        
        TypeHandle __TypeHandle;
        global::Unity.Entities.EntityQuery __query_1717227304_0;
        struct TypeHandle
        {
            public Unity.Entities.ComponentTypeHandle<global::HealthCD> __HealthCD_RW_ComponentTypeHandle;
            [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            public void __AssignHandles(ref global::Unity.Entities.SystemState state)
            {
                __HealthCD_RW_ComponentTypeHandle = state.GetComponentTypeHandle<global::HealthCD>(false);
            }
            
        }
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        void __AssignQueries(ref global::Unity.Entities.SystemState state)
        {
            var entityQueryBuilder = new global::Unity.Entities.EntityQueryBuilder(global::Unity.Collections.Allocator.Temp);
            __query_1717227304_0 = 
                entityQueryBuilder
                    .WithAll<global::PugAutomationCD>()
                    .WithAll<global::DropsLootWhenDamagedCD>()
                    .WithAll<global::MineableDamageDecreaseCD>()
                    .WithAllRW<global::HealthCD>()
                    .WithOptions(global::Unity.Entities.EntityQueryOptions.IncludeDisabledEntities)
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
