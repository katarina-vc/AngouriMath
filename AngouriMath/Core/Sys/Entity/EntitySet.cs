﻿using AngouriMath.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AngouriMath
{
    /// <summary>
    /// Custom view for list of entities
    /// </summary>
    public class EntitySet : List<Entity>
    {
        public EntitySet Simplify() => Simplify(3);
        public EntitySet Simplify(int level) => new EntitySet(this.Select(p => p.Simplify(level)));
        private readonly HashSet<string> exsts = new HashSet<string>();

        /// <summary>
        /// Neat output
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "[" + string.Join(", ", this) + "]";
        }
        public new void Add(Entity ent) => Add(ent, true);
        public void Add(Entity ent, bool check)
        {
            if (!check)
            {
                base.Add(ent);
                return;
            }
            if (ent == null)
                return;
            if (ent.entType == Entity.EntType.NUMBER && ent.GetValue().IsNull)
                return;
            ent = MathS.CanBeEvaluated(ent) ? ent.InnerSimplify() : ent;
            var hash = ent.ToString();
            if (!exsts.Contains(hash))
            {
                base.Add(ent);
                exsts.Add(hash);
            }
        }
        public void Merge(IEnumerable<Number> list)
        {
            foreach (var l in list)
                Add(l);
        }
        public void Merge(IEnumerable<Entity> list)
        {
            foreach (var l in list)
                Add(l);
        }
        public EntitySet(params Entity[] entites)
        {
            foreach (var el in entites)
                Add(el);
        }
        public EntitySet(bool check, params Entity[] entities)
        {
            foreach (var el in entities)
                Add(el, check);
        }
        public EntitySet(IEnumerable<Entity> list)
        {
            foreach (var el in list)
                Add(el);
        }
        // TODO: needs optimization
        public static EntitySet operator +(EntitySet set, Entity a) => new EntitySet(set.Select(el => el + a));
        public static EntitySet operator -(EntitySet set, Entity a) => new EntitySet(set.Select(el => el - a));
        public static EntitySet operator *(EntitySet set, Entity a) => new EntitySet(set.Select(el => el * a));
        public static EntitySet operator /(EntitySet set, Entity a) => new EntitySet(set.Select(el => el / a));
        public static EntitySet operator +(Entity a, EntitySet set) => new EntitySet(set.Select(el => a + el));
        public static EntitySet operator -(Entity a, EntitySet set) => new EntitySet(set.Select(el => a - el));
        public static EntitySet operator *(Entity a, EntitySet set) => new EntitySet(set.Select(el => a * el));
        public static EntitySet operator /(Entity a, EntitySet set) => new EntitySet(set.Select(el => a / el));
    }
}