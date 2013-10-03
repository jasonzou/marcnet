using System;
using System.Collections;

namespace MarcNet
{

    /// <summary>
    /// LargeListDictionary acts the same as a ListDictionary but performs well on large lists
    /// while still preserving the order of the list when enumerated over.
    /// </summary>
    public class DupHashlist : IDictionary, ICollection, IEnumerable
    {

        private int _maxIndex;
        private Hashtable _itemsByKey;
        private Hashtable _itemsByIndex;

        /// <summary>
        /// Creates a LargeListDictionary with the default comparer
        /// </summary>
        public DupHashlist()
            : this(null)
        {
        }

        /// <summary>
        /// Creates a LargeListDictionary with a custom comparer
        /// </summary>
        /// <param name="comparer">IComparer object or null for default comparer</param>
        public DupHashlist(IComparer comparer)
        {
            _itemsByKey = new Hashtable(null, comparer);
            _itemsByIndex = new Hashtable();
            _maxIndex = -1;
        }

        #region IDictionary Members

        /// <summary>
        /// IsReadonly pass-through
        /// </summary>
        public bool IsReadOnly
        {
            get { return _itemsByKey.IsReadOnly; }
        }

        /// <summary>
        /// Return a LargeListDictionaryEnumerator
        /// </summary>
        /// <returns>IDictionaryEnumerator for a LargeListDictionary</returns>
        public IDictionaryEnumerator GetEnumerator()
        {
            return (new DupHashlistEnumerator(_itemsByIndex, _maxIndex));
        }

        /// <summary>
        /// Item accessor
        /// </summary>
        public object this[object key]
        {
            get
            {
                IndexedObject io = null;
                foreach (DictionaryEntry de in _itemsByKey)
                {
                    IndexedKey myKey = null;
                    myKey = (IndexedKey)de.Key;
                    if (myKey.Key.Equals(key))
                    {
                        io = (IndexedObject)de.Value;
                        break;
                    }
                }

                if (io == null)
                    return null;
                else
                    return io.Object;
            }
            set
            {
                IndexedObject io = null;
                IndexedKey myKey = null;
                foreach (DictionaryEntry de in _itemsByKey)
                {                    
                    myKey = (IndexedKey)de.Key;
                    if (myKey.Key.Equals(key))
                    {
                        io = (IndexedObject)de.Value;
                        break;
                    }
                }
                
                if (io == null)
                    Add(myKey, value);
                else
                    io.Object = value;
            }
        }

        private IndexedKey getIndexedKey(object key)
        {
            IndexedObject io = null;
            foreach (DictionaryEntry de in _itemsByKey)
            {
                IndexedKey myKey = null;
                myKey = (IndexedKey)de.Key;
                if (myKey.Key.Equals(key))
                {
                    io = (IndexedObject)de.Value;
                    break;
                }
            }

            if (io == null)
                return null;
            else
                return io.keyObject;
/*
            IDictionaryEnumerator myEnum = this.GetEnumerator();


            IndexedObject myObj = null;
            DictionaryEntry de = new DictionaryEntry();
            while (myEnum.MoveNext())
            {
                de = (DictionaryEntry)myEnum.Current;
                myObj = (IndexedObject)de.Value;
                if (myObj.Key.Equals(key))
                {
                    break;
                }

            }
            return myObj.keyObject;
 */
        }

        /// <summary>
        /// getIndexedKeys: 
        /// return a list of keys;
        /// </summary>
        /// <param name="key"></param>
        private void getIndexedKeys(object key)
        {
        }

        /// <summary>
        /// getIndexedObjects:
        /// return a list of objects.
        /// </summary>
        /// <param name="key"></param>
        private void getIndexedObjects(object key)
        {
        }

