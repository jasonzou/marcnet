using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;

namespace MarcNet
{
    public class MARCFile
    {
        byte[] _buff;
        FileStream _fs;
        BinaryReader _br;
        BinaryWriter _bw;
        ArrayList _records;
        ArrayList _MARCRecords;
        private int _Count;

        int _index;

        public MARCFile(string fileName)
        {

            _records = new ArrayList();
            _MARCRecords = new ArrayList();

            // check file exists or not
            _fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            _br = new BinaryReader(_fs);
            byte[] _buff1 = new byte[_fs.Length];
            _buff = new byte[_fs.Length];

            char c = '0';
            int i = 0;
            int j = 0;
            int len = (int)_fs.Length;
            //int numOfRecords = 0;
            _buff1 = _br.ReadBytes(len);
            _br.Close();

            _buff1.CopyTo(_buff, 0);

            j = numOfRecords();


            divide2RawRecords();


            if (_records != null)
            {
                decode();
            }
        }

        public int Count
        {
            get
            {
                return _Count;
            }
            set
            {
                _Count = value;
            }
        }

        private void divide2RawRecords()
        {
            int len = _buff.Length;
            int i = 0;
            byte c;
            int j = 0;
            int pos = 0;

            while (i < len)
            {
                c = _buff[i++];
                j++;

                if (c == MARCChar.END_OF_RECORD)
                {
                    char[] tempBuff = new char[j];
                    //Array.Copy(_buff, tempBuff, j);
                    Array.Copy(_buff, pos, tempBuff, 0, j);

                    _records.Add(tempBuff);

                    pos += j;
                    j = 0;

                }
            }
        }

        private int numOfRecords()
        {
            int i = 0;
            int len = _buff.Length;
            int numOfRecords = 0;
            byte c;

            while (i < len)
            {
                c = _buff[i++];

                if (c == MARCChar.END_OF_RECORD)
                {
                    numOfRecords++;
                }
            }
            Count = numOfRecords;

            return numOfRecords;
        }

        private void writeFile()
        {
            _fs = new FileStream("c:\\12.mrc", FileMode.OpenOrCreate, FileAccess.Write);

            _bw = new BinaryWriter(_fs);

            foreach (char[] test in _records)
            {
                //byte[] t = (byte [])test;

                byte[] t = new byte[test.Length];
                for (int i = 0; i < test.Length; i++)
                {
                    t[i] = (byte)test[i];
                }
                _bw.Write(t);
            }

            _bw.Close();
        }

        private void decode()
        {
            int num = 0;
            foreach (char[] field in _records)
            {
                
                MARCRecord record = new MARCRecord(field);
                if (record != null)
                {
                    _MARCRecords.Add(record);

                }
            }
        }

        public void addRecord(MARCRecord marcRec)
        {
            _MARCRecords.Add(marcRec);
        }

        public void writeToFile(string fileName)
        {
            _fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write);

            _bw = new BinaryWriter(_fs);
            /*
            foreach (MARCRecord record in _MARCRecords )
            {
                byte[] t = new byte[record.getRecordLength()];
                char[] test = record.asRaw();
                for (int i = 0; i < test.Length; i++)
                {
                    t[i] = (byte)test[i];
                }
                _bw.Write(t);
            }
            */
            _bw.Write(_buff);
            _bw.Close();
        }

        public string asFormatted()
        {
            string retStr = "";
            foreach (MARCRecord record in _MARCRecords)
            {
                retStr += record.asFormatted() + "\n\r";
            }
            return retStr;
        }

        public ArrayList MARCRecords()
        {
            return _MARCRecords;
        }

        public int num_records
        {
            get
            {
                return _MARCRecords.Count;
            }

        }


    }
}
