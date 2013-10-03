using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;


namespace MarcNet
{


    public class MARCRecordDirectory
    {
        ArrayList _dir;
        int _entries;
        int _recLength;
        int _baseAddress;
        char[] _buff;

        public MARCRecordDirectory()
        {
            init();
        }

        public MARCRecordDirectory(char[] dirRaw)
        {
            char[] temp = new char[MARCChar.DIRECTORY_ENTRY_LEN];
            MARCDirectoryEntry tempEntry;

            init();
            _entries = (int)(dirRaw.Length / MARCChar.DIRECTORY_ENTRY_LEN);
            
            for (int i = 0; i < _entries; i++)
            {
                Array.Copy(dirRaw, i * MARCChar.DIRECTORY_ENTRY_LEN, temp, 0, MARCChar.DIRECTORY_ENTRY_LEN);
                tempEntry = new MARCDirectoryEntry(temp);

                addEntry(tempEntry);
            }

        }

        public ArrayList getEntries()
        {
            return _dir;
        }

        private void init()
        {
            _dir = new ArrayList();
            _entries = 0;
            _recLength = 0;
            _baseAddress = 0;

        }

        public int numOfEntries()
        {
            return _entries;
        }

        public char[] asRaw()
        {
            int len = _dir.Count * MARCChar.DIRECTORY_ENTRY_LEN + 1;
            _buff = new char[len];

            int tempLen = 0;
            foreach (MARCDirectoryEntry entry in _dir)
            {
                entry.asRaw().CopyTo(_buff, tempLen);
                tempLen += MARCChar.DIRECTORY_ENTRY_LEN;
            }
            _buff[len-1] = MARCChar.END_OF_FIELD;
            return _buff;
            
        }

        public void Clear()
        {
            _dir.Clear();
        }

        public int getBaseAddress()
        {
            _baseAddress = MARCChar.LEADER_LEN + _dir.Count * MARCChar.DIRECTORY_ENTRY_LEN + 1;
            return _baseAddress;
        }

        public int getRecLen()
        {
            _baseAddress = getBaseAddress();
            _recLength = _baseAddress + fieldLength() + 1;
            return _recLength;
        }

        public int Length()
        {
            return _dir.Count*MARCChar.DIRECTORY_ENTRY_LEN+1;
        }

        private int fieldLength()
        {
            int len = 0;
            foreach (MARCDirectoryEntry entry in _dir)
            {
                len += entry.fieldLength;
            }
            return len;
        }

        public void addEntry(MARCDirectoryEntry entry)
        {
            _dir.Add(entry);
        }

        public void deleteEntry(MARCDirectoryEntry entry)
        {
            _dir.Remove(entry);
        }
    }

    public class MARCDirectoryEntry
    {
        string _tag;
        int _len;
        int _dataEnd;
        char[] _buff;
        int _entries;

        public MARCDirectoryEntry(string tagno, int len, int dataEnd)
        {
            _tag        = tagno;
            _len        = len;
            _dataEnd    = dataEnd;
            _buff       = new char[MARCChar.DIRECTORY_ENTRY_LEN];

            String temp = String.Format("{0:3}{1:0000}{2:00000}", _tag, _len, _dataEnd);

            temp.CopyTo(0, _buff, 0, MARCChar.DIRECTORY_ENTRY_LEN);

        }

        public MARCDirectoryEntry(char[] entryRaw)
        {
  
            char[] buff = new char[3];

            Array.Copy(entryRaw, 0, buff, 0, 3);
            _tag = new string(buff);

            buff = new char[4];
            Array.Copy(entryRaw, 3, buff, 0, 4);
            _len = getNum(buff);

            buff = new char[5];
            Array.Copy(entryRaw, 7, buff, 0, 5);
            _dataEnd = getNum(buff);
        }

        private int getNum(char[] charArray)
        {
            int len = charArray.Length;
            int retNum = 0;;

            for (int i = 0; i < len; i++)
            {
                int temp = (int)(charArray[i] - '0') * (int)Math.Pow( 10, (len - i - 1));
                retNum += temp;
            }
            return retNum;
        }

        public char[] asRaw()
        {
            return _buff;
        }

        public int fieldLength
        {
            get
            {
                return _len;
            }
        }

        public int dataEnd
        {
            get
            {
                return _dataEnd;
            }
        }

        public string tagStr
        {
            get
            {
                return _tag;
            }
        }

    }


    public class MARCRecord
    {
        // Leader of the Record
        MARCLeader _ldr;

        // Record Directory
        MARCRecordDirectory _dir;

        // Contain all MARCField objects of the Record
        ArrayList _fields;

        ArrayList _warning;
        char[] _buff;

        public MARCRecord()
        {
            init();
        }

