﻿using System;
using DefaultEcs;
using DefaultEcs.System;
using DefaultSlap.Component;
using DefaultSlap.Message;
using Microsoft.Xna.Framework;

namespace DefaultSlap.System
{
    public class GameSystem : ISystem<float>
    {
        private readonly World _world;
        private readonly EntitySet _bugsSet;
        private readonly Random _random;
        private readonly Func<Vector2>[] _defaultPositions;

        private int _score;
        private float _timeBeforeNextSpawn;
        private int _life;

        public GameSystem(World world)
        {
            _world = world;
            _bugsSet = _world.GetEntities().With<Bug>().AsSet();
            _random = new Random();
            _defaultPositions = new Func<Vector2>[]
            {
                () => new Vector2(-10, _random.Next(0, 600)),
                () => new Vector2(810, _random.Next(0, 600)),
                () => new Vector2(_random.Next(0, 800), -10),
                () => new Vector2(_random.Next(0, 800), 610)
            };

            _world.Subscribe(this);
        }

        private void Init()
        {
            Span<Entity> bugs = stackalloc Entity[_bugsSet.Count];
            _bugsSet.GetEntities().CopyTo(bugs);
            foreach (ref readonly Entity bug in bugs)
            {
                bug.Dispose();
            }

            _life = 3;
            _timeBeforeNextSpawn = 0f;
            _score = 0;
        }

        [Subscribe]
        private void On(in PlayerHitMessage _) => --_life;

        [Subscribe]
        private void On(in SlapMessage message)
        {
            Span<Entity> bugs = stackalloc Entity[_bugsSet.Count];
            _bugsSet.GetEntities().CopyTo(bugs);
            foreach (ref readonly Entity bug in bugs)
            {
                Point position = bug.Get<Position>().Value;
                Point size = bug.Get<DrawInfo>().Size;
                if (message.DeathZone.Intersects(new Rectangle(position - (size / new Point(2)), size)))
                {
                    ++_score;
                    bug.Dispose();
                }
            }
        }

        public bool IsEnabled { get; set; } = true;

        public void Update(float state)
        {
            if (_life <= 0)
            {
                Init();
            }

            if ((_timeBeforeNextSpawn -= state) < 0f)
            {
                _timeBeforeNextSpawn = 5f;

                int bugPerSpawn = 1 + (_score / 3);
                while (--bugPerSpawn >= 0)
                {
                    Entity bug = _world.CreateEntity();
                    bug.Set<Bug>(default);
                    bug.Set(new Speed(100f + _score));
                    bug.Set(new PositionFloat(_defaultPositions[_random.Next(0, 3)]()));
                    bug.Set<Position>(default);
                    bug.Set(new TargetPosition(Point.Zero));
                    bug.Set(new DrawInfo(new Point(10, 10), Color.Blue));
                    bug.Set(new HitDelay(5f));
                }
            }
        }

        public void Dispose()
        {
            _bugsSet.Dispose();
        }
    }
}
