﻿using System;
using System.Collections.Generic;
using NFluent;
using Xunit;

namespace DefaultEcs.Test
{
    public sealed class EntityRuleBuilderTest
    {
        #region Tests

        [Fact]
        public void AsSet_Should_return_EntitySet_with_all_Entity()
        {
            using World world = new World(4);
            using EntitySet set = world.GetEntities().AsSet();

            List<Entity> entities = new List<Entity>
                {
                    world.CreateEntity(),
                    world.CreateEntity(),
                    world.CreateEntity(),
                    world.CreateEntity()
                };

            Check.That(set.GetEntities().ToArray()).ContainsExactly(entities);

            entities[2].Dispose();
            entities.RemoveAt(2);

            Check.That(set.GetEntities().ToArray()).ContainsExactly(entities);
        }

        [Fact]
        public void AsSet_With_T_Should_return_EntitySet_with_all_Entity_with_component_T()
        {
            using World world = new World(4);
            using EntitySet set = world.GetEntities().With<bool>().AsSet();

            List<Entity> entities = new List<Entity>
                {
                    world.CreateEntity(),
                    world.CreateEntity(),
                    world.CreateEntity(),
                    world.CreateEntity()
                };

            Check.That(set.GetEntities().ToArray()).IsEmpty();

            foreach (Entity entity in entities)
            {
                entity.Set(true);
            }

            Check.That(set.GetEntities().ToArray()).ContainsExactly(entities);

            entities[2].Remove<bool>();
            entities.RemoveAt(2);

            Check.That(set.GetEntities().ToArray()).ContainsExactly(entities);

            Entity temp = entities[2];
            temp.Disable<bool>();
            entities.Remove(temp);

            Check.That(set.GetEntities().ToArray()).ContainsExactly(entities);
            temp.Enable<bool>();
            entities.Add(temp);

            Check.That(set.GetEntities().ToArray()).ContainsExactly(entities);
        }

        [Fact]
        public void AsSet_With_T1_T2_Should_return_EntitySet_with_all_Entity_with_component_T1_T2()
        {
            using World world = new World(4);
            using EntitySet set = world.GetEntities().With<bool>().With<int>().AsSet();

            List<Entity> entities = new List<Entity>
                {
                    world.CreateEntity(),
                    world.CreateEntity(),
                    world.CreateEntity(),
                    world.CreateEntity()
                };

            Check.That(set.GetEntities().ToArray()).IsEmpty();

            foreach (Entity entity in entities)
            {
                entity.Set(true);
            }

            Check.That(set.GetEntities().ToArray()).IsEmpty();

            foreach (Entity entity in entities)
            {
                entity.Set(42);
            }

            Check.That(set.GetEntities().ToArray()).ContainsExactly(entities);

            Entity temp = entities[2];
            temp.Remove<bool>();
            entities.Remove(temp);

            Check.That(set.GetEntities().ToArray()).ContainsExactly(entities);

            temp.Set(true);
            temp.Remove<int>();

            Check.That(set.GetEntities().ToArray()).ContainsExactly(entities);
        }

        [Fact]
        public void AsSet_WithEither_T1_T2_Should_return_EntitySet_with_all_Entity_with_component_T1_or_T2()
        {
            using World world = new World(4);
            using EntitySet set = world.GetEntities().WithEither<bool>().Or<int>().AsSet();

            List<Entity> entities = new List<Entity>
                {
                    world.CreateEntity(),
                    world.CreateEntity(),
                    world.CreateEntity(),
                    world.CreateEntity()
                };

            Check.That(set.GetEntities().ToArray()).IsEmpty();

            foreach (Entity entity in entities)
            {
                entity.Set(true);
            }

            Check.That(set.GetEntities().ToArray()).ContainsExactly(entities);

            foreach (Entity entity in entities)
            {
                entity.Set(42);
            }

            Check.That(set.GetEntities().ToArray()).ContainsExactly(entities);

            Entity temp = entities[2];
            temp.Remove<bool>();

            Check.That(set.GetEntities().ToArray()).ContainsExactly(entities);

            temp.Remove<int>();
            entities.Remove(temp);

            Check.That(set.GetEntities().ToArray()).ContainsExactly(entities);
        }

