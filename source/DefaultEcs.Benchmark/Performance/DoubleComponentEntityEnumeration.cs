using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using DefaultEcs.System;
using DefaultEcs.Threading;
using Entitas;
using DefaultEntity = DefaultEcs.Entity;
using DefaultEntitySet = DefaultEcs.EntitySet;
using DefaultWorld = DefaultEcs.World;
using EntitasEntity = Entitas.Entity;
using EntitiasWorld = Entitas.IContext<Entitas.Entity>;

namespace DefaultEcs.Benchmark.Performance
{
    [MemoryDiagnoser]
    public class DoubleComponentEntityEnumeration
    {
        private const float Time = 1f / 60f;

        private struct DefaultSpeed
        {
            public float X;
            public float Y;
        }

        private struct DefaultPosition
        {
            public float X;
            public float Y;
        }

        private sealed class DefaultEcsSystem : AEntitySystem<float>
        {
            public DefaultEcsSystem(DefaultWorld world, IParallelRunner runner)
                : base(world.GetEntities().With<DefaultSpeed>().With<DefaultPosition>().AsSet(), runner)
            { }

            public DefaultEcsSystem(DefaultWorld world)
                : this(world, null)
            { }

            protected unsafe override void Update(float state, ReadOnlySpan<DefaultEntity> entities)
            {
                foreach (ref readonly DefaultEntity entity in entities)
                {
                    DefaultSpeed speed = entity.Get<DefaultSpeed>();
                    ref DefaultPosition position = ref entity.Get<DefaultPosition>();

                    position.X += speed.X * state;
                    position.Y += speed.Y * state;

                    entity.Set(in position);
                }
            }
        }

        private sealed class DefaultEcsBatchedChangeSystem : AEntitySystem<float>
        {
            public DefaultEcsBatchedChangeSystem(DefaultWorld world, IParallelRunner runner)
                : base(world.GetEntities().With<DefaultSpeed>().With<DefaultPosition>().AsSet(), runner)
            { }

            public DefaultEcsBatchedChangeSystem(DefaultWorld world)
                : this(world, null)
            { }

            protected unsafe override void Update(float state, ReadOnlySpan<DefaultEntity> entities)
            {
                foreach (ref readonly DefaultEntity entity in entities)
                {
                    DefaultSpeed speed = entity.Get<DefaultSpeed>();
                    ref DefaultPosition position = ref entity.Get<DefaultPosition>();

                    position.X += speed.X * state;
                    position.Y += speed.Y * state;
                }

                foreach (ref readonly DefaultEntity entity in entities)
                {
                    entity.MarkChanged<DefaultPosition>();
                }
            }
        }

        private sealed class DefaultEcsComponentSystem : AEntitySystem<float>
        {
            private readonly DefaultWorld _world;

            public DefaultEcsComponentSystem(DefaultWorld world, IParallelRunner runner)
                : base(world.GetEntities().With<DefaultSpeed>().With<DefaultPosition>().AsSet(), runner)
            {
                _world = world;
            }

            protected unsafe override void Update(float state, ReadOnlySpan<DefaultEntity> entities)
            {
                Components<DefaultSpeed> speeds = _world.GetComponents<DefaultSpeed>();
                Components<DefaultPosition> positions = _world.GetComponents<DefaultPosition>();

                foreach (ref readonly DefaultEntity entity in entities)
                {
                    DefaultSpeed speed = speeds[entity];
                    ref DefaultPosition position = ref positions[entity];

                    position.X += speed.X * state;
                    position.Y += speed.Y * state;

                    entity.Set(in position);
                }
            }
        }

        private sealed class DefaultEcsComponentBatchedUpdateSystem : AEntitySystem<float>
        {
            private readonly DefaultWorld _world;

            public DefaultEcsComponentBatchedUpdateSystem(DefaultWorld world, IParallelRunner runner)
                : base(world.GetEntities().With<DefaultSpeed>().With<DefaultPosition>().AsSet(), runner)
            {
                _world = world;
            }

