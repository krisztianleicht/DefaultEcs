﻿using System.Linq;
using NFluent;
using Xunit;

namespace DefaultEcs.Test
{
    public sealed class EntityMapTest
    {
        [Fact]
        public void World_Should_return_world()
        {
            using World world = new World();

            using EntityMap<int> map = world.GetEntities().AsMap<int>();

            Check.That(map.World).IsEqualTo(world);
        }

        [Fact]
        public void ContainsEntity_Should_return_weither_an_entity_is_in_or_not()
        {
            using World world = new World();

            Entity entity = world.CreateEntity();
            entity.Set(42);

            using EntityMap<int> map = world.GetEntities().AsMap<int>();

            Check.That(map.ContainsEntity(entity)).IsTrue();

            entity.Disable<int>();

            Check.That(map.ContainsEntity(entity)).IsFalse();

            entity.Enable<int>();

            Check.That(map.ContainsEntity(entity)).IsTrue();

            entity.Remove<int>();

            Check.That(map.ContainsEntity(entity)).IsFalse();
        }

        [Fact]
        public void ContainsKey_Should_return_weither_a_key_is_in_or_not()
        {
            using World world = new World();

            using EntityMap<int> map = world.GetEntities().AsMap<int>();

            Entity entity = world.CreateEntity();

            Check.That(map.ContainsKey(42)).IsFalse();

            entity.Set(42);

            Check.That(map.ContainsKey(42)).IsTrue();

            entity.Disable<int>();

            Check.That(map.ContainsKey(42)).IsFalse();

            entity.Enable<int>();

            Check.That(map.ContainsKey(42)).IsTrue();

            entity.Remove<int>();

            Check.That(map.ContainsKey(42)).IsFalse();
        }

        [Fact]
        public void This_Should_return_entity()
        {
            using World world = new World();

            using EntityMap<int> map = world.GetEntities().AsMap<int>();

            Entity entity = world.CreateEntity();
            entity.Set(42);

            Check.That(map[42]).IsEqualTo(entity);
        }

        [Fact]
        public void Keys_Should_return_keys()
        {
            using World world = new World();

            using EntityMap<int> map = world.GetEntities().AsMap<int>();

            Entity entity = world.CreateEntity();
            entity.Set(42);

            Check.That(map.Keys.AsEnumerable()).ContainsExactly(42);

            entity.Remove<int>();

            Check.That(map.Keys).IsEmpty();
        }

        [Fact]
        public void TryGetEntity_Should_return_weither_a_key_is_in_or_not()
        {
            using World world = new World();

            using EntityMap<int> map = world.GetEntities().AsMap<int>();

            Entity entity = world.CreateEntity();

            Check.That(map.TryGetEntity(42, out Entity result)).IsFalse();

            entity.Set(42);

            Check.That(map.TryGetEntity(42, out result)).IsTrue();
            Check.That(result).IsEqualTo(entity);

            entity.Disable<int>();

            Check.That(map.TryGetEntity(42, out result)).IsFalse();

            entity.Enable<int>();

            Check.That(map.TryGetEntity(42, out result)).IsTrue();
            Check.That(result).IsEqualTo(entity);

            entity.Remove<int>();

            Check.That(map.TryGetEntity(42, out result)).IsFalse();
        }

        [Fact]
        public void Should_behave_correctly_when_key_changed()
        {
            using World world = new World();

            using EntityMap<int> map = world.GetEntities().AsMap<int>();

            Entity entity = world.CreateEntity();
            entity.Set(42);

            Check.That(map.TryGetEntity(42, out Entity result)).IsTrue();
            Check.That(result).IsEqualTo(entity);

            entity.Set(1337);

            Check.That(map.TryGetEntity(42, out result)).IsFalse();
            Check.That(map.TryGetEntity(1337, out result)).IsTrue();
            Check.That(result).IsEqualTo(entity);

            entity.Get<int>() = 42;
            entity.NotifyChanged<int>();

            Check.That(map.TryGetEntity(1337, out result)).IsFalse();
            Check.That(map.TryGetEntity(42, out result)).IsTrue();
            Check.That(result).IsEqualTo(entity);
        }

        [Fact]
        public void Complete_Should_empty_When_reative()
        {
            using World world = new World();

            using EntityMap<int> map = world.GetEntities().WhenAddedEither<int>().AsMap<int>();

            Entity entity = world.CreateEntity();
            entity.Set(42);

            Check.That(map.TryGetEntity(42, out Entity result)).IsTrue();
            Check.That(result).IsEqualTo(entity);

            map.Complete();

            Check.That(map.TryGetEntity(42, out result)).IsFalse();

            entity.Set(1337);

            Check.That(map.TryGetEntity(42, out result)).IsFalse();
        }
    }
}
