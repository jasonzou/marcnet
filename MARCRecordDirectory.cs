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
            _buff = dirRaw;
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
            _buff[len - 1] = MARCChar.END_OF_FIELD;
            return _buff;

        }

        public void Clear()
        {
            _dir.Clear();
            _entries = 0;
            _recLength = 0;
            _baseAddress = 0;
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
            return _dir.Count * MARCChar.DIRECTORY_ENTRY_LEN + 1;
        }

        private void update()
        {
            _baseAddress = getBaseAddress();
            _recLength = _baseAddress + fieldLength() + 1;

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
            update();
        }

        public void deleteEntry(MARCDirectoryEntry entry)
        {
            _dir.Remove(entry);
            update();
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
            _tag = tagno;
            _len = len;
            _dataEnd = dataEnd;
            _buff = new char[MARCChar.DIRECTORY_ENTRY_LEN];

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
            int retNum = 0; ;

            for (int i = 0; i < len; i++)
            {
                int temp = (int)(charArray[i] - '0') * (int)Math.Pow(10, (len - i - 1));
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
}