using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

namespace MarcNet
{
    public class MARCField
    {
        /// <summary>
        /// This class is for managing MARC fileds.
        /// </summary>
        
        ///The tag name of the Field
        MARCTag _tag;

        // Value of the first indicator
        string _indicator1;

        // Value of the second indicator
        string _indicator2;
        
        // subfields
        subFieldsList _subfields;

        // warnings
        string _warn;

        // Value of field, if field is a Control field
        string _data;

        public MARCField(string tagNo, string indicator1, string indicator2, Hashtable subfields)
        {
            init();
            _tag = new MARCTag(tagNo);

            if (_tag._warn != null)
            {
                _warn = "It is not a valid tag.";
            }
            else
            {
                if (isControlField())
                {
                    addControlField(subfields);
                }
                else
                {

                    _indicator1 = indicator1;
                    if (!Regex.IsMatch(indicator1, "^[0-9A-Za-z ]$"))
                    {
                        _warn = "It is not a valid indicator 1.";
                    }

                    _indicator2 = indicator2;
                    if (!Regex.IsMatch(indicator2, "^[0-9A-Za-z ]$"))
                    {
                        _warn = "It is not a valid indicator 2.";
                    }

                    addSubFields(subfields);
                }
            }
        }

        public MARCField(string tagNo, int fieldLength, char[] fieldRaw)
        {

            init();
            
            _tag = new MARCTag(tagNo);

            string subfield;

            if (_tag._warn != null)
            {
                _warn = "It is not a valid tag.";
            }
            else
            {
                if (isControlField())
                {
                    char[] buff = new char[fieldRaw.Length -1];
                    Array.Copy(fieldRaw, buff, fieldRaw.Length-1);
                    subfield = new string(buff);

                    _data = subfield;
                }
                else
                {
                    _indicator1 = fieldRaw[0].ToString();
                    if (!Regex.IsMatch(_indicator1, "^[0-9A-Za-z ]$"))
                    {
                        _warn = "It is not a valid indicator 1.";
                    }

                    _indicator2 = fieldRaw[1].ToString();
                    if (!Regex.IsMatch(_indicator2, "^[0-9A-Za-z ]$"))
                    {
                        _warn = "It is not a valid indicator 2.";
                    }

                    _warn = divide2Subfiels(fieldLength, fieldRaw);
                }
            }
        }

        private void init()
        {
            _subfields = new subFieldsList();

            _warn = "";
            _indicator1 = "";
            _indicator2 = "";
            _data = "";
        }

        private string divide2Subfiels(int fieldLength, char[] fieldRaw)
        {
            char[] buff1;
            string subfield;

            int prev = 0;
            int next = 0;
            int len = 0;

            _warn = "";
            try
            {

                for (int i = 0; i < fieldLength; i++)
                {
                    if (fieldRaw[i] == MARCChar.SUBFIELD_INDICATOR)
                    {
                        if (prev == 0)
                        {
                            prev = i;
                        }
                        else if (next == 0)
                        {
                            next = i;
                            len = next - prev - 2;
                            buff1 = new char[len];
                            Array.Copy(fieldRaw, prev + 2, buff1, 0, len);
                            subfield = new string(buff1);

                            _subfields.addSubField(fieldRaw[prev + 1].ToString(), subfield);

                            prev = next;
                            next = 0;
                        }
                        else
                        {
                            prev = i;
                            next = 0;
                        }

                    }
                }

                if (prev > 0 && next == 0)
                {
                    next = fieldLength;
                    len = next - prev - 2;
                    buff1 = new char[len - 1];
                    Array.Copy(fieldRaw, prev + 2, buff1, 0, len - 1);
                    subfield = new string(buff1);

                    _subfields.addSubField(fieldRaw[prev + 1].ToString(), subfield);
                }
            }
            catch(Exception ex)
            {
                _warn = "Invalid subfields";
            }

            return _warn;
        }

        public int Length()
        {
            int len;

            if (this.isControlField())
            {
                len = _data.Length + 1;
            }
            else
            {
                len = _subfields.Count;

                int strLen = _subfields.Length;

                len = strLen + len + 3;

            }

            return len;

        }

        public MARCTag mTag
        {
            get
            {
                return _tag;
            }
        }

        public string tag
        {
            get
            {
                return _tag.tagStr;
            }

        }

        public string data
        {
            get
            {
                return _data;
            }
            set
            {
                _data = value;
            }

        }

        public string indicator1
        {
            get
            {
                return _indicator1;
            }
        }

        public string indicator2
        {
            get
            {
                return _indicator2;
            }
        }

