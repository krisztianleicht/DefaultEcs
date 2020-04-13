﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using DefaultEcs.Serialization;
using NFluent;
using Xunit;

namespace DefaultEcs.Test.Serialization
{
    public sealed class ISerializerTest
    {
        #region Types

        private struct Int32
        { }

        private struct Test
        {
            [SuppressMessage("Style", "IDE0044:Add readonly modifier")]
            [SuppressMessage("Code Quality", "IDE0052:Remove unread private members")]
            [SuppressMessage("Design", "RCS1169:Make field read-only.")]
            private int _privateField;
            [SuppressMessage("Code Quality", "IDE0052:Remove unread private members")]
            private readonly int _privateReadOnlyField;

            [SuppressMessage("Code Quality", "IDE0052:Remove unread private members")]
            [SuppressMessage("Design", "RCS1170:Use read-only auto-implemented property.")]
            private int PrivateProperty { get; set; }

            [SuppressMessage("Code Quality", "IDE0052:Remove unread private members")]
            private int PrivateReadOnlyProperty { get; }

            public int PublicField;
            public readonly int PublicReadOnlyField;

            public int PublicProperty { get; set; }
            public int PublicReadOnlyProperty { get; }

            public InnerTest Kikoo;

            public Test(int value)
            {
                _privateField = value;
                _privateReadOnlyField = value + 1;
                PrivateProperty = value + 2;
                PrivateReadOnlyProperty = value + 3;
                PublicField = value + 4;
                PublicReadOnlyField = value + 5;
                PublicProperty = value + 6;
                PublicReadOnlyProperty = value + 7;
                Kikoo = new InnerTest { Lol = value + 8, Enum = EnumTest.Lol };
            }
        }

        private struct InnerTest
        {
            public int Lol;
            public EnumTest Enum;
        }

        private enum EnumTest : short
        {
            Kikoo,
            Lol
        }

        private class ClassTest
        {
            public int Id;
            public Test Inner;
            public InnerTest2 Test;

            public override bool Equals(object obj)
            {
                return obj is ClassTest t
                    && Id == t.Id
                    && Inner.Equals(t.Inner)
                    && Test.Equals(t.Test);
            }

            public override int GetHashCode() => Id;
        }

        private struct InnerTest2
        {
            public InnerClass C;

            public InnerTest2(InnerClass c)
            {
                C = c;
            }

            public override bool Equals(object obj)
            {
                return obj is InnerTest2 t
                    && C?.I == t.C?.I;
            }

            public override int GetHashCode() => C?.I ?? 0;
        }

        private class InnerClass
        {
            public int I = 42;
        }

        #endregion

        public static IEnumerable<object[]> SerializerType
        {
            get
            {
                yield return new object[] { typeof(BinarySerializer) };
                yield return new object[] { typeof(TextSerializer) };
            }
        }

        #region Tests

        [Theory]
        [MemberData(nameof(SerializerType))]
        public void Serialize_world_Should_throw_When_stream_is_null(Type serializerType)
        {
            ISerializer serializer = (ISerializer)Activator.CreateInstance(serializerType);

            Check.ThatCode(() => serializer.Serialize(null, default)).Throws<ArgumentNullException>();
        }

        [Theory]
        [MemberData(nameof(SerializerType))]
        public void Serialize_world_Should_throw_When_world_is_null(Type serializerType)
        {
            ISerializer serializer = (ISerializer)Activator.CreateInstance(serializerType);

            using Stream stream = new MemoryStream();

            Check.ThatCode(() => serializer.Serialize(stream, default)).Throws<ArgumentNullException>();
        }

        [Theory]
        [MemberData(nameof(SerializerType))]
        public void Serialize_entities_Should_throw_When_stream_is_null(Type serializerType)
        {
            ISerializer serializer = (ISerializer)Activator.CreateInstance(serializerType);

            Check.ThatCode(() => serializer.Serialize(null, default(IEnumerable<Entity>))).Throws<ArgumentNullException>();
        }

        [Theory]
        [MemberData(nameof(SerializerType))]
        public void Serialize_entities_Should_throw_When_world_is_null(Type serializerType)
        {
            ISerializer serializer = (ISerializer)Activator.CreateInstance(serializerType);

            using Stream stream = new MemoryStream();

            Check.ThatCode(() => serializer.Serialize(stream, default(IEnumerable<Entity>))).Throws<ArgumentNullException>();
        }

