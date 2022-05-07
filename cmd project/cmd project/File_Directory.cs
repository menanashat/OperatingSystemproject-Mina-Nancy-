﻿using System;
using System.Collections.Generic;
using System.Text;

namespace cmd_project
{
    class File_Directory : Directory_Entry
    {
        public dirctory parent;
        public string content;
        byte[] bytes;
        public File_Directory(char[] file_Names, byte File_Attribute, int file_first_Cluster, int file_Sizes, dirctory parent, string contents) : base(file_Names, File_Attribute, file_first_Cluster, file_Sizes)
        {
            if (parent != null)
            {
                this.parent = parent;
            }
            content = contents;
            bytes = Encoding.ASCII.GetBytes(content);
        }

        public void Write_File()
        {

            double Num_of_Required_Blocks = System.Math.Ceiling(Convert.ToDouble(bytes.Length / 1024));

            double NUM_of_Full_size_Blocks = System.Math.Floor(Convert.ToDouble(bytes.Length / 1024));


            int reminder = bytes.Length % 1024;

            if (Num_of_Required_Blocks <= Fat_Table.get_available_Blocks())
            {
                int Fat_Index;
                int last_Index = -1;
                if (file_first_Cluster != 0)
                {//5 root
                    Fat_Index = file_first_Cluster;
                }
                else
                {
                    //each 1024 byte    
                    Fat_Index = Fat_Table.getavailable_Block();
                    file_first_Cluster = Fat_Index;
                }
                    List<byte[]> LS = new List<byte[]>();


                    byte[] b = new byte[1024];
                    for (int i = 0; i < Num_of_Required_Blocks; i++)
                    {
                        for (int j = i * 1024, c = 0; c < 1024; j++, c++)
                        {
                            b[c] = bytes[j];
                        }
                        LS.Add(b);

                    }

                    byte[] b2 = new byte[1024];
                    for (int i = (int)NUM_of_Full_size_Blocks * 1024, c = 0; c < reminder; i++, c++)
                    {
                        bytes[i] = b2[c];
                    }

                    for (int i = 0; i < NUM_of_Full_size_Blocks; i++)
                    {

                        Virtual_Disk.Write_Cluster(Fat_Index, LS[i]);
                        Fat_Table.set_Next_Block(Fat_Index, -1);
                        if (last_Index != -1)
                        {
                            Fat_Table.set_Next_Block(last_Index, Fat_Index);
                        }
                        last_Index = Fat_Index;
                        Fat_Index = Fat_Table.getavailable_Block();
                    }
                    Fat_Table.Write_FAT();

                
            }
        }
        public void Read_File()
        {
            List<Directory_Entry> Directory_Table = new List<Directory_Entry>();
            if (file_first_Cluster != 0 && Fat_Table.get_Next_Bloack(file_first_Cluster) != 0)
            {
                List<byte> ls = new List<byte>();
                int Fat_index = file_first_Cluster;
                int next = Fat_Table.get_Next_Bloack(Fat_index);
                do
                {
                    byte[] a = new byte[1024];
                    a = Virtual_Disk.read_Cluster(Fat_index);
                    ls.AddRange(a);
                    Fat_index = next;
                    if (Fat_index != -1)
                    {
                        next = Fat_Table.get_Next_Bloack(Fat_index);
                    }
                } while (next != -1);
                byte[] b = new byte[32];
                for (int i = 0; i < ls.Count; i++)
                {

                    for (int j = i * 32, c = 0; c < 32; j++, c++)
                    {
                        b[c] = ls[j];
                    }
                    Directory_Table.Add(getDirector_entry(b));
                }
                for (int i = 0; i < ls.Count; i++)
                {
                    b[i % 32] = ls[i];
                    if ((i + 1) % 32 == 0)
                    {
                        Directory_Entry d = this.getDirector_entry(b);
                        if (d.file_Name[0] != '\0')
                        {
                            Directory_Table.Add(d);
                        }
                    }
                }
            }
        }
        //}

        public void Delete_file()
        {
            int Fat_index = file_first_Cluster;
            if (file_first_Cluster != 0)
            {
                int next = Fat_Table.get_Next_Bloack(Fat_index);
                do
                {
                    Fat_Table.set_Next_Block(Fat_index, 0);
                    Fat_index = next;
                    if (Fat_index != -1)
                    {
                        next = Fat_Table.get_Next_Bloack(Fat_index);
                    }

                } while (next != -1);
            }
            if (parent != null)
            {
                parent.Read_Directory();
                string s = string.Join("", file_Name);

                Fat_index = parent.searchDirectory(s);

                if (Fat_index != -1)
                {

                    parent.Directory_Table.RemoveAt(Fat_index);
                    parent.Write_Directory();
                }

                Fat_Table.Write_FAT();
            }

        }

    }





}
