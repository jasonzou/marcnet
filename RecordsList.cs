using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace MarcNet
{
    public class RecordsList
    {
        private DupHashlist _myList = new DupHashlist();
        public RecordsList()
        {

        }

        public void addRecord(string subFieldCode, string subField)
        {

            _myList.Add(subFieldCode, subField);
        }

        public void removeRecord(string subFieldCode)
        {
            _myList.Remove(subFieldCode);
        }

        public char[] asRaw()
        {
            char[] raw = new char[rawLength];

            IDictionaryEnumerator myEnum = _myList.GetEnumerator();
            string myObject = null;

            String retStr = null;

            while (myEnum.MoveNext())
            {
                myObject = (string)myEnum.Value;
                retStr += String.Format("{0}{1}{2}", MARCChar.SUBFIELD_INDICATOR.ToString(), myEnum.Key.ToString(), myObject);
            }
            if (retStr != null)
            {
                retStr.CopyTo(0, raw, 0, rawLength);
            }

            return raw;
        }

        public string asFormatted()
        {
            IDictionaryEnumerator myEnum = _myList.GetEnumerator();
            string myObject = null;

            String retStr = null;

            while (myEnum.MoveNext())
            {
                myObject = (string)myEnum.Value;
                retStr += String.Format("${0:1}{1}", myEnum.Key.ToString(), myObject);
            }

            return retStr;
        }

        public string getSubField(string code)
        {
            string retStr = null;
            DupHashlist.IndexedObject myObject = _myList.getIndexedObject(code);

            if (myObject != null)
            {
                retStr = myObject.Object.ToString();
            }

            return retStr;
        }

        public bool ContainsKey(string code)
        {
            return _myList.ContainsKey(code);
        }

        public bool Contains(string code)
        {
            return _myList.Contains(code);
        }

        public int Length
        {
            get
            {
                IDictionaryEnumerator myEnum = _myList.GetEnumerator();
                string myObject = null;
                int len = 0;
                while (myEnum.MoveNext())
                {
                    myObject = (string)myEnum.Value;
                    len += (myEnum.Key.ToString().Length + myObject.Length);
                }
                return len;
            }
        }

        public int rawLength
        {
            get
            {
                return Count + Length;
            }
        }

        public int Count
        {
            get
            {
                return _myList.Count;
            }
        }
    }
}
