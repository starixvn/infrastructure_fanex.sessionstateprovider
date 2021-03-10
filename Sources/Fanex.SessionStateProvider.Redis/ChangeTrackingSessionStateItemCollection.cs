namespace Fanex.SessionStateProvider.Redis
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Runtime.Serialization;
    using System.Web.SessionState;

    [Serializable]
    public sealed class ChangeTrackingSessionStateItemCollection : NameObjectCollectionBase, ISessionStateItemCollection
    {
        public ChangeTrackingSessionStateItemCollection()
        {
            InnerCollection = new SessionStateItemCollection();
            ModifiedKeys = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            DeletedKeys = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        }

        private ChangeTrackingSessionStateItemCollection(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            InnerCollection = info.GetValue(nameof(InnerCollection), typeof(SessionStateItemCollection)) as SessionStateItemCollection;
            ModifiedKeys = info.GetValue(nameof(ModifiedKeys), typeof(HashSet<string>)) as HashSet<string>;
            DeletedKeys = info.GetValue(nameof(DeletedKeys), typeof(HashSet<string>)) as HashSet<string>;
        }

        public bool Dirty
        {
            get => InnerCollection.Dirty;

            set
            {
                InnerCollection.Dirty = value;

                if (!value)
                {
                    ModifiedKeys.Clear();
                    DeletedKeys.Clear();
                }
            }
        }

        public override int Count => InnerCollection.Count;

        public override KeysCollection Keys => InnerCollection.Keys;

        public HashSet<string> ModifiedKeys { get; }

        public HashSet<string> DeletedKeys { get; }

        private SessionStateItemCollection InnerCollection { get; }

        public object this[string name]
        {
            get
            {
                var value = InnerCollection[name];

                if (IsReferenceType(value))
                {
                    MarkAsModifiedKey(name);
                }

                return value;
            }

            set
            {
                InnerCollection[name] = value;
                MarkAsModifiedKey(name);
            }
        }

        public object this[int index]
        {
            get
            {
                var value = InnerCollection[index];

                if (IsReferenceType(value))
                {
                    MarkAsModifiedKey(InnerCollection.Keys[index]);
                }

                return value;
            }

            set
            {
                InnerCollection[index] = value;
                MarkAsModifiedKey(InnerCollection.Keys[index]);
            }
        }

        public void Clear()
        {
            foreach (string key in InnerCollection.Keys)
            {
                MarkAsDeletedKey(key);
            }

            InnerCollection.Clear();
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(InnerCollection), InnerCollection, typeof(SessionStateItemCollection));
            info.AddValue(nameof(ModifiedKeys), ModifiedKeys, typeof(HashSet<string>));
            info.AddValue(nameof(DeletedKeys), InnerCollection, typeof(HashSet<string>));
        }

        public void Remove(string name)
        {
            if (InnerCollection[name] != null)
            {
                MarkAsDeletedKey(name);
            }

            InnerCollection.Remove(name);
        }

        public void RemoveAt(int index)
        {
            var removingKey = InnerCollection.Keys[index];

            if (removingKey != null)
            {
                MarkAsDeletedKey(removingKey);
            }

            InnerCollection.RemoveAt(index);
        }

        internal void SetDataWithoutMarkingModifiedKey(string name, object value)
            => InnerCollection[name] = value;

        internal object GetDataWithoutMarkingModifiedKey(string name)
            => InnerCollection[name];

        public override IEnumerator GetEnumerator()
            => InnerCollection.GetEnumerator();

        private void MarkAsModifiedKey(string key)
        {
            Dirty = true;

            DeletedKeys.Remove(key);
            ModifiedKeys.Add(key);
        }

        private void MarkAsDeletedKey(string key)
        {
            Dirty = true;

            ModifiedKeys.Remove(key);
            DeletedKeys.Add(key);
        }

        private static bool IsReferenceType(object data)
            => data != null && !data.GetType().IsValueType && !(data is string);
    }
}