﻿using System;
using DefaultEcs.System;
using NFluent;
using Xunit;

namespace DefaultEcs.Test.System
{
    public sealed class ComponentAttributeTest
    {
        [With(typeof(bool))]
        private sealed class WithSystem : AEntitySystem<int>
        {
            public WithSystem(World world)
                : base(world)
            { }

            protected override void Update(int state, in Entity entity) => entity.Get<bool>() = true;
        }

        private sealed class WithPredicateSystem : AEntitySystem<int>
        {
            public WithPredicateSystem(World world)
                : base(world)
            { }

#pragma warning disable IDE0051 // Remove unused private members
            [WithPredicate]
            private static bool boolPredicate(in bool _) => true;

            [WithPredicate]
            private bool intPredicate(in int value) => value == 42;
#pragma warning restore IDE0051 // Remove unused private members

            protected override void Update(int state, in Entity entity) => entity.Get<bool>() = true;
        }

        private sealed class InvalidWithPredicateSystem : AEntitySystem<int>
        {
            public InvalidWithPredicateSystem(World world)
                : base(world)
            { }

#pragma warning disable IDE0051 // Remove unused private members
            [WithPredicate]
            private static void Predicate(in bool _)
            { }
#pragma warning restore IDE0051 // Remove unused private members

            protected override void Update(int state, in Entity entity) => entity.Get<bool>() = true;
        }

        [WithEither(typeof(bool), typeof(int))]
        [WithEither(typeof(bool), typeof(string))]
        private sealed class WithEitherSystem : AEntitySystem<int>
        {
            public WithEitherSystem(World world)
                : base(world)
            { }

            protected override void Update(int state, in Entity entity) => entity.Get<bool>() = true;
        }

        [With(typeof(bool))]
        [Without(typeof(string))]
        private sealed class WithoutSystem : AEntitySystem<int>
        {
            public WithoutSystem(World world)
                : base(world)
            { }

            protected override void Update(int state, in Entity entity) => entity.Get<bool>() = true;
        }

        [With(typeof(bool))]
        [WithoutEither(typeof(string), typeof(char))]
        [WithoutEither(typeof(double), typeof(long))]
        private sealed class WithoutEitherSystem : AEntitySystem<int>
        {
            public WithoutEitherSystem(World world)
                : base(world)
            { }

            protected override void Update(int state, in Entity entity) => entity.Get<bool>() = true;
        }

        [WhenAdded(typeof(bool))]
        private sealed class WhenAddedSystem : AEntitySystem<int>
        {
            public WhenAddedSystem(World world)
                : base(world)
            { }

            protected override void Update(int state, in Entity entity) => entity.Get<bool>() = true;
        }

        [WhenAddedEither(typeof(bool), typeof(int))]
        private sealed class WhenAddedEitherSystem : AEntitySystem<int>
        {
            public WhenAddedEitherSystem(World world)
                : base(world)
            { }

            protected override void Update(int state, in Entity entity) => entity.Get<bool>() = true;
        }

        [WhenChanged(typeof(bool))]
        private sealed class WhenChangedSystem : AEntitySystem<int>
        {
            public WhenChangedSystem(World world)
                : base(world)
            { }

            protected override void Update(int state, in Entity entity) => entity.Get<bool>() = true;
        }

        [WhenChangedEither(typeof(bool), typeof(int))]
        private sealed class WhenChangedEitherSystem : AEntitySystem<int>
        {
            public WhenChangedEitherSystem(World world)
                : base(world)
            { }

            protected override void Update(int state, in Entity entity) => entity.Get<bool>() = true;
        }

        [With(typeof(bool))]
        [WhenRemoved(typeof(string))]
        private sealed class WhenRemovedSystem : AEntitySystem<int>
        {
            public WhenRemovedSystem(World world)
                : base(world)
            { }

            protected override void Update(int state, in Entity entity) => entity.Get<bool>() = true;
        }

        [With(typeof(bool))]
        [WhenRemovedEither(typeof(string), typeof(char))]
        private sealed class WhenRemovedEitherSystem : AEntitySystem<int>
        {
            public WhenRemovedEitherSystem(World world)
                : base(world)
            { }

            protected override void Update(int state, in Entity entity) => entity.Get<bool>() = true;
        }

        #region Tests

        [Fact]
        public void ComponentAttribute_Should_throw_When_componentTypes_is_null()
        {
            Check.ThatCode(() => new ComponentAttribute(ComponentFilterType.With, default)).Throws<ArgumentNullException>();
        }

        [Fact]
        public void WithAttribute_Should_create_correct_EntitySet()
        {
            using World world = new World();
            using ISystem<int> system = new WithSystem(world);

            Entity entity = world.CreateEntity();
            entity.Set<bool>();

            system.Update(0);

            Check.That(entity.Get<bool>()).IsTrue();
        }

        [Fact]
        public void WithPredicateAttribute_Should_create_correct_EntitySet()
        {
            using World world = new World();
            using ISystem<int> system = new WithPredicateSystem(world);

            Entity entity = world.CreateEntity();
            entity.Set<bool>();
            entity.Set(1337);

            system.Update(0);

            Check.That(entity.Get<bool>()).IsFalse();

            entity.Set(42);

            system.Update(0);

            Check.That(entity.Get<bool>()).IsTrue();
        }