        public MARCRecord(char[] rawRecord)
        {
            char[] temp = new char[MARCChar.LEADER_LEN];

            init();

            Array.Copy(rawRecord, temp, MARCChar.LEADER_LEN);
            _ldr = new MARCLeader(temp);

            int dirLength = _ldr.getBaseAddressOfData() - MARCChar.LEADER_LEN;
            char[] temp1 = new char[dirLength];
            Array.Copy(rawRecord, MARCChar.LEADER_LEN, temp1, 0, dirLength);
            _dir = new MARCRecordDirectory(temp1);

            MARCField field;
            ArrayList dirEntries = _dir.getEntries();

            foreach(MARCDirectoryEntry entry in dirEntries)
            {
                temp1 = new char[entry.fieldLength];
                Array.Copy(rawRecord, _ldr.getBaseAddressOfData() + entry.dataEnd, temp1, 0, entry.fieldLength);
                field = new MARCField( entry.tagStr,entry.fieldLength, temp1 );
                addAField(field);
            }
            //char[] dir = new 


        }

        private void init()
        {
            _fields = new ArrayList();

            _dir = new MARCRecordDirectory();

            _ldr = null;

            _warning = new ArrayList();
        }

        public int getRecordLength()
        {
            return _dir.getRecLen();
        }

        public int getBaseAddress()
        {
            return _dir.getBaseAddress();
        }

        public void build_dir()
        {
            int dataEnd = 0;
            int len = 0;
            IEnumerator myEn = _fields.GetEnumerator();
            MARCField field = null;

            _dir.Clear();
            while(myEn.MoveNext())
            {
                field = (MARCField)myEn.Current;

                len = field.Length();
                _dir.addEntry( new MARCDirectoryEntry(field.tag, len, dataEnd));
                dataEnd += len;
            }
        }

        public void build_Leader()
        {
            if (_ldr == null)
            {
                _ldr = new MARCLeader(getRecordLength(), getBaseAddress());
            }
        }


        public char[] asRaw()
        {
            build_dir();

            int RecLen = getRecordLength();
            _buff = new char[RecLen];

            build_Leader();
            _ldr.asRaw().CopyTo(_buff, 0);

            
            _dir.asRaw().CopyTo(_buff,MARCChar.LEADER_LEN);

            int test = _dir.Length();

            int len = MARCChar.LEADER_LEN + _dir.Length();
            foreach (MARCField field in _fields)
            {
                field.asRaw().CopyTo(_buff, len);
                len += field.Length();
            }

            _buff[RecLen - 1] = MARCChar.END_OF_RECORD;
            //_dir.asRaw();

            return _buff;

        }

        public void addAField(MARCField field)
        {
            _fields.Add(field);
        }


        public MARCLeader leader
        {
            get
            {
                return _ldr;
            }
        }

        public MARCField getAField(string fieldno)
        {
            MARCField retMARCField  = null;
            MARCField temp1         = null;
            IEnumerator myEn        = _fields.GetEnumerator();

            while (myEn.MoveNext())
            {
                temp1 = (MARCField) myEn.Current;
                if (temp1.tag == fieldno)
                {
                    retMARCField = temp1;
                    break;
                }
                else
                {
                    continue;
                }
            }
            return retMARCField;
        }

        public subFieldsList getSubFields(string fieldno)
        {
            MARCField field = getAField(fieldno);

            if (field != null)
            {
                return field.subFields();
            }
            return null;

        }

        public string getASubField(string fieldno, string subfieldno)
        {
            MARCField field = getAField(fieldno);
            if (field != null)
            {
                return field.subField(subfieldno);
            }
            return null;
        }

        public void deleteAField(string fieldno)
        {
            MARCField retMARCField = null;
            MARCField temp1 = null;
            IEnumerator myEn = _fields.GetEnumerator();

            while (myEn.MoveNext())
            {
                temp1 = (MARCField)myEn.Current;
                if (temp1.tag == fieldno)
                {
                    _fields.Remove(temp1);
                    break;
                }
                else
                {
                    continue;
                }
            }
        }

        public ArrayList getFields()
        {
            return _fields;
        }

        public string asFormatted()
        {
            string retStr = "";
            foreach (MARCField field in _fields)
            {
                retStr += field.asFormatted() + "\n\r";
            }

            return retStr;

        }
/*        
        public string title()
        {
        }

        public string title_proper()
        {
        }

        public string author()
        {
        }

        public string edition()
        {
        }

        public string publication_date()
        {
        }

        public ArrayList fields()
        {
        }

        public string[] field()
        {
        }

        public string subfield()
        {
        }

        public int appendFields()
        {
        }

        public int insertFieldsBefore()
        {
        }

        public int insertFieldsAfter()
        {
        }

        public int insertFieldsOrdered()
        {
        }

        public int insertGroupedField()
        {
        }

        public int deleteField()
        {
        }

        public int asUsMarc()
        {
        }

        public int asFormatted()
        {
        }

        public int encoding()
        {
        }

        public int setLeaderLengths()
        {
        }

        public int clone()
        {
        }

        public ArrayList warnings()
        {
        }


        */

    }
    

}