        [Fact]
        public void AsSet_Without_T_Should_return_EntitySet_with_all_Entity_without_component_T()
        {
            using World world = new World(4);
            using EntitySet set = world.GetEntities().Without<int>().AsSet();

            List<Entity> entities = new List<Entity>
                {
                    world.CreateEntity(),
                    world.CreateEntity(),
                    world.CreateEntity(),
                    world.CreateEntity()
                };

            Check.That(set.GetEntities().ToArray()).ContainsExactly(entities);

            foreach (Entity entity in entities)
            {
                entity.Set(42);
            }

            Check.That(set.GetEntities().ToArray()).IsEmpty();

            Entity temp = entities[2];
            temp.Disable<int>();

            Check.That(set.GetEntities().ToArray()).ContainsExactly(temp);

            temp.Enable<int>();

            Check.That(set.GetEntities().ToArray()).IsEmpty();

            temp.Remove<int>();

            Check.That(set.GetEntities().ToArray()).ContainsExactly(temp);
        }

        [Fact]
        public void AsSet_WithoutEither_T1_T2_Should_return_EntitySet_with_all_Entity_without_component_T1_or_T2()
        {
            using World world = new World(4);
            using EntitySet set = world.GetEntities().WithoutEither<bool>().Or<int>().AsSet();

            List<Entity> entities = new List<Entity>
                {
                    world.CreateEntity(),
                    world.CreateEntity(),
                    world.CreateEntity(),
                    world.CreateEntity()
                };

            Check.That(set.GetEntities().ToArray()).ContainsExactly(entities);

            foreach (Entity entity in entities)
            {
                entity.Set(true);
            }

            Check.That(set.GetEntities().ToArray()).ContainsExactly(entities);

            foreach (Entity entity in entities)
            {
                entity.Set(42);
            }

            Check.That(set.Count).IsZero();

            foreach (Entity entity in entities)
            {
                entity.Remove<bool>();
            }

            Check.That(set.GetEntities().ToArray()).ContainsExactly(entities);
        }

        [Fact]
        public void AsSet_WhenAdded_T_Should_return_EntitySet_with_all_Entity_when_component_T_is_added()
        {
            using World world = new World(4);
            using EntitySet set = world.GetEntities().WhenAdded<bool>().AsSet();

            List<Entity> entities = new List<Entity>
                {
                    world.CreateEntity(),
                    world.CreateEntity(),
                    world.CreateEntity(),
                    world.CreateEntity()
                };

            Check.That(set.Count).IsZero();

            foreach (Entity entity in entities)
            {
                entity.Set(true);
            }

            Check.That(set.GetEntities().ToArray()).ContainsExactly(entities);

            set.Complete();

            Check.That(set.Count).IsZero();

            foreach (Entity entity in entities)
            {
                entity.Set(false);
            }

            Check.That(set.Count).IsZero();

            foreach (Entity entity in entities)
            {
                entity.Disable<bool>();
            }

            Check.That(set.Count).IsZero();

            foreach (Entity entity in entities)
            {
                entity.Enable<bool>();
            }

            Check.That(set.GetEntities().ToArray()).ContainsExactly(entities);
        }

        [Fact]
        public void AsSet_WhenAddedEither_T1_T2_Should_return_EntitySet_with_all_Entity_when_component_T1_or_T2_is_added()
        {
            using World world = new World(4);
            using EntitySet set = world.GetEntities().WhenAddedEither<bool>().Or<int>().AsSet();

            List<Entity> entities = new List<Entity>
                {
                    world.CreateEntity(),
                    world.CreateEntity(),
                    world.CreateEntity(),
                    world.CreateEntity()
                };

            Check.That(set.Count).IsZero();

            foreach (Entity entity in entities)
            {
                entity.Set(true);
            }

            Check.That(set.GetEntities().ToArray()).ContainsExactly(entities);

            set.Complete();

            foreach (Entity entity in entities)
            {
                entity.Set(false);
            }

            Check.That(set.Count).IsZero();

            foreach (Entity entity in entities)
            {
                entity.Set(42);
            }

            Check.That(set.GetEntities().ToArray()).ContainsExactly(entities);
        }

        [Fact]
        public void AsSet_WhenChanged_T_Should_return_EntitySet_with_all_Entity_when_component_T_is_added_and_changed()
        {
            using World world = new World(4);
            using EntitySet set = world.GetEntities().WhenChanged<bool>().AsSet();

            List<Entity> entities = new List<Entity>
                {
                    world.CreateEntity(),
                    world.CreateEntity(),
                    world.CreateEntity(),
                    world.CreateEntity()
                };

            Check.That(set.Count).IsZero();

            foreach (Entity entity in entities)
            {
                entity.Set(true);
            }

            Check.That(set.Count).IsZero();

            set.Complete();

            Check.That(set.Count).IsZero();

            foreach (Entity entity in entities)
            {
                entity.Set(false);
            }

            Check.That(set.GetEntities().ToArray()).ContainsExactly(entities);
        }

