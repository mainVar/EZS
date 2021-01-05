﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Wargon.ezs
{
    public class Systems
    {
        public int id;
        private World world;
        private GrowList<IUpdateSystem> updateSystemsList = new GrowList<IUpdateSystem>(16);
        private TypeMap<int, List<IOnAdd>> OnAddSystems = new TypeMap<int, List<IOnAdd>>(4);
        private TypeMap<int, List<IOnRemove>> OnRemoveSystems = new TypeMap<int, List<IOnRemove>>(4);
        private int updateSystemsCount;
        public bool Alive;
        public Systems(World world)
        {
            this.world = world;
            world.AddSystems(this);
        }

        public void Init()
        {
            for (int i = 0; i < updateSystemsList.Count; i++)
                updateSystemsList.Items[i].Update();
        }
        public Systems Add(UpdateSystem system)
        {
            updateSystemsList.Add(system);
            system.Init(world.Entities);
            system.Update();
            updateSystemsCount++;
            //injector.InjectArchetype(system, world);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void OnAdd(int type)
        {
            if(!Alive) return;;
            if (OnAddSystems.HasKey(type))
            {
                foreach (var system in OnAddSystems[type])
                {
                    system.Execute();
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void OnRemove(int type)
        {
            if(!Alive) return;;
            if (OnRemoveSystems.HasKey(type))
                foreach (var system in OnRemoveSystems[type])
                    system.Execute();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void OnRemove<T>()
        {
            if(!Alive) return;;
            if (OnRemoveSystems.HasKey(ComponentType<T>.ID))
                foreach (var system in OnRemoveSystems[ComponentType<T>.ID])
                    system.Execute();
        }

        internal void Kill()
        {
            
            Alive = false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Systems AddReactive(IReactive system)
        {
            if (system is IOnAdd add)
            {
                var triggerType = add.TriggerType;
                if (!OnAddSystems.HasKey(triggerType))
                    OnAddSystems.Add(triggerType, new List<IOnAdd>());
                OnAddSystems[triggerType].Add(add);
                add.Init(world.Entities);
                add.Execute();
            }
            else
            if (system is IOnRemove remove)
            {
                var triggerType = remove.TriggerType;
                if (!OnRemoveSystems.HasKey(triggerType))
                    OnRemoveSystems.Add(triggerType, new List<IOnRemove>());
                OnRemoveSystems[triggerType].Add(remove);
                remove.Init(world.Entities);
                remove.Execute();
            }
            return this;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnUpdate()
        {
            if(!Alive) return;;
            for (var i = 0; i < updateSystemsCount; i++)
                updateSystemsList.Items[i].Update();
        }
    }
    public interface IUpdateSystem
    {
        void Update();
    }

    public abstract class OnAdd<A> : IOnAdd
    {
        public int TriggerType => ComponentType<A>.ID;
        protected Entities entities;
        public void Init(Entities entities)
        {
            this.entities = entities;
        }
        public abstract void Execute();

    }
    public abstract class OnRemove<A> : IOnRemove
    {
        public int TriggerType => ComponentType<A>.ID;
        protected Entities entities;
        public void Init(Entities entities)
        {
            this.entities = entities;
        }
        public abstract void Execute();
    }
    internal interface IOnAdd : IReactive
    {
        void Execute();
    }
    internal interface IOnRemove : IReactive
    {
        void Execute();
    }
    public interface IReactive
    {
        void Init(Entities entities);
        int TriggerType { get; }
    }
    public abstract class UpdateSystem : IUpdateSystem
    {
        protected Entities entities;
        public void Init(Entities entities)
        {
            this.entities = entities;
        }
        public abstract void Update();
    }
}