        public string indicator(int no)
        {
            if (no == 1)
            {
                return _indicator1;
            }
            else
            {
                if (no == 2)
                {
                    return _indicator2;
                }
            }
            return "Invalid indicator";
        }


        public bool isControlField()
        {
            return _tag.isControl();

        }

        public bool isData()
        {
            return _tag.isData();
        }

        // get a subfield by code
        public string getSubField(string code)
        {
            if (this.isControlField())
            {
                return null;
            }
            else
            {
                if (_subfields.ContainsKey(code))
                {
                    return _subfields.getSubField(code);
                }
            }
            return null;

        }

        public int addSubFields(Hashtable subFields)
        {
            if (this.isControlField())
            {
                _warn = "Subfiles allowd only for tags bigger or equal to 10";
            }
            else
            {
                if (subFields.Count > 0)
                {
                    foreach (DictionaryEntry entry in subFields)
                    {
                        if (entry.Key.ToString().Length >= 1)
                        {
                            _subfields.addSubField(entry.Key.ToString().Substring(0, 1), entry.Value.ToString());
                        }
                        else
                        {
                            _warn = "error subfields";
                        }

                    }
                }
            }
            return subFields.Count;
        }

        private void addControlField(Hashtable subFields)
        {
            if (subFields.Count > 0)
            {
                foreach (DictionaryEntry entry in subFields)
                {
                    if (entry.Key.ToString().Length >= 1)
                    {
                        _data = _data + entry.Value.ToString();
                    }
                    else
                    {
                        _warn = "error subfields";
                    }

                }
            }

        }

        private void updateControlField(Hashtable subFields)
        {
            _data = "";
            addControlField(subFields);
        }

        private void removeControlField()
        {
            _data = "";
        }

        public subFieldsList getSubFields()
        {
            if (!this.isControlField())
            {
                return _subfields;
            }
            else
            {
                return null;
            }
        }

        private bool containsKey(string key)
        {
            return _subfields.ContainsKey(key);
        }

        private void replaceWith(MARCField field)
        {
            _data = field.data;
            _indicator1 = field.indicator1;
            _indicator2 = field.indicator2;
            _subfields = field.getSubFields();
            _tag = field.mTag;

        }

        private void replace(string key, string value)
        {
        }
        
        public bool update(Hashtable fields)
        {
            if (this.isControlField())
            {
                updateControlField(fields);
            }
            else
            {
                foreach (DictionaryEntry de in fields)
                {
                    if (de.Key.ToString() == "ind1")
                    {
                        _indicator1 = de.Value.ToString();
                    }
                    else if (de.Key.ToString() == "ind2")
                    {
                        _indicator2 = de.Value.ToString();
                    }
                    else if (containsKey(de.Key.ToString()))
                    {
                        _subfields.replaceSubField(de.Key.ToString(), de.Value.ToString());
                    }
                    else
                    {
                        _subfields.addSubField(de.Key.ToString(), de.Value.ToString());
                    }
                }
            }
            return true;

        }

        public void remove(string key)
        {
            _subfields.removeSubField(key);
        }

        public bool deleteSubFields(ArrayList fields)
        {
            if (this.isControlField())
            {
                removeControlField();
            }
            else
            {
                foreach (String de in fields)
                {
                    if (containsKey(de))
                    {
                        remove(de);
                    }
                }
            }
            return true;

        }

        public string asFormatted()
        {
            String pre, retStr;
            
            if (this.isControlField())
            {
                return String.Format("{0:3} {1}", _tag.tagStr, _data);
            }
            else
            {
                pre = String.Format("{0:3} {1:1}{2:1}", _tag.tagStr, _indicator1.ToString(), _indicator2.ToString());
            }
            retStr = _subfields.asFormatted();

            retStr = String.Format("{0:6}{1}", pre, retStr);

            return retStr;
        }

        public char[] asRaw()
        {
            char[] buff;
            int len;
            int strLen = 2;

            if (this.isControlField())
            {
                len = _data.Length + 1;
                buff = new char[len];

                _data.CopyTo(0, buff, 0, _data.Length);
                buff[_data.Length] = MARCChar.END_OF_FIELD;
            }
            else
            {

                char[] tempStr = _subfields.asRaw();
                len = tempStr.Length;

                buff = new char[len + 3];

                _indicator1.CopyTo(0, buff, 0, 1);
                _indicator2.CopyTo(0, buff, 1, 1);
                Array.Copy(tempStr, 0, buff, 2, len);
                buff[len+2] = MARCChar.END_OF_FIELD;

            }

            return buff;

        }

    }
}