            protected unsafe override void Update(float state, ReadOnlySpan<DefaultEntity> entities)
            {
                Components<DefaultSpeed> speeds = _world.GetComponents<DefaultSpeed>();
                Components<DefaultPosition> positions = _world.GetComponents<DefaultPosition>();

                foreach (ref readonly DefaultEntity entity in entities)
                {
                    DefaultSpeed speed = speeds[entity];
                    ref DefaultPosition position = ref positions[entity];

                    position.X += speed.X * state;
                    position.Y += speed.Y * state;
                }

                foreach (ref readonly DefaultEntity entity in entities)
                {
                    entity.MarkChanged<DefaultPosition>();
                }
            }
        }

        private class EntitasSpeed : IComponent
        {
            public float X;
            public float Y;
        }

        private class EntitasPosition : IComponent
        {
            public float X;
            public float Y;
        }

        public class EntitasExecuteSystem : IExecuteSystem
        {

            IGroup<EntitasEntity> group;
            List<EntitasEntity> buffer;

            public EntitasExecuteSystem(EntitiasWorld world)
            {
                group = world.GetGroup(Matcher<EntitasEntity>.AllOf(0, 1));
                buffer = new List<EntitasEntity>();
            }

            public void Execute()
            {
                group.GetEntities(buffer);
                foreach (var entity in buffer)
                {
                    EntitasSpeed speed = (EntitasSpeed)entity.GetComponent(0);
                    EntitasPosition position = (EntitasPosition)entity.GetComponent(1);
                    position.X += speed.X * Time;
                    position.Y += speed.Y * Time;
                    entity.ReplaceComponent(1, position);
                }
            }
        }

        public class EntitasSystem : JobSystem<EntitasEntity>
        {
            public EntitasSystem(EntitiasWorld world, int threadCount) : base(world.GetGroup(Matcher<EntitasEntity>.AllOf(0, 1)), threadCount)
            { }

            public EntitasSystem(EntitiasWorld world) : this(world, 1)
            { }

            protected override void Execute(EntitasEntity entity)
            {
                EntitasSpeed speed = (EntitasSpeed)entity.GetComponent(0);
                EntitasPosition position = (EntitasPosition)entity.GetComponent(1);
                position.X += speed.X * Time;
                position.Y += speed.Y * Time;
                entity.ReplaceComponent(1, position);
            }
        }

        private DefaultWorld _defaultWorld;
        private DefaultEntitySet _defaultEntitySet;
        private DefaultParallelRunner _defaultRunner;
        private DefaultEcsSystem _defaultSystem;
        private DefaultEcsBatchedChangeSystem _defaultBatchedSystem;
        private DefaultEcsSystem _defaultMultiSystem;
        private DefaultEcsBatchedChangeSystem _defaultMultiBatchedSystem;
        private DefaultEcsComponentSystem _defaultComponentSystem;
        private DefaultEcsComponentBatchedUpdateSystem _defaultComponentBatchedSystem;
        private DefaultEcsComponentSystem _defaultMultiComponentSystem;
        private DefaultEcsComponentBatchedUpdateSystem _defaultMultiComponentBatchedSystem;

        private EntitiasWorld _entitasWorld;
        private EntitasExecuteSystem _entitasExecuteSystem;
        private EntitasSystem _entitasSystem;
        private EntitasSystem _entitasMultiSystem;