        [Fact]
        public void WithPredicateAttribute_Should_throw_When_incorrect_method()
        {
            using World world = new World();

            Check.ThatCode(() => new InvalidWithPredicateSystem(world)).Throws<NotSupportedException>();
        }

        [Fact]
        public void WithEitherAttribute_Should_create_correct_EntitySet()
        {
            using World world = new World();
            using ISystem<int> system = new WithEitherSystem(world);

            Entity entity = world.CreateEntity();
            entity.Set<bool>();

            system.Update(0);

            Check.That(entity.Get<bool>()).IsTrue();
        }

        [Fact]
        public void WithoutAttribute_Should_create_correct_EntitySet()
        {
            using World world = new World();
            using ISystem<int> system = new WithoutSystem(world);

            Entity entity = world.CreateEntity();
            entity.Set<bool>();

            system.Update(0);

            Check.That(entity.Get<bool>()).IsTrue();

            entity.Set<bool>();
            entity.Set<string>();

            system.Update(0);

            Check.That(entity.Get<bool>()).IsFalse();
        }

        [Fact]
        public void WithoutEitherAttribute_Should_create_correct_EntitySet()
        {
            using World world = new World();
            using ISystem<int> system = new WithoutEitherSystem(world);

            Entity entity = world.CreateEntity();
            entity.Set<bool>();
            entity.Set<char>();

            system.Update(0);

            Check.That(entity.Get<bool>()).IsTrue();

            entity.Set<bool>();
            entity.Set<string>();

            system.Update(0);

            Check.That(entity.Get<bool>()).IsFalse();
        }

        [Fact]
        public void WhenAddedAttribute_Should_create_correct_EntitySet()
        {
            using World world = new World();
            using ISystem<int> system = new WhenAddedSystem(world);

            Entity entity = world.CreateEntity();
            entity.Set<bool>();

            system.Update(0);

            Check.That(entity.Get<bool>()).IsTrue();
        }

        [Fact]
        public void WhenAddedEitherAttribute_Should_create_correct_EntitySet()
        {
            using World world = new World();
            using ISystem<int> system = new WhenAddedEitherSystem(world);

            Entity entity = world.CreateEntity();
            entity.Set<bool>();

            system.Update(0);

            Check.That(entity.Get<bool>()).IsTrue();

            entity.Set<bool>();

            system.Update(0);

            Check.That(entity.Get<bool>()).IsFalse();

            entity.Set<int>();

            system.Update(0);

            Check.That(entity.Get<bool>()).IsTrue();
        }

        [Fact]
        public void WhenChangedAttribute_Should_create_correct_EntitySet()
        {
            using World world = new World();
            using ISystem<int> system = new WhenChangedSystem(world);

            Entity entity = world.CreateEntity();
            entity.Set<bool>();

            system.Update(0);

            Check.That(entity.Get<bool>()).IsFalse();

            entity.Set<bool>();

            system.Update(0);

            Check.That(entity.Get<bool>()).IsTrue();
        }

        [Fact]
        public void WhenChangedEitherAttribute_Should_create_correct_EntitySet()
        {
            using World world = new World();
            using ISystem<int> system = new WhenChangedEitherSystem(world);

            Entity entity = world.CreateEntity();
            entity.Set<bool>();
            entity.Set<int>();

            system.Update(0);

            Check.That(entity.Get<bool>()).IsFalse();

            entity.Set<bool>();

            system.Update(0);

            Check.That(entity.Get<bool>()).IsTrue();

            entity.Get<bool>() = false;
            entity.Set<int>();

            system.Update(0);

            Check.That(entity.Get<bool>()).IsTrue();
        }

        [Fact]
        public void WhenRemovedAttribute_Should_create_correct_EntitySet()
        {
            using World world = new World();
            using ISystem<int> system = new WhenRemovedSystem(world);

            Entity entity = world.CreateEntity();
            entity.Set<bool>();
            entity.Set<string>();

            system.Update(0);

            Check.That(entity.Get<bool>()).IsFalse();

            entity.Remove<string>();

            system.Update(0);

            Check.That(entity.Get<bool>()).IsTrue();

            entity.Get<bool>() = false;

            system.Update(0);

            Check.That(entity.Get<bool>()).IsFalse();
        }

        [Fact]
        public void WhenRemovedEitherAttribute_Should_create_correct_EntitySet()
        {
            using World world = new World();
            using ISystem<int> system = new WhenRemovedEitherSystem(world);

            Entity entity = world.CreateEntity();
            entity.Set<bool>();
            entity.Set<string>();
            entity.Set<char>();

            system.Update(0);

            Check.That(entity.Get<bool>()).IsFalse();

            entity.Remove<string>();

            system.Update(0);

            Check.That(entity.Get<bool>()).IsTrue();

            entity.Set<bool>();

            system.Update(0);

            Check.That(entity.Get<bool>()).IsFalse();

            entity.Remove<char>();

            system.Update(0);

            Check.That(entity.Get<bool>()).IsTrue();
        }

        #endregion
    }
}