        [Fact]
        public void AsSet_WhenChangedEither_T1_T2_Should_return_EntitySet_with_all_Entity_when_component_T1_or_T2_is_changed()
        {
            using World world = new World(4);
            using EntitySet set = world.GetEntities().WhenChangedEither<bool>().Or<int>().AsSet();

            List<Entity> entities = new List<Entity>
                {
                    world.CreateEntity(),
                    world.CreateEntity(),
                    world.CreateEntity(),
                    world.CreateEntity()
                };

            Check.That(set.Count).IsZero();

            foreach (Entity entity in entities)
            {
                entity.Set(true);
                entity.Set(42);
            }

            Check.That(set.Count).IsZero();

            foreach (Entity entity in entities)
            {
                entity.Set(false);
            }

            Check.That(set.GetEntities().ToArray()).ContainsExactly(entities);
            set.Complete();
            Check.That(set.Count).IsZero();

            foreach (Entity entity in entities)
            {
                entity.Set(1337);
            }

            Check.That(set.GetEntities().ToArray()).ContainsExactly(entities);
        }

        [Fact]
        public void AsSet_WhenRemoved_T_Should_return_EntitySet_with_all_Entity_when_component_T_is_removed()
        {
            using World world = new World(4);
            using EntitySet set = world.GetEntities().WhenRemoved<bool>().AsSet();

            List<Entity> entities = new List<Entity>
                {
                    world.CreateEntity(),
                    world.CreateEntity(),
                    world.CreateEntity(),
                    world.CreateEntity()
                };

            Check.That(set.Count).IsZero();

            foreach (Entity entity in entities)
            {
                entity.Set(true);
            }

            Check.That(set.Count).IsZero();

            foreach (Entity entity in entities)
            {
                entity.Disable<bool>();
            }

            Check.That(set.GetEntities().ToArray()).ContainsExactly(entities);

            foreach (Entity entity in entities)
            {
                entity.Enable<bool>();
            }

            Check.That(set.Count).IsZero();

            foreach (Entity entity in entities)
            {
                entity.Remove<bool>();
            }

            Check.That(set.GetEntities().ToArray()).ContainsExactly(entities);
        }

        [Fact]
        public void AsSet_WhenRemovedEither_T1_T2_Should_return_EntitySet_with_all_Entity_when_component_T1_or_T2_is_changed()
        {
            using World world = new World(4);
            using EntitySet set = world.GetEntities().WhenRemovedEither<bool>().Or<int>().AsSet();

            List<Entity> entities = new List<Entity>
                {
                    world.CreateEntity(),
                    world.CreateEntity(),
                    world.CreateEntity(),
                    world.CreateEntity()
                };

            Check.That(set.Count).IsZero();

            foreach (Entity entity in entities)
            {
                entity.Set(true);
                entity.Set(42);
            }

            Check.That(set.Count).IsZero();

            foreach (Entity entity in entities)
            {
                entity.Remove<bool>();
            }

            Check.That(set.GetEntities().ToArray()).ContainsExactly(entities);
            set.Complete();
            Check.That(set.Count).IsZero();

            foreach (Entity entity in entities)
            {
                entity.Remove<int>();
            }

            Check.That(set.GetEntities().ToArray()).ContainsExactly(entities);
        }

        [Fact]
        public void AsPredicate_With_T_Should_return_true_When_entity_has_component_T()
        {
            using World world = new World(4);

            Entity entity = world.CreateEntity();

            Predicate<Entity> predicate = world.GetEntities().With<bool>().AsPredicate();

            Check.That(predicate(entity)).IsFalse();

            entity.Set(true);

            Check.That(predicate(entity)).IsTrue();

            entity.Disable<bool>();

            Check.That(predicate(entity)).IsFalse();

            entity.Enable<bool>();

            Check.That(predicate(entity)).IsTrue();

            entity.Remove<bool>();

            Check.That(predicate(entity)).IsFalse();
        }

        [Fact]
        public void AsPredicate_WithEither_T1_T2_Should_return_true_When_entity_has_component_T1()
        {
            using World world = new World(4);

            Entity entity = world.CreateEntity();

            Predicate<Entity> predicate = world.GetEntities().WithEither<bool>().Or<int>().AsPredicate();

            Check.That(predicate(entity)).IsFalse();

            entity.Set(true);

            Check.That(predicate(entity)).IsTrue();

            entity.Disable<bool>();

            Check.That(predicate(entity)).IsFalse();

            entity.Enable<bool>();

            Check.That(predicate(entity)).IsTrue();

            entity.Remove<bool>();

            Check.That(predicate(entity)).IsFalse();

            entity.Set(42);

            Check.That(predicate(entity)).IsTrue();
        }

        #endregion
    }
}
