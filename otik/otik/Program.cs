using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics.Eventing.Reader;
using System.Diagnostics;
using System.Globalization;
using System.Security.AccessControl;
using Microsoft.SqlServer.Server;
using System.Runtime.Remoting;

namespace ConsoleApp1
{
    class Program
    {
        static int block_lendth = 10;
        static int buf_len = 9; //should be less than 10
        static int decimals = 8;
        static int N_pow = decimals;
        static int N = (int)Math.Pow(10, N_pow);
        static int header_length = 32;
        static int version = 1;
        static int lab = 5;
        static int c = 5; //compressiontypes count
        static bool no_lab = true; //ignore №lab
        static bool multy_compression = true;
        #region header_info
        static string get_header(bool[] compression, int source_length, int data_length, int subheader_length, int directories)
        {
            //header:
            //0 -3 -4b-sign
            //4 -5 -2b-version
            //6 -   1b-compression types
            //7 -   1b-lab
            //8 -   1b-files count
            //9 -15-8b-reserved
            //16-19-4b-source file length
            //20-23-4b-data length
            //24-27-4b-subheader length
            //28-31-4b-reserved
            string header = "Otik";
            int byte_ = 0;
            int pow = 1;
            for (int i = 0; i < c; i++)
            {
                if (compression[i])
                    byte_ += pow;
                pow *= 2;
            }
            header += (char)(version / 256) + "" + (char)(version % 256);
            header += (char)byte_;
            header += (char)lab;
            header += (char)directories;
            for (int i = 0; i < 7; i++)
                header += (char)0;
            header += (char)(source_length / 256 / 256 / 256) + "" + (char)((source_length / 256 / 256) % 256) + "" + (char)((source_length / 256) % 256) + "" + (char)(source_length % 256);
            header += (char)(data_length / 256 / 256 / 256) + "" + (char)((data_length / 256 / 256) % 256) + "" + (char)((data_length / 256) % 256) + "" + (char)(data_length % 256);
            header += (char)(subheader_length / 256 / 256 / 256) + "" + (char)((subheader_length / 256 / 256) % 256) + "" + (char)((subheader_length / 256) % 256) + "" + (char)(subheader_length % 256);
            for (int i = 0; i < 4; i++)
                header += (char)0;
            return header;
        }
        static bool check_header(string header)
        {
            //header:
            //0 -3 -4b-sign
            //4 -5 -2b-version
            //6 -   1b-compression types
            //7 -   1b-lab
            //8 -   1b-files count
            //9 -15-7b-reserved
            //16-19-4b-source file length
            //20-23-4b-data length
            //24-27-4b-subheader length
            //28-31-4b-reserved
            if (header.Substring(0, 4) == "Otik")
            {
                if ((int)header[4] * 256 + (int)header[5] <= version)
                {
                    if (((int)header[7] == lab) || no_lab)
                    {
                        if ((header.Substring(9, 7) == "\0\0\0\0\0\0\0") && ((header.Substring(28, 4) == "\0\0\0\0")))
                        {
                            return true;
                        }
                        else Console.WriteLine("unknown reserved was used");
                    }
                    else Console.WriteLine("i'm not in this lab");
                }
                else Console.WriteLine("unknown version");
            }
            else Console.WriteLine("unknown type");
            return false;
        }
        static bool[] get_compression_types(char types)
        {
            int t = (int)types;
            bool[] compression = new bool[c];
            int i = 0;
            while (t != 0)
            {
                if (t % 2 == 1)
                    compression[i] = true;
                i++;
                t /= 2;
            }
            return compression;
        }
        #endregion
        #region files
        static string read_file(string path)
        {
            string readText = "";
            if (File.Exists(path))
            {
                byte[] bytes = File.ReadAllBytes(path);
                foreach (byte b in bytes)
                {
                    readText += (char)b;
                }
            }
            return readText;
        }
        
