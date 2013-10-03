using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;
using MarcNet;
using System.Text.RegularExpressions;

namespace testMarc01
{
    class Program
    {

        static ArrayList update856(ArrayList oldRec, ArrayList newRec)
        {
            ArrayList retUpdate856 = new ArrayList();
            ArrayList retUpdate8561 = new ArrayList();
            int UrlField = 0;
            int AllField = 0;
            int ZField = 0;
            int pos = 0;

            for (int j = 0; j < newRec.Count; j++)
            {
                retUpdate856.Add(newRec[j]);
            }

            for(int j = 0; j < oldRec.Count; j++)
            {
                MARCField oldField = (MARCField)oldRec[j];
                string old3field = oldField.getSubField("3");
                if (old3field == null)
                {
                    retUpdate856.Add(oldRec[j]);
                }
            }

 
            int Count = oldRec.Count + newRec.Count;
            for(int i=0; i<oldRec.Count; i++)
            {
                
                MARCField oldField = (MARCField)oldRec[i];
                string oldUfield = oldField.getSubField("u");
                string oldZfield = oldField.getSubField("z");

                for (int j = 0; j < newRec.Count; j++)
                {
                    MARCField newField = (MARCField)newRec[j];
                    string newUfield = newField.getSubField("u");
                    string newZfield = newField.getSubField("z");

                    // compare $u field
                    if (newUfield.CompareTo(oldUfield) == 0) 
                    {
                        // compare $z field
                        if (newZfield.CompareTo(oldZfield) == 0)
                        {
                            retUpdate856.Remove(oldField);
                            AllField++;

                        }
                        else
                        {
                            retUpdate856.Remove(oldField);
                            UrlField++;
                        }
                    }
                    else
                    {
                        if (newZfield.CompareTo(oldZfield) == 0)
                        {
                            retUpdate856.Remove(oldField);
                            ZField++;
                        }
      
                    }
                }

            }


            return retUpdate856;
        }

        static string compare(ArrayList oldRec, ArrayList newRec)
        {
            StringBuilder str = new StringBuilder();
            int UrlField = 0;
            int AllField = 0;
            int ZField = 0;

            for (int i = 0; i < oldRec.Count; i++)
            {
                MARCField oldField = (MARCField)oldRec[i];
                string oldUfield = oldField.getSubField("u");
                string oldZfield = oldField.getSubField("z");

                for (int j = 0; j < newRec.Count; j++)
                {
                    MARCField newField = (MARCField)newRec[j];
                    string newUfield = newField.getSubField("u");
                    string newZfield = newField.getSubField("z");

                    if (newUfield.CompareTo(oldUfield) == 0)
                    {
                        //str.Append("old:" + (i + 1).ToString() + "<-> new:" + (j + 1).ToString() + "==> $u ");

                        if (newZfield.CompareTo(oldZfield) == 0)
                        {
                            //str.Append("old:" + (i + 1).ToString() + "<-> new:" + (j + 1).ToString() + "==> $z");
                            AllField++;
                        }
                        else
                        {
                            UrlField++;
                        }
                    }
                    else
                    {
                        if (newZfield.CompareTo(oldZfield) == 0)
                        {
                            //str.Append("old:" + (i + 1).ToString() + "<-> new:" + (j + 1).ToString() + "==> $z");
                            ZField++;
                        }
                    }


                }
            }
            str.Append("$u field," + UrlField.ToString());
            str.Append(",All," + AllField.ToString());
            str.Append(",$z field," + ZField.ToString());
            str.Append(",Diff," + (oldRec.Count - newRec.Count).ToString());
            str.Append("\r\n");
            return str.ToString();
               
        }

        static void Main(string[] args)
        {
            BatchCat.UpdateBibReturnCode retCode;

            Hashtable fromEBZ = new Hashtable();
            string filename1 = "n:\\495833.bib";
            Voyager.myEbzRecords myEbzRecs = new Voyager.myEbzRecords(filename1, 0);
            
            ArrayList ebzlist = myEbzRecs.ebzList;

            Voyager.MyVoyagerRecords myVoyRec = new Voyager.MyVoyagerRecords();

            
            StringBuilder myResultStr = new StringBuilder();
            for (int i = 0; i < ebzlist.Count; i++)
            {
                retCode = BatchCat.UpdateBibReturnCode.ubUnknownError;

                Console.WriteLine("{0} -- {1}", i, ebzlist[i].ToString());
                myVoyRec.setVoyagerRecord(ebzlist[i].ToString());
                ArrayList voy856old = myVoyRec.retrieveOld856();
                
                ArrayList ebz856_1 = myEbzRecs.getEbz856(ebzlist[i].ToString());
                ArrayList ebz856New = myEbzRecs.generateNew856(ebz856_1);

                string compareResult = compare(voy856old, ebz856New);
                ArrayList ebz856Update = update856(voy856old, ebz856New);

                myResultStr.Append("\r\n================== "+ (i+1).ToString());
                myResultStr.Append("\t");
                myResultStr.Append(myVoyRec.BibID.ToString()+"\t");
                myResultStr.Append("\t");
                
                myResultStr.Append("From inukshuk: \t");
                myResultStr.Append(ebzlist[i].ToString()+"\t");
                myResultStr.Append(voy856old.Count.ToString());
                myResultStr.Append("\t");
                myResultStr.Append(ebz856New.Count.ToString());
                myResultStr.Append("\r\n");
                myResultStr.Append(compareResult);
                
                
                for(int j = 0; j < voy856old.Count; j ++)
                {
                    MarcNet.MARCField tempMarcField = (MarcNet.MARCField)voy856old[j];
                    myResultStr.Append( (j+1).ToString() + " :"+ tempMarcField.asFormatted() );
                    myResultStr.Append( "\r\n");
                }

                myResultStr.Append("From Ebsco: \r\n");
                for (int j = 0; j < ebz856New.Count; j++)
                {
                    MarcNet.MARCField tempMarcField = (MarcNet.MARCField)ebz856New[j];
                    myResultStr.Append((j + 1).ToString() + " :" + tempMarcField.asFormatted());
                    myResultStr.Append("\r\n");
                }
                myResultStr.Append("\r\n");

                myResultStr.Append("From Jason: \r\n");
                for (int j = 0; j < ebz856Update.Count; j++)
                {
                    MarcNet.MARCField tempMarcField = (MarcNet.MARCField)ebz856Update[j];
                    myResultStr.Append((j + 1).ToString() + " :" + tempMarcField.asFormatted());
                    myResultStr.Append("\r\n");
                }
                myResultStr.Append("\r\n");

                //DBConnected first, get bib record, then update
                if (myVoyRec.DbConnected)
                {
                    if (voy856old.Count > 0)
                    {

                        retCode = myVoyRec.update856(ebz856Update);
                    }
                }
                else
                {
                    if (myVoyRec.DbConnect())
                    {
                        if (voy856old.Count > 0)
                        {

                            retCode = myVoyRec.update856(ebz856Update);
                        }
                    }
                }
                myResultStr.Append("Update: " + retCode.ToString() + "\t\r\n");

                //fromEBZ.Add(ebzlist[i].ToString(), ebz856New);
            }
            
                
            string f1 = "c:\\New.mrc";
            FileStream fs1;
            fs1 = new FileStream(f1, FileMode.Create, FileAccess.Write);
            StreamWriter sw1 = new StreamWriter(fs1);
       
            sw1.Write(myResultStr);
            sw1.Close();
            fs1.Close();
            
        }

        void getBibRecord(long bib_ID)
        {
            
        }

    }

}
