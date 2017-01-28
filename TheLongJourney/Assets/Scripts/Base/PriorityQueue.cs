using System.Collections.Generic;

namespace Assets.Scripts.Base
{
    #region IPriorityQueue

    internal interface IPriorityQueue<T>
    {
        #region Methods

        int Push(T item);

        T Pop();

        T Peek();

        void Update(int i);

        void Clear();

        #endregion Methods
    }

    #endregion IPriorityQueue

    #region PriorityQueue

    internal class PriorityQueue<T> : IPriorityQueue<T>
    {
        #region Variables Declaration

        protected List<T> m_innerList = new List<T>();
        protected IComparer<T> m_mComparer;

        #endregion Variables Declaration

        #region Contructors

        public PriorityQueue()
        {
            m_mComparer = Comparer<T>.Default;
        }

        public PriorityQueue(IComparer<T> comparer)
        {
            m_mComparer = comparer;
        }

        public PriorityQueue(IComparer<T> comparer, int capacity)
        {
            m_mComparer = comparer;
            m_innerList.Capacity = capacity;
        }

        #endregion Contructors

        #region Methods

        protected void SwitchElements(int i, int j)
        {
            T h = m_innerList[i];
            m_innerList[i] = m_innerList[j];
            m_innerList[j] = h;
        }

        protected virtual int OnCompare(int i, int j)
        {
            return m_mComparer.Compare(m_innerList[i], m_innerList[j]);
        }

        /// <summary>
        /// Push an object onto the PQ
        /// </summary>
        /// <param name="item"></param>
        /// <returns>The index in the list where the object is now_. This will change when objects are taken from or put onto the PQ.</returns>
        public int Push(T item)
        {
            int p = m_innerList.Count;
            m_innerList.Add(item); // E[p] = O
            do
            {
                if (p == 0)
                    break;
                int p2 = (p - 1) / 2;
                if (OnCompare(p, p2) < 0)
                {
                    SwitchElements(p, p2);
                    p = p2;
                }
                else
                    break;
            } while (true);
            return p;
        }

        /// <summary>
        /// Get the smallest object and remove it.
        /// </summary>
        /// <returns>The smallest object</returns>
        public T Pop()
        {
            T result = m_innerList[0];
            int p = 0;
            m_innerList[0] = m_innerList[m_innerList.Count - 1];
            m_innerList.RemoveAt(m_innerList.Count - 1);
            do
            {
                int pn = p;
                int p1 = 2 * p + 1;
                int p2 = 2 * p + 2;
                if (m_innerList.Count > p1 && OnCompare(p, p1) > 0) // links kleiner
                    p = p1;
                if (m_innerList.Count > p2 && OnCompare(p, p2) > 0) // rechts noch kleiner
                    p = p2;

                if (p == pn)
                    break;
                SwitchElements(p, pn);
            } while (true);

            return result;
        }

        /// <summary>
        /// Notify the PQ that the object at position x has changed
        /// and the PQ needs to restore order.
        /// Since you dont have access to any indexes (except by using the
        /// explicit IList.this) you should not call this function without knowing exactly
        /// what you do.
        /// </summary>
        /// <param name="i">The index of the changed object.</param>
        public void Update(int i)
        {
            int p = i;
            int p2;
            do	// aufsteigen
            {
                if (p == 0)
                    break;
                p2 = (p - 1) / 2;
                if (OnCompare(p, p2) < 0)
                {
                    SwitchElements(p, p2);
                    p = p2;
                }
                else
                    break;
            } while (true);
            if (p < i)
                return;
            do	   // absteigen
            {
                int pn = p;
                int p1 = 2 * p + 1;
                p2 = 2 * p + 2;
                if (m_innerList.Count > p1 && OnCompare(p, p1) > 0) // links kleiner
                    p = p1;
                if (m_innerList.Count > p2 && OnCompare(p, p2) > 0) // rechts noch kleiner
                    p = p2;

                if (p == pn)
                    break;
                SwitchElements(p, pn);
            } while (true);
        }

        /// <summary>
        /// Get the smallest object without removing it.
        /// </summary>
        /// <returns>The smallest object</returns>
        public T Peek()
        {
            if (m_innerList.Count > 0)
                return m_innerList[0];
            return default(T);
        }

        public void Clear()
        {
            m_innerList.Clear();
        }

        public int Count
        {
            get { return m_innerList.Count; }
        }

        public void RemoveLocation(T item)
        {
            int index = -1;
            for (int i = 0; i < m_innerList.Count; i++)
            {
                if (m_mComparer.Compare(m_innerList[i], item) == 0)
                    index = i;
            }

            if (index != -1)
                m_innerList.RemoveAt(index);
        }

        public T this[int index]
        {
            get { return m_innerList[index]; }
            set
            {
                m_innerList[index] = value;
                Update(index);
            }
        }

        #endregion Methods
    }

    #endregion PriorityQueue
}