        public IndexedObject getIndexedObject(object key)
        {
            IndexedObject io = null;
            foreach (DictionaryEntry de in _itemsByKey)
            {
                IndexedKey myKey = null;
                myKey = (IndexedKey)de.Key;
                if (myKey.Key.Equals(key))
                {
                    io = (IndexedObject)de.Value;
                    break;
                }
            }

            if (io == null)
            {
                return null;
            }
            else
            {
                return io;
            }

            /*IDictionaryEnumerator myEnum = this.GetEnumerator();


            IndexedObject myObj = null;
            DictionaryEntry de = new DictionaryEntry();
            while (myEnum.MoveNext())
            {
                de = (DictionaryEntry)myEnum.Current;
                myObj = (IndexedObject)de.Value;
                if (myObj.Key.Equals(key))
                {
                    break;
                }

            }
            return myObj;
            */
        }

        /// <summary>
        /// Remove an item by it's key
        /// </summary>
        /// <param name="key">key of item to remove</param>
        public void Remove(object key)
        {

            // Remove the object from the hashtable, and also remove the corresponding item from the
            // key list... adjust all the indexes appropriately
            if (Contains(key))
            {
                IndexedKey io = (IndexedKey)getIndexedKey(key);
                _itemsByKey.Remove(io);
                _itemsByIndex.Remove(io.Index);
            }
        }

        /// <summary>
        /// Returns true if the Dictionary contains the specified key
        /// </summary>
        /// <param name="key">key of item to check for</param>
        /// <returns>true if key is in the dictionary</returns>
        public bool Contains(object key)
        {
            IndexedKey io = (IndexedKey)getIndexedKey(key);
            if (io == null)
            {
                return false;
            }
            else
            {
                return _itemsByKey.ContainsKey(io);
            }
        }

        /// <summary>
        /// Returns true if the Dictionary contains the specified key
        /// </summary>
        /// <param name="key">key of item to check for</param>
        /// <returns>true if key is in the dictionary</returns>
        public bool ContainsKey(object key)
        {
            return Contains(key);
        }

        /// <summary>
        /// Clears the dictionary
        /// </summary>
        public void Clear()
        {
            _itemsByIndex.Clear();
            _itemsByKey.Clear();
            _maxIndex = -1;
        }

        /// <summary>
        /// Returns an ArrayList of the values in their FIFO sequence
        /// </summary>
        public ICollection Values
        {
            get
            {
                ArrayList result = new ArrayList(this.Count);
                foreach (DictionaryEntry de in this)
                {
                    result.Add(de.Value);
                }
                return result;
            }
        }

        /// <summary>
        /// Adds an object to the dictionary
        /// </summary>
        /// <param name="key">key for the object</param>
        /// <param name="value">object to store</param>
        public void Add(object key, object value)
        {
            _maxIndex++;
            int newIndex = _maxIndex;

            IndexedObject io = new IndexedObject(newIndex, key, value);
            _itemsByKey.Add(io.keyObject, io);
            _itemsByIndex.Add(io.Index, io);
        }

        public void Replace(object key, object value)
        {
            Remove(key);
            Add(key,value);
        }

        /// <summary>
        /// Returns an ArrayList of the keys in their FIFO sequence
        /// </summary>
        public ICollection Keys
        {
            get
            {
                ArrayList result = new ArrayList(this.Count);
                foreach (DictionaryEntry de in this)
                {
                    result.Add(de.Key);
                }
                return result;
            }
        }

        /// <summary>
        /// IsFixedSize pass-through
        /// </summary>
        public bool IsFixedSize
        {
            get { return _itemsByKey.IsFixedSize; }
        }

        #endregion

        #region ICollection Members

        /// <summary>
        /// IsSynchronized pass-through
        /// </summary>
        public bool IsSynchronized
        {
            get { return _itemsByKey.IsSynchronized; }
        }

        /// <summary>
        /// Returns number of items in dictionary
        /// </summary>
        public int Count
        {
            get { return _itemsByKey.Count; }
        }

        /// <summary>
        /// Copies the values in FIFO order to an array.
        /// </summary>
        /// <param name="array">destination Array</param>
        /// <param name="index">index for first item</param>
        public void CopyTo(Array array, int index)
        {
            this.Values.CopyTo(array, index);
        }

