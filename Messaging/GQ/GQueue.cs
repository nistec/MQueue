using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Nistec.Messaging
{

    /// <summary>
    /// Represents a first-in, first-out collection of objects.
    /// </summary>
    /// <typeparam name="T">Specifies the type of elements in the queue.</typeparam>
    [Serializable, /*ComVisible(false), DebuggerTypeProxy(typeof(System_QueueDebugView<>)),*/ DebuggerDisplay("Count = {Count}")]
    public class GQueue<T> : IEnumerable<T>, ICollection, IEnumerable
    {
        // Fields
        private const int _DefaultCapacity = 4;
        private const int _GrowFactor = 200;
        private const int _MinimumGrow = 4;
        private const int _ShrinkThreshold = 0x20;

        private T[] _array;
        private static T[] _emptyArray;
        private int _head;
        private int _size;
        [NonSerialized]
        private object _syncRoot;
        private int _tail;
        private int _version;



        // Methods
        static GQueue()
        {
            GQueue<T>._emptyArray = new T[0];
        }
        /// <summary>
        /// Initializes a new instance of the GQueue class
        /// </summary>
        public GQueue()
        {
            this._array = GQueue<T>._emptyArray;
        }
        /// <summary>
        /// Initializes a new instance of the GQueue class 
        /// that contains elements copied from the specified collection.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new Queue.</param>
        public GQueue(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("Invalid collection");
            }
            this._array = new T[4];
            this._size = 0;
            this._version = 0;
            using (IEnumerator<T> enumerator = collection.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    this.Enqueue(enumerator.Current);
                }
            }
        }
        /// <summary>
        /// Initializes a new instance of the GQueue class
        ///with a specified initial capacity.
        /// </summary>
        /// <param name="capacity"></param>
        public GQueue(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException("Argument capacity is out of range");
            }
            this._array = new T[capacity];
            this._head = 0;
            this._tail = 0;
            this._size = 0;
        }

        /// <summary>
        /// Removes all items from the Queue.
        /// </summary>
        public void Clear()
        {
            if (this._head < this._tail)
            {
                Array.Clear(this._array, this._head, this._size);
            }
            else
            {
                Array.Clear(this._array, this._head, this._array.Length - this._head);
                Array.Clear(this._array, 0, this._tail);
            }
            this._head = 0;
            this._tail = 0;
            this._size = 0;
            this._version++;
        }

        /// <summary>
        /// Determines whether an element is in the Queue.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(T item)
        {
            int index = this._head;
            int num2 = this._size;
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            while (num2-- > 0)
            {
                if (item == null)
                {
                    if (this._array[index] == null)
                    {
                        return true;
                    }
                }
                else if ((this._array[index] != null) && comparer.Equals(this._array[index], item))
                {
                    return true;
                }
                index = (index + 1) % this._array.Length;
            }
            return false;
        }

        /// <summary>
        /// Copies the Queue elements to an existing one-dimensional
        /// System.Array, starting at the specified array index.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
            {
                new ArgumentNullException("Invalid array");
                return;
            }
            if ((arrayIndex < 0) || (arrayIndex > array.Length))
            {
                new ArgumentOutOfRangeException("array Index is out of range");//, ExceptionResource.ArgumentOutOfRange_Index);
                return;
            }
            int length = array.Length;
            if ((length - arrayIndex) < this._size)
            {
                new ArgumentNullException("Argument Invalid Off Length");
                return;
            }
            int num2 = ((length - arrayIndex) < this._size) ? (length - arrayIndex) : this._size;
            if (num2 != 0)
            {
                int num3 = ((this._array.Length - this._head) < num2) ? (this._array.Length - this._head) : num2;
                Array.Copy(this._array, this._head, array, arrayIndex, num3);
                num2 -= num3;
                if (num2 > 0)
                {
                    Array.Copy(this._array, 0, array, (arrayIndex + this._array.Length) - this._head, num2);
                }
            }
        }
        /*
        /// <summary>
        /// Removes and returns the object at the beginning of the Queue.
        /// </summary>
        /// <returns>The type of elements in the queue, if queue is empty return default(T).</returns>
        public virtual T Dequeue()
        {
            if (this._size == 0)
            {
                return default(T);  // throw new Exception("InvalidOperation_EmptyQueue");
            }
            T local = this._array[this._head];
            this._array[this._head] = default(T);
            this._head = (this._head + 1) % this._array.Length;
            this._size--;
            this._version++;
            //OnDequeueMessage(local);
            return local;
        }

        /// <summary>
        /// Adds an item to the end of the Queue.
        /// </summary>
        /// <param name="item"></param>
        public virtual void Enqueue(T item)
        {
            if (this._size == this._array.Length)
            {
                int capacity = (int)((this._array.Length * 200) / ((long)100));
                if (capacity < (this._array.Length + 4))
                {
                    capacity = this._array.Length + 4;
                }
                this.SetCapacity(capacity);
            }
            this._array[this._tail] = item;
            this._tail = (this._tail + 1) % this._array.Length;
            this._size++;
            this._version++;

            //OnEnqueueMessage(item);
        }
        */
        /// <summary>
        /// Removes and returns the object at the beginning of the Queue.
        /// </summary>
        /// <returns>The type of elements in the queue, if queue is empty return default(T).</returns>
        public void Enqueue(T item)
        {
            lock (this.SyncRoot)
            {
                if (this._size == this._array.Length)
                {
                    int capacity = (int)((this._array.Length * 200) / ((long)100));
                    if (capacity < (this._array.Length + 4))
                    {
                        capacity = this._array.Length + 4;
                    }
                    this.SetCapacity(capacity);
                }

                this._array[this._tail] = item;
                Interlocked.Exchange(ref this._tail, (this._tail + 1) % this._array.Length);
                Interlocked.Increment(ref this._size);
                Interlocked.Increment(ref this._version);
            }
            //OnEnqueueMessage(item);
        }


        public virtual T Dequeue()
        {
            lock (this.SyncRoot)
            {
                if (this._size == 0)
                {
                    return default(T);// throw new Exception("Empty Queue");
                }
                T local = this._array[this._head];
                if (local == null)
                {
                    return default(T); //throw new Exception("Empty Queue");

                }
                this._array[this._head] = default(T);

                Interlocked.Exchange(ref this._head, (this._head + 1) % this._array.Length);
                Interlocked.Decrement(ref this._size);
                Interlocked.Increment(ref this._version);
                //OnDequeueMessage(local);
                return local;
            }
        }

        //internal void SyncEnqueue(T item, int index)
        //{
        //    lock (this.SyncRoot)
        //    {
        //        if (this._size == this._array.Length)
        //        {
        //            int capacity = (int)((this._array.Length * 200) / ((long)100));
        //            if (capacity < (this._array.Length + 4))
        //            {
        //                capacity = this._array.Length + 4;
        //            }
        //            this.SetCapacity(capacity);
        //        }

        //        T itemTemp = this._array[index];
        //        this._array[this._tail] = itemTemp;
        //        this._array[index] = item;

        //        Interlocked.Exchange(ref this._tail, (this._tail + 1) % this._array.Length);
        //        Interlocked.Increment(ref this._size);
        //        Interlocked.Increment(ref this._version);
        //    }
        //    //OnEnqueueMessage(item);
        //}

        //internal void Enqueue(T item, int index)
        //{
        //    lock (this.SyncRoot)
        //    {
        //        if (this._size == this._array.Length)
        //        {
        //            int capacity = (int)((this._array.Length * 200) / ((long)100));
        //            if (capacity < (this._array.Length + 4))
        //            {
        //                capacity = this._array.Length + 4;
        //            }
        //            this.SetCapacity(capacity);
        //        }

        //        T itemTemp = this._array[index];
        //        this._array[this._tail] = itemTemp;
        //        this._array[index] = item;

        //        this._tail = (this._tail + 1) % this._array.Length;
        //        this._size++;
        //        this._version++;
        //    }
        //    //OnEnqueueMessage(item);
        //}

        internal T GetElement(int i)
        {
            return this._array[(this._head + i) % this._array.Length];
        }
        /// <summary>
        /// Returns an enumerator that iterates through the GQueue.
        /// </summary>
        /// <returns></returns>
        public Enumerator/*<T>*/ GetEnumerator()
        {
            return new Enumerator/*<T>*/((GQueue<T>)this);
        }

        /// <summary>
        /// Peek the first object at the beginning of the Queue without removing it.
        /// </summary>
        /// <returns>The type of elements in the queue, if queue is empty return default(T).</returns>
        public virtual T Peek()
        {
            if (this._size == 0)
            {
                return default(T);  //throw new Exception("ExceptionResource.InvalidOperation_EmptyQueue");
            }
            return this._array[this._head];
        }

        private void SetCapacity(int capacity)
        {
            T[] destinationArray = new T[capacity];
            if (this._size > 0)
            {
                if (this._head < this._tail)
                {
                    Array.Copy(this._array, this._head, destinationArray, 0, this._size);
                }
                else
                {
                    Array.Copy(this._array, this._head, destinationArray, 0, this._array.Length - this._head);
                    Array.Copy(this._array, 0, destinationArray, this._array.Length - this._head, this._tail);
                }
            }
            this._array = destinationArray;
            this._head = 0;
            this._tail = this._size;
            this._version++;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator/*<T>*/((GQueue<T>)this);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
            {
                new ArgumentNullException("invalid array");
                return;
            }
            if (array.Rank != 1)
            {
                new ArgumentNullException("Argument Rank Multi Dimenssion Not Supported");
                return;
            }
            if (array.GetLowerBound(0) != 0)
            {
                new ArgumentNullException("Argument Non Zero Lower Bound");
                return;
            }
            int length = array.Length;
            if ((index < 0) || (index > length))
            {
                new ArgumentOutOfRangeException("index Argument is Out Of Range");
                return;
            }
            if ((length - index) < this._size)
            {
                throw new Exception("Invalid Off Length");
            }
            int num2 = ((length - index) < this._size) ? (length - index) : this._size;
            if (num2 != 0)
            {
                try
                {
                    int num3 = ((this._array.Length - this._head) < num2) ? (this._array.Length - this._head) : num2;
                    Array.Copy(this._array, this._head, array, index, num3);
                    num2 -= num3;
                    if (num2 > 0)
                    {
                        Array.Copy(this._array, 0, array, (index + this._array.Length) - this._head, num2);
                    }
                }
                catch (ArrayTypeMismatchException)
                {
                    new ArgumentNullException("Invalid Array Type");
                    return;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator/*<T>*/((GQueue<T>)this);
        }

        public T[] ToArray()
        {
            T[] destinationArray = new T[this._size];
            if (this._size != 0)
            {
                if (this._head < this._tail)
                {
                    Array.Copy(this._array, this._head, destinationArray, 0, this._size);
                    return destinationArray;
                }
                Array.Copy(this._array, this._head, destinationArray, 0, this._array.Length - this._head);
                Array.Copy(this._array, 0, destinationArray, this._array.Length - this._head, this._tail);
            }
            return destinationArray;
        }

        /// <summary>
        /// Sets the capacity to the actual number of elements,
        /// if that number is less than 90 percent of current capacity.
        /// </summary>
        public void TrimOver()
        {
            int num = (int)(this._array.Length * 0.9);
            if (this._size < num)
            {
                this.SetCapacity(this._size);
            }
        }

        /// <summary>
        /// Gets the number of elements contained in the Queue.
        /// </summary>
        public int Count
        {
            get
            {
                return Interlocked.CompareExchange(ref _size,0,0);// this._size;
            }
        }
        /// <summary>
        /// Gets value indicating the Queue has elements contained in the Queue.
        /// </summary>
        public bool HasItems
        {
            get
            {
                //return  Interlocked.CompareExchange(ref _head, _size - 1, 0);
                return this._size > 0;
            }
        }
        public bool IsEmpty
        {
            get
            {
                return  Interlocked.CompareExchange(ref _size, 0, 0)==0;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                if (this._syncRoot == null)
                {
                    Interlocked.CompareExchange(ref this._syncRoot, new object(), null);
                }
                return this._syncRoot;
            }
        }

        public object SyncRoot
        {
            get
            {
                if (this._syncRoot == null)
                {
                    Interlocked.CompareExchange(ref this._syncRoot, new object(), null);
                }
                return this._syncRoot;
            }
        }

        internal int Head
        {
            get
            {
                return _head;
            }
        }
        internal int Tail
        {
            get
            {
                return _tail;
            }
        }

        // Nested Types
        [Serializable, StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
        {
            private GQueue<T> _gq;
            private int _index;
            private int _version;
            private T _currentElement;
            internal Enumerator(GQueue<T> q)
            {
                this._gq = q;
                this._version = this._gq._version;
                this._index = -1;
                this._currentElement = default(T);
            }

            public void Dispose()
            {
                this._index = -2;
                this._currentElement = default(T);
            }

            public bool MoveNext()
            {
                if (this._version != this._gq._version)
                {
                    throw new Exception("Enumerator Failed Version");
                }
                if (this._index == -2)
                {
                    return false;
                }
                this._index++;
                if (this._index == this._gq._size)
                {
                    this._index = -2;
                    this._currentElement = default(T);
                    return false;
                }
                this._currentElement = this._gq.GetElement(this._index);
                return true;
            }

            public T Current
            {
                get
                {
                    if (this._index < 0)
                    {
                        if (this._index == -1)
                        {
                            throw new Exception("Enumerator Not Started");
                        }
                        else
                        {
                            throw new Exception("Enumerator Ended");
                        }
                    }
                    return this._currentElement;
                }
            }
            object IEnumerator.Current
            {
                get
                {
                    if (this._index < 0)
                    {
                        if (this._index == -1)
                        {
                            throw new Exception("Enumerator Not Started");
                        }
                        else
                        {
                            throw new Exception("Enumerator Ended");
                        }
                    }
                    return this._currentElement;
                }
            }
            void IEnumerator.Reset()
            {
                if (this._version != this._gq._version)
                {
                    throw new Exception("Enumerator Failed Version");
                }
                this._index = -1;
                this._currentElement = default(T);
            }
        }
    }
}
