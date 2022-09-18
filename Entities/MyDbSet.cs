using ARInventory.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARInventory.Entities
{
    /// <summary>
    /// A set of entities accessible by the context.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MyDbSet<T> where T : IGotId
    {
        public readonly List<T> _entities = new List<T>();

        public void Add(T entity)
        {
            // TODO check to make sure GUID is unique, or init GUID if none
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

        // TODO? might be nice API to have the MyDbSet class already be a list, instead of needing to use this method...
        public List<T> ToList()
        {
            return _entities.ToList();
        }
    }
}
