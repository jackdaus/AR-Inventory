using ARInventory.Entities.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ARInventory.Entities
{
    /// <summary>
    /// A set of entities accessible by the context.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// 
    /// Implementation note: We are using the ICollection interface for JSON serialization
    public class EntitySet<T> : ICollection<T> where T : IGotId
    {
        private readonly List<T> _entities = new List<T>();

        public void Add(T entity)
        {
            if (Contains(entity))
                throw new InvalidOperationException($"Cannot add entity {typeof(T).Name} to {typeof(EntitySet<T>).Name}. Duplicate Id {entity.Id}");

            if (entity.Id == Guid.Empty)
                entity.Id = Guid.NewGuid();

            _entities.Add(entity);
        }

        public bool Remove(T entity)
        {
            var entityToRemove = _entities.Find(x => x.Id == entity.Id);
            if (entityToRemove == null)
                return false;

            _entities.Remove(entityToRemove);
            return true;
        }

        public bool Update(T updatedEntity)
        {
            var oldEntity = _entities.Find(x => x.Id == updatedEntity.Id);
            if (oldEntity == null)
                return false;

            _entities.Remove(oldEntity);
            _entities.Add(updatedEntity);
            return true;
        }

        #region ICollection stuff

        public int Count => _entities.Count();

        public bool IsReadOnly => false;

        public void Clear()
        {
            _entities.Clear();
        }

        public bool Contains(T entity)
        {
            return _entities.Any(ent => ent.Id == entity.Id);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _entities.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _entities.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _entities.GetEnumerator();
        }

        #endregion
    }
}