        [Params(1000)]
        public int EntityCount { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            _defaultWorld = new DefaultWorld(EntityCount);
            _defaultEntitySet = _defaultWorld.GetEntities().With<DefaultSpeed>().With<DefaultPosition>().AsSet();
            _defaultRunner = new DefaultParallelRunner(Environment.ProcessorCount);
            _defaultSystem = new DefaultEcsSystem(_defaultWorld);
            _defaultBatchedSystem = new DefaultEcsBatchedChangeSystem(_defaultWorld);
            _defaultMultiSystem = new DefaultEcsSystem(_defaultWorld, _defaultRunner);
            _defaultMultiBatchedSystem = new DefaultEcsBatchedChangeSystem(_defaultWorld, _defaultRunner); // TODO Proper ordering
            _defaultComponentSystem = new DefaultEcsComponentSystem(_defaultWorld, null);
            _defaultComponentBatchedSystem = new DefaultEcsComponentBatchedUpdateSystem(_defaultWorld, null);
            _defaultMultiComponentSystem = new DefaultEcsComponentSystem(_defaultWorld, _defaultRunner);
            _defaultMultiComponentBatchedSystem = new DefaultEcsComponentBatchedUpdateSystem(_defaultWorld, _defaultRunner);// TODO Proper ordering

            _entitasWorld = new Context<EntitasEntity>(2, () => new EntitasEntity());
            _entitasExecuteSystem = new EntitasExecuteSystem(_entitasWorld);
            _entitasSystem = new EntitasSystem(_entitasWorld);
            _entitasMultiSystem = new EntitasSystem(_entitasWorld, Environment.ProcessorCount);

            for (int i = 0; i < EntityCount; ++i)
            {
                DefaultEntity defaultEntity = _defaultWorld.CreateEntity();
                defaultEntity.Set<DefaultPosition>();
                defaultEntity.Set(new DefaultSpeed { X = 42, Y = 42 });

                EntitasEntity entitasEntity = _entitasWorld.CreateEntity();
                entitasEntity.AddComponent(0, new EntitasSpeed { X = 42, Y = 42 });
                entitasEntity.AddComponent(1, new EntitasPosition());
            }
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _defaultRunner.Dispose();
            _defaultWorld.Dispose();
        }

        [Benchmark]
        public void DefaultEcs_EntitySet()
        {
            foreach (ref readonly DefaultEntity entity in _defaultEntitySet.GetEntities())
            {
                DefaultSpeed speed = entity.Get<DefaultSpeed>();
                ref DefaultPosition position = ref entity.Get<DefaultPosition>();

                position.X += speed.X * Time;
                position.Y += speed.Y * Time;

                entity.Set(in position);
            }
        }

        [Benchmark]
        public void DefaultEcs_EntitySet_Batch_Changed()
        {
            foreach (ref readonly DefaultEntity entity in _defaultEntitySet.GetEntities())
            {
                DefaultSpeed speed = entity.Get<DefaultSpeed>();
                ref DefaultPosition position = ref entity.Get<DefaultPosition>();

                position.X += speed.X * Time;
                position.Y += speed.Y * Time;
            }

            foreach (ref readonly DefaultEntity entity in _defaultEntitySet.GetEntities())
            {
                entity.MarkChanged<DefaultPosition>();
            }
        }

        [Benchmark]
        public void DefaultEcs_System() => _defaultSystem.Update(Time);

        [Benchmark]
        public void DefaultEcs_System_Batch_Changed() => _defaultBatchedSystem.Update(Time);

        [Benchmark]
        public void DefaultEcs_MultiSystem() => _defaultMultiSystem.Update(Time);

        [Benchmark]
        public void DefaultEcs_MultiSystem_Batch_Changed() => _defaultMultiBatchedSystem.Update(Time);

        [Benchmark]
        public void DefaultEcs_ComponentSystem() => _defaultComponentSystem.Update(Time);

        [Benchmark]
        public void DefaultEcs_ComponentSystem_Batch_Changed() => _defaultComponentBatchedSystem.Update(Time);

        [Benchmark]
        public void DefaultEcs_ComponentMultiSystem() => _defaultMultiComponentSystem.Update(Time);

        [Benchmark]
        public void DefaultEcs_ComponentMultiSystem_Batch_Changed() => _defaultMultiComponentBatchedSystem.Update(Time);

        [Benchmark]
        public void Entitas_ExecuteSystem() => _entitasExecuteSystem.Execute();

        [Benchmark]
        public void Entitas_System() => _entitasSystem.Execute();

        [Benchmark]
        public void Entitas_MultiSystem() => _entitasMultiSystem.Execute();
    }
}
