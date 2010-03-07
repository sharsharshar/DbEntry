﻿using System.Collections;
using System.Collections.Generic;
using Lephone.Data.Common;

namespace Lephone.Data.Definition
{
    public abstract class LazyLoadListBase<T> : IList<T>, ILazyLoading
    {
        protected string ForeignKeyName;
        protected object owner;
        protected DbContext context;
        protected bool m_IsLoaded;
        protected IList<T> InnerList = new DbObjectList<T>();

        protected LazyLoadListBase(object owner)
        {
            this.owner = owner;
        }

        #region ILazyLoad members

        bool ILazyLoading.IsLoaded
        {
            get { return m_IsLoaded; }
            set { m_IsLoaded = value; }
        }

        object ILazyLoading.Read()
        {
            if (!m_IsLoaded)
            {
                ((ILazyLoading)this).Load();
                m_IsLoaded = true;
                context = null;
            }
            return InnerList;
        }

        void ILazyLoading.Write(object item, bool IsLoad)
        {
            InnerWrite(item, IsLoad);
            InnerList.Add((T)item);
        }

        protected abstract void InnerWrite(object item, bool IsLoad);

        protected void WriteAndSet(object item)
        {
            m_IsLoaded = true;
            InnerWrite(item, false);
            context = null;
        }

        void ILazyLoading.Init(DbContext context, string ForeignKeyName)
        {
            this.context = context;
            this.ForeignKeyName = ForeignKeyName;
        }

        void ILazyLoading.Load()
        {
            if (ForeignKeyName == null) { return; }
            IList<T> l = InnerLoad();
            AddToInnerList(l);
        }

        protected abstract IList<T> InnerLoad();

        protected void AddToInnerList(IList<T> l)
        {
            MemberHandler af = ObjectInfo.GetInstance(typeof(T)).KeyFields[0];
            object tkey = null;
            if (InnerList.Count == 1)
            {
                tkey = af.GetValue(InnerList[0]);
            }
            bool Found = false;
            int Index = 0;
            foreach (T o in l)
            {
                InnerWrite(o, true);
                if (af.GetValue(o).Equals(tkey))
                {
                    Found = true;
                }
                else
                {
                    if (Found)
                    {
                        InnerList.Add(o);
                    }
                    else
                    {
                        InnerList.Insert(Index++, o);
                    }
                }
            }
        }

        protected virtual void OnRemoveItem(T item) { }

        #endregion

        #region IList<T> members

        public int IndexOf(T item)
        {
            ((ILazyLoading)this).Read();
            return InnerList.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            WriteAndSet(item);
            InnerList.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            ((ILazyLoading)this).Read();
            T item = InnerList[index];
            InnerList.RemoveAt(index);
            OnRemoveItem(item);
        }

        public T this[int index]
        {
            get
            {
                ((ILazyLoading)this).Read();
                return InnerList[index];
            }
            set
            {
                WriteAndSet(value);
                InnerList[index] = value;
            }
        }

        #endregion

        #region ICollection<T> members

        public void Add(T item)
        {
            WriteAndSet(item);
            InnerList.Add(item);
        }

        public void Clear()
        {
            ((ILazyLoading)this).Read();
            foreach (var t in InnerList)
            {
                OnRemoveItem(t);
            }
            InnerList.Clear();
        }

        public bool Contains(T item)
        {
            ((ILazyLoading)this).Read();
            return InnerList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ((ILazyLoading)this).Read();
            InnerList.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get
            {
                ((ILazyLoading)this).Read();
                return InnerList.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                ((ILazyLoading)this).Read();
                return InnerList.IsReadOnly;
            }
        }

        public bool Remove(T item)
        {
            ((ILazyLoading)this).Read();
            bool ret = RemoveItem(item);
            if (ret)
            {
                OnRemoveItem(item);
            }
            return ret;
        }

        private bool RemoveItem(T item)
        {
            var oi = ObjectInfo.GetInstance(typeof(T));
            var kh = oi.KeyFields[0];
            var id = kh.GetValue(item);
            foreach (var obj in InnerList)
            {
                var idx = kh.GetValue(obj);
                if(idx.Equals(id))
                {
                    return InnerList.Remove(obj);
                }
            }
            return false;
        }

        #endregion

        #region IEnumerable<T> members

        public IEnumerator<T> GetEnumerator()
        {
            ((ILazyLoading)this).Read();
            return InnerList.GetEnumerator();
        }

        #endregion

        #region IEnumerable members

        IEnumerator IEnumerable.GetEnumerator()
        {
            ((ILazyLoading)this).Read();
            return ((IEnumerable)InnerList).GetEnumerator();
        }

        #endregion
    }
}
