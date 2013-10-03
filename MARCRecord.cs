using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;


namespace MarcNet
{

    public class MARCRecord
    {
        // Leader of the Record
        MARCLeader _ldr;

        // Record Directory
        MARCRecordDirectory _dir;

        // Contain all MARCField objects of the Record
        ArrayList _fields;

        string _warning = "";
        char[] _buff;

        public MARCRecord()
        {
            init();
        }

        public MARCRecord(char[] rawRecord)
        {
            char[] temp = new char[MARCChar.LEADER_LEN];

            init();

            try
            {

                Array.Copy(rawRecord, temp, MARCChar.LEADER_LEN);
                _ldr = new MARCLeader(temp);

                int dirLength = _ldr.getBaseAddressOfData() - MARCChar.LEADER_LEN;
                char[] temp1 = new char[dirLength];
                Array.Copy(rawRecord, MARCChar.LEADER_LEN, temp1, 0, dirLength);
                _dir = new MARCRecordDirectory(temp1);

                MARCField field;
                ArrayList dirEntries = _dir.getEntries();

                foreach (MARCDirectoryEntry entry in dirEntries)
                {
                    temp1 = new char[entry.fieldLength];
                    Array.Copy(rawRecord, _ldr.getBaseAddressOfData() + entry.dataEnd, temp1, 0, entry.fieldLength);
                    field = new MARCField(entry.tagStr, entry.fieldLength, temp1);
                    addField(field);
                }
                //char[] dir = new 
                build_dir();
                build_Leader();
            }
            catch (Exception ex)
            {
                _warning = "Error!";
            }



        }

        private void init()
        {
            _fields = new ArrayList();

            _dir = new MARCRecordDirectory();

            _ldr = null;

            _warning = "";
        }

        public int Length
        {
            get
            {
                return _dir.getRecLen();
            }
        }

        public int BaseAddress
        {
            get
            {
                return _dir.getBaseAddress();
            }
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
            else
            {
                _ldr.update(getRecordLength(), getBaseAddress());
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

        public void addField(MARCField field)
        {
            _fields.Add(field);
            update();
        }

        private void update()
        {
            //build_dir();
            //updateLeader();
        }


        public MARCLeader leader
        {
            get
            {
                return _ldr;
            }
        }

        public MARCField getField(string fieldno)
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

        public ArrayList getFields(string fieldno)
        {
            ArrayList tempList = new ArrayList();
            MARCField retMARCField  = null;
            MARCField temp1         = null;
            IEnumerator myEn        = _fields.GetEnumerator();

            while (myEn.MoveNext())
            {
                temp1 = (MARCField) myEn.Current;
                if (temp1.tag == fieldno)
                {
                    retMARCField = temp1;
                    tempList.Add(retMARCField);
                }
                else
                {
                    continue;
                }
            }
            return tempList;
        }

        public subFieldsList getSubFields(string fieldno)
        {
            MARCField field = getField(fieldno);

            if (field != null)
            {
                return field.getSubFields();
            }
            return null;

        }

        public string getSubField(string fieldno, string subfieldno)
        {
            MARCField field = getField(fieldno);
            if (field != null)
            {
                return field.getSubField(subfieldno);
            }
            return null;
        }

        public void deleteField(string fieldno)
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
            update();
        }

        public void deleteFields(string fieldno)
        {
            MARCField field = getField(fieldno);
            while (field != null)
            {

                deleteField(fieldno);
                field = getField(fieldno);
            }
        }

        public ArrayList getAllFields()
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

        public void writeToFile(string fileName)
        {
            FileStream fs;
            fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);

            sw.Write(this.asRaw());
            sw.Close();
            fs.Close();

        }

    }
    

}