        [Theory]
        [MemberData(nameof(SerializerType))]
        public void Deserialize_Should_throw_When_stream_is_null(Type serializerType)
        {
            ISerializer serializer = (ISerializer)Activator.CreateInstance(serializerType);

            Check.ThatCode(() => serializer.Deserialize(null)).Throws<ArgumentNullException>();
        }

        [Theory]
        [MemberData(nameof(SerializerType))]
        public void Deserialize_world_Should_throw_When_stream_is_null(Type serializerType)
        {
            ISerializer serializer = (ISerializer)Activator.CreateInstance(serializerType);

            Check.ThatCode(() => serializer.Deserialize(null, default)).Throws<ArgumentNullException>();
        }

        [Theory]
        [MemberData(nameof(SerializerType))]
        public void Deserialize_world_Should_throw_When_world_is_null(Type serializerType)
        {
            ISerializer serializer = (ISerializer)Activator.CreateInstance(serializerType);

            using Stream stream = new MemoryStream();

            Check.ThatCode(() => serializer.Deserialize(stream, default)).Throws<ArgumentNullException>();
        }

        [Theory]
        [MemberData(nameof(SerializerType))]
        public void Serialize_Should_serialize_World(Type serializerType)
        {
            using World world = new World(42);

            world.SetMaxCapacity<int>(13);
            world.SetMaxCapacity<float>(60);

            Entity[] entities = new[]
            {
                    world.CreateEntity(),
                    world.CreateEntity(),
                    world.CreateEntity()
                };
            entities[0].Set<Int32>();
            entities[0].Set<bool>(true);
            entities[0].Set<sbyte>(13);
            entities[0].Set<byte>(7);
            entities[0].Set<short>(13);
            entities[0].Set<ushort>(7);
            entities[0].Set<int>(13);
            entities[0].Set<uint>(7);
            entities[0].Set<long>(13);
            entities[0].Set<ulong>(7);
            entities[0].Set<char>('c');
            entities[0].Set<decimal>(3.14m);
            entities[0].Set<double>(1337);
            entities[0].Set<float>(-1);
            entities[0].Set<string>("kikoo");
            entities[0].Set(new Test(666));
            entities[0].Set(new ClassTest { Id = 12345, Inner = new Test(66), Test = new InnerTest2() });
            entities[2].Set(new InnerTest { Lol = 313 });
            entities[1].SetSameAs<InnerTest>(entities[2]);
            entities[1].Set(new Test(42));
            entities[2].SetSameAs<Test>(entities[1]);
            entities[2].SetSameAs<bool>(entities[0]);
            entities[2].Disable<bool>();
            entities[2].Set<sbyte>(42);
            entities[2].Disable<sbyte>();

            entities[0].Set<InnerClass>();
            entities[0].Set<IEnumerable<int>>(new int[] { 1, 2, 3 });

            entities[1].Disable();

            entities[0].SetAsParentOf(entities[1]);

            ISerializer serializer = (ISerializer)Activator.CreateInstance(serializerType);

            string filePath = Path.GetRandomFileName();
            try
            {
                using (Stream stream = File.Create(filePath))
                {
                    serializer.Serialize(stream, world);
                }

                World copyWorld;
                using (Stream stream = File.OpenRead(filePath))
                {
                    copyWorld = serializer.Deserialize(stream);
                }

                using (copyWorld)
                {
                    Check.That(copyWorld.MaxCapacity).IsEqualTo(world.MaxCapacity);

                    Entity[] entitiesCopy = copyWorld.ToArray();

                    Check.That(entitiesCopy[0].Has<Int32>());
                    Check.That(entitiesCopy[0].Get<bool>()).IsEqualTo(entities[0].Get<bool>());
                    Check.That(entitiesCopy[0].Get<sbyte>()).IsEqualTo(entities[0].Get<sbyte>());
                    Check.That(entitiesCopy[0].Get<byte>()).IsEqualTo(entities[0].Get<byte>());
                    Check.That(entitiesCopy[0].Get<short>()).IsEqualTo(entities[0].Get<short>());
                    Check.That(entitiesCopy[0].Get<ushort>()).IsEqualTo(entities[0].Get<ushort>());
                    Check.That(entitiesCopy[0].Get<int>()).IsEqualTo(entities[0].Get<int>());
                    Check.That(entitiesCopy[0].Get<uint>()).IsEqualTo(entities[0].Get<uint>());
                    Check.That(entitiesCopy[0].Get<long>()).IsEqualTo(entities[0].Get<long>());
                    Check.That(entitiesCopy[0].Get<ulong>()).IsEqualTo(entities[0].Get<ulong>());
                    Check.That(entitiesCopy[0].Get<char>()).IsEqualTo(entities[0].Get<char>());
                    Check.That(entitiesCopy[0].Get<decimal>()).IsEqualTo(entities[0].Get<decimal>());
                    Check.That(entitiesCopy[0].Get<double>()).IsEqualTo(entities[0].Get<double>());
                    Check.That(entitiesCopy[0].Get<float>()).IsEqualTo(entities[0].Get<float>());
                    Check.That(entitiesCopy[0].Get<string>()).IsEqualTo(entities[0].Get<string>());
                    Check.That(entitiesCopy[0].Get<ClassTest>()).IsEqualTo(entities[0].Get<ClassTest>());

                    Check.That(entities[0].Get<Test>()).IsEqualTo(entitiesCopy[0].Get<Test>());

                    Check.That(entitiesCopy[1].Get<Test>()).IsEqualTo(entities[1].Get<Test>());
                    Check.That(entitiesCopy[1].Get<InnerTest>()).IsEqualTo(entities[1].Get<InnerTest>());

                    Check.That(entitiesCopy[1].Get<Test>()).IsEqualTo(entitiesCopy[2].Get<Test>());
                    Check.That(entitiesCopy[1].Get<InnerTest>()).IsEqualTo(entitiesCopy[2].Get<InnerTest>());

                    Check.That(entitiesCopy[0].Get<InnerClass>()).IsEqualTo(entities[0].Get<InnerClass>());
                    Check.That(entitiesCopy[0].Get<IEnumerable<int>>()).ContainsExactly(entities[0].Get<IEnumerable<int>>());

                    Check.That(entitiesCopy[1].IsEnabled()).IsEqualTo(entities[1].IsEnabled());

                    Check.That(entitiesCopy[2].Get<bool>()).IsEqualTo(entities[2].Get<bool>());
                    Check.That(entitiesCopy[2].IsEnabled<bool>()).IsFalse();
                    Check.That(entitiesCopy[2].Get<sbyte>()).IsEqualTo(entities[2].Get<sbyte>());
                    Check.That(entitiesCopy[2].IsEnabled<sbyte>()).IsFalse();

                    entitiesCopy[0].Dispose();

                    Check.That(copyWorld.Count()).IsEqualTo(1);
                }
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [Theory]
        [MemberData(nameof(SerializerType))]
        public void Serialize_Should_serialize_Entities(Type serializerType)
        {
            using World world = new World(42);

            world.SetMaxCapacity<int>(13);
            world.SetMaxCapacity<float>(60);

            Entity[] entities = new[]
            {
                    world.CreateEntity(),
                    world.CreateEntity(),
                    world.CreateEntity()
                };
            entities[0].Set<Int32>();
            entities[0].Set<bool>(true);
            entities[0].Set<sbyte>(13);
            entities[0].Set<byte>(7);
            entities[0].Set<short>(13);
            entities[0].Set<ushort>(7);
            entities[0].Set<int>(13);
            entities[0].Set<uint>(7);
            entities[0].Set<long>(13);
            entities[0].Set<ulong>(7);
            entities[0].Set<char>('c');
            entities[0].Set<decimal>(3.14m);
            entities[0].Set<double>(1337);
            entities[0].Set<float>(-1);
            entities[0].Set<string>("kikoo");
            entities[0].Set(new Test(666));
            entities[0].Set(new ClassTest { Id = 12345, Inner = new Test(66), Test = new InnerTest2() });
            entities[2].Set(new InnerTest { Lol = 313 });
            entities[1].SetSameAs<InnerTest>(entities[2]);
            entities[1].Set(new Test(42));
            entities[2].SetSameAs<Test>(entities[1]);
            entities[2].SetSameAs<bool>(entities[0]);
            entities[2].Disable<bool>();
            entities[2].Set<sbyte>(42);
            entities[2].Disable<sbyte>();

            entities[0].Set<InnerClass>();
            entities[0].Set<IEnumerable<int>>(new int[] { 1, 2, 3 });

            entities[1].Disable();

            entities[0].SetAsParentOf(entities[1]);

            ISerializer serializer = (ISerializer)Activator.CreateInstance(serializerType);

            string filePath = Path.GetRandomFileName();
            try
            {
                using (Stream stream = File.Create(filePath))
                {
                    serializer.Serialize(stream, entities[0], entities[1], entities[2]);
                }

                using World copyWorld = new World(42);

                Entity[] entitiesCopy;

                using (Stream stream = File.OpenRead(filePath))
                {
                    entitiesCopy = serializer.Deserialize(stream, copyWorld).ToArray();
                }

                Check.That(entitiesCopy[0].Has<Int32>());
                Check.That(entitiesCopy[0].Get<bool>()).IsEqualTo(entities[0].Get<bool>());
                Check.That(entitiesCopy[0].Get<sbyte>()).IsEqualTo(entities[0].Get<sbyte>());
                Check.That(entitiesCopy[0].Get<byte>()).IsEqualTo(entities[0].Get<byte>());
                Check.That(entitiesCopy[0].Get<short>()).IsEqualTo(entities[0].Get<short>());
                Check.That(entitiesCopy[0].Get<ushort>()).IsEqualTo(entities[0].Get<ushort>());
                Check.That(entitiesCopy[0].Get<int>()).IsEqualTo(entities[0].Get<int>());
                Check.That(entitiesCopy[0].Get<uint>()).IsEqualTo(entities[0].Get<uint>());
                Check.That(entitiesCopy[0].Get<long>()).IsEqualTo(entities[0].Get<long>());
                Check.That(entitiesCopy[0].Get<ulong>()).IsEqualTo(entities[0].Get<ulong>());
                Check.That(entitiesCopy[0].Get<char>()).IsEqualTo(entities[0].Get<char>());
                Check.That(entitiesCopy[0].Get<decimal>()).IsEqualTo(entities[0].Get<decimal>());
                Check.That(entitiesCopy[0].Get<double>()).IsEqualTo(entities[0].Get<double>());
                Check.That(entitiesCopy[0].Get<float>()).IsEqualTo(entities[0].Get<float>());
                Check.That(entitiesCopy[0].Get<string>()).IsEqualTo(entities[0].Get<string>());
                Check.That(entitiesCopy[0].Get<ClassTest>()).IsEqualTo(entities[0].Get<ClassTest>());

                Check.That(entities[0].Get<Test>()).IsEqualTo(entitiesCopy[0].Get<Test>());

                Check.That(entitiesCopy[1].Get<Test>()).IsEqualTo(entities[1].Get<Test>());
                Check.That(entitiesCopy[1].Get<InnerTest>()).IsEqualTo(entities[1].Get<InnerTest>());

                Check.That(entitiesCopy[1].Get<Test>()).IsEqualTo(entitiesCopy[2].Get<Test>());
                Check.That(entitiesCopy[1].Get<InnerTest>()).IsEqualTo(entitiesCopy[2].Get<InnerTest>());

                Check.That(entitiesCopy[0].Get<InnerClass>()).IsEqualTo(entities[0].Get<InnerClass>());
                Check.That(entitiesCopy[0].Get<IEnumerable<int>>()).ContainsExactly(entities[0].Get<IEnumerable<int>>());

                Check.That(entitiesCopy[1].IsEnabled()).IsEqualTo(entities[1].IsEnabled());

                Check.That(entitiesCopy[2].Get<bool>()).IsEqualTo(entities[2].Get<bool>());
                Check.That(entitiesCopy[2].IsEnabled<bool>()).IsFalse();
                Check.That(entitiesCopy[2].Get<sbyte>()).IsEqualTo(entities[2].Get<sbyte>());
                Check.That(entitiesCopy[2].IsEnabled<sbyte>()).IsFalse();

                entitiesCopy[0].Dispose();

                Check.That(copyWorld.Count()).IsEqualTo(1);
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        #endregion
    }
}