        static string[] read_all(string path)
        {
            string[] directories = Directory.GetDirectories(path);
            string[] files = Directory.GetFiles(path);
            if (directories.Length != 0)
            {
                for (int i = 0; i < directories.Length; i++)
                    files = Add(files, read_all(directories[i]));
            }
            return files;
        }
        static string[] Add(string[] files, string[] add)
        {
            string[] res = new string[files.Length + add.Length];
            files.CopyTo(res, 0);
            add.CopyTo(res, files.Length);
            return res;
        }
        static void write_file(string path, string text)
        {
            byte[] bytes = new byte[text.Length];
            for (int i = 0; i < text.Length; i++)
            {
                bytes[i] = (byte)text[i];
            }
            File.WriteAllBytes(path, bytes);
        }
        static void files_in_archieve(string path)
        {
            string source;
            string header;
            string[] files;
            source = read_file(path);
            header = source.Substring(0, header_length);
            bool tmp = false;
            try
            {
                if (check_header(header))
                    tmp = true;
            }
            catch (Exception exc)
            {
                Console.WriteLine("no header in file (file too short)");
            }
            if (tmp)
            {
                int files_count = (int)header[8];
                if (files_count > 1)
                {
                    files = new string[files_count];
                    int j = header_length;
                    for (int i = 0; i < files_count; i++)
                    {
                        if (source.Substring(j, 5) == "path:")
                        {
                            j += 5;
                            while (source[j] != '|')
                            {
                                files[i] += source[j];
                                j++;
                            }
                            j++;
                            string len = "";
                            while (source[j] != '|')
                            {
                                len += source[j];
                                j++;
                            }
                            j += int.Parse(len) + 1;
                        }
                    }
                    for (int i = 0; i < files_count; i++)
                    {
                        Console.WriteLine(files[i]);
                    }
                }
                else
                {
                    Console.WriteLine(path.Substring(0, path.Length - 5));
                }
            }
        }
        #endregion 
        #region blocks
        static string block_compression(string text)
        {
            Random random = new Random();
            string data = "";
            for (int i = 0; i < text.Length;)
            {
                int hex = random.Next() % 16 + 1;
                if (i + hex >= text.Length)
                    hex = text.Length - i;
                if (hex < 10)
                    data += hex;
                else data += (char)('A' + (hex - 10));
                if (i + hex < text.Length)
                    data += text.Substring(i, hex);
                else
                    data += text.Substring(i);
                i += hex;
            }
            return data;
        }
        static string block_uncompression(string text, bool directories)
        {
            string data = "";
            for (int i = 0; i < text.Length - 2;)
            {
                int hex = ((text[i] - 'A') < 0) ?
                    ((int)(text[i] - 48)) :
                    (text[i] - 'A' + 10);
                if ((hex < 0) && (i + 2 == text.Length))
                { data += text.Substring(i, 2); break; }
                if (i + 1 + hex < text.Length)
                    data += text.Substring(i + 1, hex);
                else
                    data += text.Substring(i + 1);
                i += hex + 1;
            }
            return data;
        }
        #endregion
        #region Shenon_Fano
        static string Shenon_Fano_compression(string text, ref int subheader_length)
        {
            List<char> symbols = new List<char>();
            List<double> p = new List<double>();
            List<string> code = new List<string>();
            for (int i = 0; i < text.Length; i++)
            {
                if (!symbols.Contains(text[i]))
                { symbols.Add(text[i]); p.Add(1); code.Add(""); }
                else
                {
                    p[symbols.IndexOf(text[i])]++;
                }
            }
            for (int i = 0; i < p.Count; i++)
            {
                p[i] /= text.Length;
            }
            for (int i = 0; i < p.Count; i++)
            {
                for (int j = 0; j < p.Count; j++)
                {
                    if (p[i] > p[j])
                    {
                        p[i] = p[i] + p[j];
                        p[j] = p[i] - p[j];
                        p[i] = p[i] - p[j];
                        char t = symbols[i];
                        symbols[i] = symbols[j];
                        symbols[j] = t;
                    }
                }
            }
            for (int i = 0; i < p.Count; i++)
            {
                p[i] = Math.Round(p[i], decimals);
                //information
                //Console.WriteLine("I({0}): {1}", symbols[i], -Math.Log(p[i]) / Math.Log(2));
            }
            code = Shenon_Fano_find_code(symbols, p);
            string subheader = "";
            for (int i = 0; i < symbols.Count; i++)
            {
                subheader += symbols[i] + "" + p[i] + "|";
            }
            subheader_length = subheader.Length;
            string data = "";
            for (int i = 0; i < text.Length; i++)
            {
                data += code[symbols.IndexOf(text[i])];
            }
            data = BinaryToString(data);
            data = subheader + data;
            return data;
        }
        static string Shenon_Fano_uncompression(string text, int subheader_length, int source_length)
        {
            List<char> symbols = new List<char>();
            List<double> p = new List<double>();
            List<string> code = new List<string>();
            string tmp;
            int j = 0;
            for (int i = 0; i < subheader_length; i++)
            {
                symbols.Add(text[i]);
                i++;
                while (text[i] != '|')
                {
                    p.Add(read_number(text, ref i));
                    i++;
                }
                j++;
            }
            code = Shenon_Fano_find_code(symbols, p);
            text = StringToBinary(text.Substring(subheader_length));
            string data = "";
            j = 0;
            while (data.Length != source_length)
            {
                tmp = "";
                while (true)
                {
                    tmp += text[j];
                    if (code.Contains(tmp))
                    {
                        data += symbols[code.IndexOf(tmp)];
                        j++;
                        break;
                    }
                    j++;
                }
            }
            return data;
        }
        static List<string> Shenon_Fano_find_code(List<char> symbols, List<double> p)
        {
            List<string> code = new List<string>();
            List<string> code1 = new List<string>();
            List<string> code2 = new List<string>();
            double summ_p = 0;
            int i = 0;
            for (i = 0; i < p.Count; i++)
            {
                summ_p += p[i];
            }
            summ_p /= 2;
            for (i = 0; i < p.Count; i++)
            {
                summ_p -= p[i];
                if (summ_p < 0)
                {
                    if ((i % 2 == 0) && (i != 0))
                        i--;
                    break;
                }
            }
            if (symbols.Count > 2)
            {
                code1 = Shenon_Fano_find_code(symbols.GetRange(0, i + 1), p.GetRange(0, i + 1));
                for (int j = 0; j < code1.Count; j++)
                    code1[j] = "0" + code1[j];
                code2 = Shenon_Fano_find_code(symbols.GetRange(i + 1, symbols.Count - i - 1), p.GetRange(i + 1, symbols.Count - i - 1));
                for (int j = 0; j < code2.Count; j++)
                    code2[j] = "1" + code2[j];
            }
            code = new List<string>();
            if (p.Count == 1)
            {
                code.Add("0");
            }
            else
            {
                if (p.Count == 2)
                {
                    code.Add("0");
                    code.Add("1");
                }
                else
                {
                    for (i = 0; i < p.Count; i++)
                    {
                        if (i < code1.Count)
                        {
                            code.Add(code1[i]);
                        }
                        else
                        {
                            code.Add(code2[i - code1.Count]);
                        }
                    }
                }
            }
            return code;
        }
        #endregion  
        #region Arithmetic_Compression
        static string Arithmetic_compression(string text, ref int subheader_length)
        {
            List<char> symbols = new List<char>();
            List<double> p = new List<double>();
            for (int i = 0; i < text.Length; i++)
            {
                if (!symbols.Contains(text[i]))
                { symbols.Add(text[i]); p.Add(1); }
                else
                {
                    p[symbols.IndexOf(text[i])]++;
                }
            }
            for (int i = 0; i < p.Count; i++)
            {
                p[i] /= text.Length;
            }
            for (int i = 0; i < p.Count; i++)
            {
                for (int j = 0; j < p.Count; j++)
                {
                    if (p[i] > p[j])
                    {
                        p[i] = p[i] + p[j];
                        p[j] = p[i] - p[j];
                        p[i] = p[i] - p[j];
                        char t = symbols[i];
                        symbols[i] = symbols[j];
                        symbols[j] = t;
                    }
                }
            }
            for (int i = 0; i < p.Count; i++)
            {
                p[i] = Math.Round(p[i], decimals);
                //information
                //Console.WriteLine("I({0}): {1}", symbols[i], -Math.Log(p[i]) / Math.Log(2));
                // Console.WriteLine(symbols[i] + " " + p[i]);
            }
            string subheader = "";
            for (int i = 0; i < symbols.Count; i++)
            {
                subheader += symbols[i] + "" + p[i] + "|";
            }
            subheader_length = subheader.Length;
            double summ = 0;
            for (int i = 1; i < p.Count; i++)
            {
                p[i] = p[i] + p[i - 1];
            }
            for (int i = 0; i < p.Count; i++)
            {
                p[i] = Math.Round(p[i], decimals);
                //Console.WriteLine(symbols[i] + "\t" + p[i]);
            }
            summ = p[p.Count - 1];
            //Console.WriteLine("s" + summ);
            string data = "";
            int left = 0, right = (int)(N * summ - 1);
            foreach (char c in text)
            {
                int k = symbols.IndexOf(c);
                int l = right - left;
                int tl = left;
                left = (int)(tl + (l + 1) * ((k == 0) ? 0 : p[k - 1]));
                right = (int)(tl + (l + 1) * p[k] - 1);
                string sl = left.ToString();
                string sr = right.ToString();
                int i = 0;
                while (sl[i] == sr[i])
                {
                    data += (right * 10 / N);
                    left = (left % (N / 10) * 10);
                    right = (right % (N / 10) * 10) + 9;
                    i++;
                }
            }
            data += left;
            //Console.WriteLine(data);
            data = InttoBinary(data);
            //Console.WriteLine(data);
            data = BinaryToString(data);
            //Console.WriteLine(data);
            data = subheader + data;
            return data;
        }
        static string Aritmetic_uncompression(string text, int subheader_length, int source_length)
        {
            List<char> symbols = new List<char>();
            List<double> p = new List<double>();
            string tmp;
            int j = 0;
            for (int i = 0; i < subheader_length; i++)
            {
                symbols.Add(text[i]);
                i++;
                while (text[i] != '|')
                {
                    p.Add(read_number(text, ref i));
                    i++;
                }
                j++;
            }
            double summ = 0;
            for (int i = 0; i < p.Count; i++)
            {
                summ += p[i];
            }
            for (int i = 0; i < p.Count; i++)
            {
                if (i > 0)
                    p[i] = p[i] + p[i - 1];
                //Console.WriteLine(symbols[i] + " " + p[i]);
            }
            for (int i = 0; i < p.Count; i++)
            {
                p[i] = Math.Round(p[i], decimals);
                //Console.WriteLine(symbols[i] + "\t" + p[i]);
            }
            //text = StringToBinary(text.Substring(subheader_length));
            text = text.Substring(subheader_length);
            //Console.WriteLine(text);
            text = StringToBinary(text);
            //Console.WriteLine(text);
            text = BinarytoInt(text);
            //Console.WriteLine(text);

            string data = "";
            int left = 0, right = (int)(N * summ - 1);
            data = "";
            //Console.WriteLine("s" + summ);
            bool flag = false;
            double code = double.Parse(text.Substring(0, N_pow)) / (double)N;
            int l = 0;
            while ((data.Length != source_length))
            {
                if (flag)
                {
                    if (N_pow + l < text.Length)
                        code = double.Parse(text.Substring(l, N_pow)) / (double)N;
                    else
                        code = double.Parse(text.Substring(l)) / N;
                    code = (code * N - left) / (right - left);
                    flag = false;
                }
                for (int i = 0; i < p.Count; i++)
                {
                    double tl, tr;
                    if (i == 0)
                        tl = 0;
                    else
                        tl = p[i - 1];
                    tr = p[i];
                    if ((code >= tl) && (code < tr))
                    {
                        data += symbols[i];
                        code = ((code - tl)) / (tr - tl);
                        int len = right - left;
                        tl = left;
                        left = (int)(tl + (len + 1) * ((i == 0) ? 0 : p[i - 1]));
                        right = (int)(tl + (len + 1) * p[i] - 1);
                        string sl = left.ToString();
                        string sr = right.ToString();
                        int kk = 0;
                        while (sl[kk] == sr[kk])
                        {
                            left = (left % (N / 10) * 10);
                            right = (right % (N / 10) * 10) + 9;
                            flag = true;
                            l++;
                            kk++;
                        }
                        break;
                    }
                }
            }
            //Console.WriteLine(data);
            return data.Substring(0, source_length);
        }
        #endregion
        #region LZ77
        static string LZ77_compression(string text)
        {
            List<string> res = new List<string>();

            string data = "";
            res.Add("0,0," + text[0].ToString());
            string buffer = "" + text[0];
            int k;
            int b = 0;
            for (int i = 1; i < text.Length; i++)
            {
                b = 0;
                k = 0;
                char c = ' ';
                for (int j = 0; j < buffer.Length; j++)
                {
                    if (text[i] == buffer[j])
                    {
                        k = 1;
                        b = j + 1;
                        if (i + k >= text.Length)
                            break;
                        if (j + k >= buffer.Length)
                            break;
                        while (text[i + k] == buffer[j + k])
                        {
                            k++;
                            if (i + k >= text.Length)
                                break;
                            if (j + k >= buffer.Length)
                                break;
                        }
                        b = j + 1;
                        break;
                    }
                }
                if (i + k < text.Length)
                {
                    c = text[i + k];
                    buffer += text.Substring(i, k + 1);
                    res.Add(b + "," + k + "," + c);
                    i += k;
                }
                else
                {
                    if (i >= text.Length)
                        res.Add(b + "," + k + "," + "END");
                    else
                        res.Add(b + "," + (k - 1) + "," + text[text.Length - 1]);
                    break;
                }
                if (buffer.Length >= buf_len)
                {
                    if (i + 2 < text.Length)
                        buffer = text.Substring(i - buf_len + 1, buf_len);
                    //Console.WriteLine(buffer);
                }
            }
            foreach (string s in res)
            {
                //Console.WriteLine(s);
                data += s;
            }
            data = LZtoString(data);
            return data;
        }
        static string LZ77_uncompression(string text)
        {
            string data = "";
            string buffer = "";
            int j = 0;
            bool end = false;
            text = StringtoLZ(text);
            for (int i = 0; i < text.Length; i++)
            {
                int ind = int.Parse(text[i] + "");
                i += 2;
                int len = int.Parse(text[i] + "");
                i += 2;
                char symb = text[i];
                if (text.Substring(i) == "END")
                    end = true;
                if (!((len == 0) && (ind == 0)))
                {
                    j = ind - 1;
                    for (int k = 0; k < len; k++, j++)
                    {
                        data += buffer[j];
                        buffer += buffer[j];
                    }
                }
                if (end)
                    break;
                else
                {
                    data += symb;
                    buffer += symb;
                }
                while (buffer.Length > buf_len)
                    buffer = buffer.Substring(1);
            }
            return data;
        }
        #endregion
        #region Hemming
        static string Hemming_cript_block(string text, int block_lendth)
        {
            string data = "";
            for (int i = 0; i < text.Length / block_lendth; i++)
            {
                data += Hemming_cript(text.Substring(i * block_lendth, block_lendth));
            }
            data += Hemming_cript(text.Substring((text.Length / block_lendth) * block_lendth));
            return data;
        }
        static string Hemming_encript_block(string text, int block_lendth)
        {
            string data = "";
            int r = 1;
            int k = 0;
            while (r < block_lendth)
            {
                r <<= 1;
                k++;
            }
            int l = 0;
            for (int i = 0; i < text.Length / (block_lendth + k); i++)
            {
                data += Hemming_encript(text.Substring(i * (block_lendth + k), block_lendth + k), i);
                l += block_lendth + k;
            }
            data += Hemming_encript(text.Substring(l), text.Length / block_lendth + 1);
            return data;
        }
        static string Hemming_cript(string text)
        {
            string data = "";
            int control_bits_count = 0;
            int k = 0;
            string r = "";
            for (int i = 0; ; i++)
            {
                data += "0";
                if (k + (int)Math.Pow(2, i) - 1 < text.Length)
                {
                    control_bits_count++;
                    data += text.Substring(k, (int)Math.Pow(2, i) - 1);
                }
                else
                {
                    data += text.Substring(k);
                    break;
                }
                k += (int)Math.Pow(2, i) - 1;
            }
            for (int j = 0; j < control_bits_count+1; j++)
            {
                k = 0;
                for (int i = 0; i < data.Length; i++)
                {
                    k += ((i + 1) >> j) % 2 * (data[i] - 48);
                }
                k %= 2;
                r += k.ToString();
            }
            data = "";
            k = 0;
            for (int i = 0; i < control_bits_count+1; i++)
            {
                data += r[i];
                if (k + (int)Math.Pow(2, i) - 1 < text.Length)
                    data += text.Substring(k, (int)Math.Pow(2, i) - 1);
                else
                    data += text.Substring(k);
                k += (int)Math.Pow(2, i) - 1;
            }
            return data;
        }
        static string Hemming_encript(string text,int block)
        {
            string data = "";
            int control_bits_count = 0;
            while (text.Length > Math.Pow(2, control_bits_count))
                control_bits_count++;
            control_bits_count--;
            int k = 0;
            int ind = 0;
            for (int j = 0; j < control_bits_count; j++)
            {
                k = 0;
                for (int i = 0; i < text.Length; i++)
                {
                    k += ((i + 1) >> j) % 2 * (text[i] - 48);
                }
                k %= 2;
                ind += k << j;
            }
            if (ind != 0)
            {
                Console.WriteLine("perhaps noise in {0} bit in {1} block", ind, block);
            }
            k = 1;
            for (int i = 1; i <= text.Length; i++)
            {
                if (i == k)
                    k <<= 1;
                else
                {
                    if (i == ind)
                        data += (char)((text[i - 1] - 48 + 1) % 2 + 48);
                    else data += text[i-1];
                }
            }
            return data;
        }
        #endregion
        #region sub_functions
        static double read_number(string str, ref int st)
        {
            string num_str = "0123456789,E-";
            string num = "";
            while (num_str.Contains(str[st]))
            {
                num += str[st];
                st++;
                if (st >= str.Length) break;
            }
            st--;
            return double.Parse(num);
        }
        static string StringToBinary(string data)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in data.ToCharArray())
            {
                int i = (int)c;
                string tmp = "";
                while (i != 0)
                {
                    if (i % 2 == 1)
                        tmp = "1" + tmp;
                    else
                        tmp = "0" + tmp;
                    i /= 2;
                }
                sb.Append(tmp.PadLeft(8, '0'));
            }
            return sb.ToString();
        }
        public static string BinaryToString(string data)
        {
            string str = "";
            for (int i = 0; i < data.Length; i += 8)
            {
                string tmp = "";
                int c = 0;
                int pow = 1;
                if (i + 8 < data.Length)
                    tmp = data.Substring(i, 8);
                else
                    tmp = data.Substring(i).PadRight(8, '0');
                for (int j = tmp.Length - 1; j > -1; j--)
                {
                    if (tmp[j] == '1')
                        c += pow;
                    pow *= 2;
                }
                str += (char)c;
            }
            return str;
        }
        public static string InttoBinary(string data)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in data.ToCharArray())
            {
                int i = c - 48;
                string tmp = "";
                while (i != 0)
                {
                    if (i % 2 == 1)
                        tmp = "1" + tmp;
                    else
                        tmp = "0" + tmp;
                    i /= 2;
                }
                sb.Append(tmp.PadLeft(4, '0'));
            }
            while (sb.Length % 8 != 0)
                sb.Append('0');
            return sb.ToString();
        }
        public static string BinarytoInt(string data)
        {
            string str = "";
            for (int i = 0; i < data.Length; i += 4)
            {
                string tmp = "";
                int c = 0;
                int pow = 1;
                if (i + 4 < data.Length)
                    tmp = data.Substring(i, 4);
                else
                    tmp = data.Substring(i).PadRight(4, '0');
                for (int j = tmp.Length - 1; j > -1; j--)
                {
                    if (tmp[j] == '1')
                        c += pow;
                    pow *= 2;
                }
                str += (char)(c + 48);
            }
            return str;
        }
        public static string LZtoString(string data)
        {

            string str = "";
            bool end = false;
            for (int i = 0; i < data.Length; i++)
            {
                int ind = int.Parse(data[i] + "");
                i += 2;
                int len = int.Parse(data[i] + "");
                i += 2;
                char symb = data[i];
                if (data.Substring(i) == "END")
                    end = true;
                else
                {
                    for (int j = 3; j >= 0; j--)
                        str += (ind >> j) % 2;
                    for (int j = 3; j >= 0; j--)
                        str += (len >> j) % 2;
                }
                if (end)
                {
                    for (int j = 3; j >= 0; j--)
                        str += (ind >> j) % 2;
                    for (int j = 3; j >= 0; j--)
                        str += (len >> j) % 2;
                    str += StringToBinary("END");
                    break;
                }
                str += StringToBinary(symb + "");
            }
            str = BinaryToString(str);
            return str;
        }
        public static string StringtoLZ(string data)
        {
            string str = "";
            data = StringToBinary(data);
            int i = 0;
            while (true)
            {
                string ind = BinarytoInt(data.Substring(i, 4));
                string len = BinarytoInt(data.Substring(i+4, 4));
                string symbol = BinaryToString(data.Substring(i + 8, 8));
                if (data.Length - 24 > i)
                    if (symbol == "E")
                        if (BinaryToString(data.Substring(i + 8 + 8, 8)) == "N")
                            if (BinaryToString(data.Substring(i + 8 + 8 + 8, 8)) == "D")
                            { str += ind + "," + len + "," + "END"; break; }
                str += ind + "," + len + "," + symbol;
                i += 16;
                if (i > data.Length - 16)
                    break;
            }
            return str;
        }
        #endregion
        static void compression(string path, string file)
        {
            char k;
            int source_length;
            string source = "";
            string header;
            string[] files = new string[1];
            bool tmp = true;
            bool[] compressiontypes = new bool[c];
            int subheader_length = 0;
            source = read_file(path + file);
            try
            {
                if (check_header(source.Substring(0, header_length)))
                    tmp = false;
            }
            catch (Exception exc) { }
            if (multy_compression || tmp)
            {
                Console.WriteLine("write compression type\n"
                + "1 - block compression (lab3)\n"
                + "2 - no context compression (separable code|Shenon-Fano)\n"
                + "3 - no context compression (arithmetical code)\n"
                + "4 - context compression\n"
                + "5 - Anti-interference coding\n"
                + "0 - no compression\n");
                while (true)
                {
                    k = Console.ReadKey().KeyChar;
                    if ("012345".Contains(k))
                    {
                        for (int i = 0; i < c; i++)
                        {
                            if (i == k - 48 - 1)
                                compressiontypes[i] = true;
                            else
                                compressiontypes[i] = false;
                        }
                        break;
                    }
                }
                Console.WriteLine();
                source = read_file(path + file);
                //directories = true;
                if (source == "")
                {
                    files = read_all(path + file);
                    for (int i = 0; i < files.Length; i++)
                    {
                        files[i] = files[i].Substring(path.Length + 1);
                        Console.WriteLine(files[i]);
                    }
                }
                else
                    files[0] = file;
                source_length = source.Length;
                //if (directories == false)
                //{
                //    if (k=='1')
                //    {
                //        Console.WriteLine("block_file");
                //        source = block_compression(source);
                //    }
                //    if (k=='2')
                //    {
                //        Console.WriteLine("Shenon-Fano_file");
                //        source = Shenon_Fano_compression(source, ref subheader_length);
                //    }
                //    if (k == '3')
                //    {
                //        Console.WriteLine("Arithmetic_file");
                //        source = Arithmetic_compression(source, ref subheader_length);
                //    }
                //    if (k == '4')
                //    {
                //        Console.WriteLine("LZ77_file");
                //        source = LZ77_compression(source);
                //    }
                //    if (k == '5')
                //    {
                //        Console.WriteLine("Hemming_file");
                //        source = BinaryToString(Hemming_cript(StringToBinary(source)));
                //    }
                //    Console.WriteLine("\nsource length: " + source_length + "\ndata length: " + source.Length);
                //    header = get_header(compressiontypes, source_length, source.Length, subheader_length, 0);
                //    Console.WriteLine("header: " + header);
                //    source = header + source;
                //}
                //if (directories == true)

                string data = "";
                for (int i = 0; i < files.Length; i++)
                {
                    source = read_file(path + "\\" + files[i]);
                    source_length += source.Length;
                    string temp = source;
                    data += "path:" + files[i] + "|";
                    if (k == '1')
                    {
                        Console.WriteLine("block_file");
                        temp = block_compression(source);
                    }
                    if (k == '2')
                    {
                        Console.WriteLine("Shenon-Fano_file");
                        int templen = temp.Length; // source_len
                        temp = Shenon_Fano_compression(temp, ref subheader_length);
                        data += subheader_length + "|" + templen + "|";
                    }
                    if (k == '3')
                    {
                        Console.WriteLine("Arithmetic_file");
                        int templen = temp.Length; // source_len
                        temp = Arithmetic_compression(temp, ref subheader_length);
                        data += subheader_length + "|" + templen + "|";
                    }
                    if (k == '4')
                    {
                        Console.WriteLine("LZ77_file");
                        temp = LZ77_compression(source);
                    }
                    if (k == '5')
                    {
                        Console.WriteLine("Hemming_file");
                        temp = BinaryToString(Hemming_cript_block(StringToBinary(source), block_lendth));
                    }
                    data += temp.Length + "|" + temp;
                    Console.WriteLine("\nsource length: " + source.Length + "\ndata length: " + (data.Length - subheader_length));
                }
                header = get_header(compressiontypes, source_length, data.Length, 0, files.Length);
                Console.WriteLine("header: " + header);
                source = header + data;
                write_file(path + file + ".Otik", source);
            }
            else
            {
                Console.WriteLine("no multy compression");
            }
        }
        static void uncompression(string path, string file)
        {
            string source;
            string header = "";
            string[] files;
            bool[] compressiontypes;
            source = read_file(path + file);
            Console.WriteLine("\nsource length" + source.Length);
            bool tmp = false;
            try
            {
                header = source.Substring(0, header_length);
                if (check_header(header))
                    tmp = true;
            }
            catch (Exception exc)
            {
                Console.WriteLine("no header in file (file too short)");
            }
            if (tmp)
            {
                compressiontypes = get_compression_types(header[6]);
                int files_count = (int)header[8];
                //header
                //path:<path>|datalength|text
                //path:<path>|subheader length|sourcelength|datalength|text
                files = new string[files_count];
                string[] data = new string[files_count];
                int[] subheader_length = new int[files_count];
                int[] source_length = new int[files_count];
                int j = header_length;
                for (int i = 0; i < files_count; i++)
                {
                    if (source.Substring(j, 5) == "path:")
                    {
                        string len = "";
                        j += 5;
                        while (source[j] != '|')
                        {
                            files[i] += source[j];
                            j++;
                        }
                        j++;
                        if (compressiontypes[1] || compressiontypes[2]) 
                        {
                            len = "";
                            while (source[j] != '|')
                            {
                                len += source[j];
                                j++;
                            }
                            subheader_length[i] = int.Parse(len);
                            j++;
                            len = "";
                            while (source[j] != '|')
                            {
                                len += source[j];
                                j++;
                            }
                            source_length[i] = int.Parse(len);
                            j++;
                        }
                        len = "";
                        while (source[j] != '|')
                        {
                            len += source[j];
                            j++;
                        }
                        j++;
                        data[i] = source.Substring(j, int.Parse(len));
                        j += int.Parse(len);
                    }
                }
                for (int i = 0; i < files_count; i++)
                {
                    Console.WriteLine(files[i]);
                    if (compressiontypes[0])
                        data[i] = block_uncompression(data[i], true);
                    if (compressiontypes[1])
                    {
                        data[i] = Shenon_Fano_uncompression(data[i], subheader_length[i], source_length[i]);
                    }
                    if (compressiontypes[2])
                    {
                        data[i] = Aritmetic_uncompression(data[i], subheader_length[i], source_length[i]);
                    }
                    if (compressiontypes[3])
                    {
                        data[i] = LZ77_uncompression(data[i]);
                    }
                    if (compressiontypes[4])
                    {
                        data[i] = BinaryToString(Hemming_encript_block(StringToBinary(data[i]), block_lendth));
                    }
                    j = files[i].Length - 1;
                    while (files[i][j] != '\\')
                    {
                        if (j == 0)
                            break;
                        j--;
                    }
                    if (j != 0)
                        Directory.CreateDirectory(path + file.Substring(0, file.Length - 5) + "\\" + files[i].Substring(0, j));
                    if (files_count == 1)
                    {
                        Directory.CreateDirectory(path);
                        write_file(path + "\\" + files[i], data[i]);
                    }
                    else
                    {
                        Directory.CreateDirectory(path + file.Substring(0, file.Length - 5));
                        write_file(path + file.Substring(0, file.Length - 5) + "\\" + files[i], data[i]);

                    }
                }

                //else write_file(path.Substring(0, path.Length - 5), source.Substring(header_length));
            }
            else { Console.WriteLine("unknown header"); }
        }
        static void Main(string[] args)
        {
            string path, file;
            //path = @"C:\Users\8170863\source\repos\ConsoleApp1\ConsoleApp1\bin\Debug\ConsoleApp2.exe.Otik";
            path = @"D:\4\Tafya1\";
            file = "arith0.txt.Otik";
            char k;
            while (true)
            {
                Console.WriteLine("Current path: " + path);
                Console.WriteLine("Current file: " + file);
                Console.WriteLine("0-compression\n" +
                    "1-uncompression\n" +
                    "2-enter new path\n" +
                    "3-enter new file\n" +
                    "4-show archieved files\n" +
                    "e-exit\n");
                k = Console.ReadKey().KeyChar;
                switch (k)
                {
                    case 'e':
                        return;
                    case '0':
                        compression(path, file);
                        break;
                    case '1':
                        uncompression(path, file);
                        break;
                    case '2':
                        path = Console.ReadLine();
                        break;
                    case '3':
                        file = Console.ReadLine();
                        break;
                    case '4':
                        files_in_archieve(path + file);
                        break;
                }
            }
        }
    }
}
