using System.Collections.Generic;
using FrameLog.Exceptions;
using FrameLog.Helpers;
using FrameLog.Models;
using System;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Reflection;

namespace FrameLog.Contexts
{
    public abstract partial class ObjectContextAdapter<TChangeSet, TPrincipal> 
        : IFrameLogContext<TChangeSet, TPrincipal>
        where TChangeSet : IChangeSet<TPrincipal>
    {
        private ObjectContext context;

        public ObjectContextAdapter(ObjectContext context)
        {
            this.context = context;
        }

        public virtual object GetObjectByKey(EntityKey key)
        {
            return context.GetObjectByKey(key);
        }

        public virtual string KeyPropertyName
        {
            get { return "Id"; }
        }
        public virtual object KeyFromReference(string reference)
        {
            return int.Parse(reference);
        }
        public virtual object GetObjectByReference(Type type, string reference)
        {
            try
            {
                var container = context.MetadataWorkspace.GetEntityContainer(context.DefaultContainerName, DataSpace.CSpace);
                var set = container.BaseEntitySets.FirstOrDefault(meta => meta.ElementType.Name == type.Name);
                if (set == null)
                    throw new ObjectTypeDoesNotExistInDataModelException(type);

                var key = new EntityKey(container.Name + "." + set.Name, KeyPropertyName, KeyFromReference(reference));
                return GetObjectByKey(key);
            }
            catch (Exception e)
            {
                throw new FailedToRetrieveObjectException(type, reference, e);
            }
        }
        public virtual bool ObjectHasReference(object entity)
        {
            if (entity == null)
                return false;

            IHasLoggingReference entityWithReference = entity as IHasLoggingReference;
            if (entityWithReference != null)
                return true;

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.IgnoreCase;
            string keyPropertyName = GetReferencePropertyForObject(entity);
            var keyProperty = entity.GetType().GetProperty(keyPropertyName, flags);
            if (keyProperty != null)
                return true;

            return false;
        }
        public virtual string GetReferenceForObject(object entity)
        {
            if (entity == null)
                return null;

            IHasLoggingReference entityWithReference = entity as IHasLoggingReference;
            if (entityWithReference != null)
                return entityWithReference.Reference.ToString();

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.IgnoreCase;
            string keyPropertyName = GetReferencePropertyForObject(entity);
            var keyProperty = entity.GetType().GetProperty(keyPropertyName, flags);
            if (keyProperty != null)
                return keyProperty.GetGetMethod().Invoke(entity, null).ToString();

            ObjectStateEntry entry = null;
            if (context.ObjectStateManager.TryGetObjectStateEntry(entity, out entry))
            {
                var keyMember = entry.EntityKey.EntityKeyValues.FirstOrDefault();
                if (keyMember != null && keyMember.Value != null)
                    return keyMember.Value.ToString();
            }

            throw new InvalidOperationException(string.Format("Attempted to log a foreign entity that did not implement IHasLoggingReference and that did not have a property with name '{0}'. It had type {1}, and it was '{2}'",
                    KeyPropertyName, entity.GetType(), entity));
        }
        public virtual string GetReferencePropertyForObject(object entity)
        {
            return KeyPropertyName;
        }

        public ObjectStateManager ObjectStateManager
        {
            get { return context.ObjectStateManager; }
        }
        public MetadataWorkspace Workspace
        {
            get { return context.MetadataWorkspace; }
        }
        public abstract Type UnderlyingContextType { get; }

        public virtual void DetectChanges()
        {
            context.DetectChanges();
        }

        public virtual int SaveChanges(SaveOptions saveOptions)
        {
            return context.SaveChanges(saveOptions);
        }

        public virtual int SaveAndAcceptChanges(EventHandler onSavingChanges = null)
        {
            // Wrap the save operation inside a disposable listener for the ObjectContext.SaveChanges event
            // By doing this, the event handler will be invoked after saving but BEFORE accepting the changes.
            using (new DisposableSavingChangesListener(context, onSavingChanges))
            {
                return context.SaveChanges(SaveOptions.AcceptAllChangesAfterSave);
            }
        }

        public abstract IQueryable<IChangeSet<TPrincipal>> ChangeSets { get; }
        public abstract IQueryable<IObjectChange<TPrincipal>> ObjectChanges { get; }
        public abstract IQueryable<IPropertyChange<TPrincipal>> PropertyChanges { get; }
        public abstract void AddChangeSet(TChangeSet changeSet);
    }
}