        /// <summary>
        /// SyncRoot passthrough
        /// </summary>
        public object SyncRoot
        {
            get { return _itemsByKey.SyncRoot; }
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Gets a LargeListDictionaryEnumerator
        /// </summary>
        /// <returns>LargeListDictionaryEnumerator</returns>
        IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (new DupHashlistEnumerator(_itemsByIndex, _maxIndex));
        }

        #endregion

        #region Subclasses

        public class IndexedKey
        {
            private int _index;
            private object _key;

            public IndexedKey(int index, object key)
            {
                _index = index;
                _key = key;
            }

            public object Key
            {
                get { return _key; }
                set { _key = value; }
            }

            public int Index
            {
                get { return _index; }
                set { _index = value; }
            }
        }

        /// <summary>
        /// Struct for storing the key-value pair with an index
        /// </summary>
        public class IndexedObject 
        {
            private IndexedKey _keyObject;
            private object _object;

            public IndexedObject(int index, object key, object o)
            {
                _keyObject = new IndexedKey(index, key);
                _object = o;
            }

            public object Key
            {
                get { return _keyObject.Key; }
                set { _keyObject.Key = value; }
            }

            public int Index
            {
                get { return _keyObject.Index; }
                set { _keyObject.Index = value; }
            }

            public object Object
            {
                get { return _object; }
                set { _object = value; }
            }
            public IndexedKey keyObject
            {
                get { return _keyObject; }
            }
        }

        /// <summary>
        /// Custom enumerator for LargeListDictionary
        /// </summary>
        public struct DupHashlistEnumerator : IDictionaryEnumerator, IEnumerator
        {

            private int _currentIndex;
            private Hashtable _itemsByIndex;
            private int _maxIndex;

            /// <summary>
            /// Creates an enumerator for LargeListDictionary
            /// </summary>
            /// <param name="itemsByIndex">Hashtable of the items with the index being the key</param>
            /// <param name="maxIndex">maximum index assigned so far to any item in the LargeListDictionary</param>
            public DupHashlistEnumerator(Hashtable itemsByIndex, int maxIndex)
            {
                _itemsByIndex = itemsByIndex;
                _currentIndex = -1;
                _maxIndex = maxIndex;
            }

            #region IDictionaryEnumerator Members

            /// <summary>
            /// Gets the key of the current item
            /// </summary>
            public object Key
            {
                get
                {
                    IndexedObject io = (IndexedObject)_itemsByIndex[_currentIndex];
                    return io.Key;
                }
            }

            public object KeyObject
            {
                get
                {
                    IndexedObject io = (IndexedObject)_itemsByIndex[_currentIndex];
                    return io.keyObject;
                }
            }

            /// <summary>
            /// Gets the value of the current item
            /// </summary>
            public object Value
            {
                get
                {
                    IndexedObject io = (IndexedObject)_itemsByIndex[_currentIndex];
                    return io.Object;
                }
            }

            /// <summary>
            /// Gets the DictionaryEntry of the current item
            /// </summary>
            public DictionaryEntry Entry
            {
                get
                {
                    IndexedObject io = (IndexedObject)_itemsByIndex[_currentIndex];
                    return new DictionaryEntry(io.Key, io);
                }
            }

            #endregion

            #region IEnumerator Members

            /// <summary>
            /// Resets the enumerator
            /// </summary>
            public void Reset()
            {
                _currentIndex = -1;
            }

            /// <summary>
            /// Gets the Dictionary entry of the current item
            /// </summary>
            public object Current
            {
                get { return this.Entry; }
            }

            /// <summary>
            /// Increments the indexer.  For the LargeListDictionary, if items have been removed, the next index (+1) may not
            /// have an entry.  Continue to increment until an item is found, or the max is reached.
            /// </summary>
            /// <returns>true if another item has been reached, false if the end of the list is hit.</returns>
            public bool MoveNext()
            {
                while (_currentIndex <= _maxIndex)
                {
                    _currentIndex++;
                    if (_itemsByIndex.ContainsKey(_currentIndex))
                        return true;
                }

                return false;
            }

            #endregion
        }

        #endregion
    }
}
