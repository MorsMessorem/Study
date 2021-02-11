﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Tafya
{
    class Lab1
    {
        static string mes = "no";
        static double read_number(string str, ref int st, bool t)
        {
            string num_str = "0123456789.";
            string num = "";
            if (t)
                while (num_str.Contains(str[st]))
                {
                    if (str[st] == '.')
                        num += ',';
                    else
                        num += str[st];
                    st++;
                    if (st >= str.Length) break;
                }
            else
                while ((num_str + ",").Contains(str[st]))
                {
                    num += str[st];
                    st++;
                    if (st >= str.Length) break;
                }
            st--;
            return double.Parse(num);
        }
        static bool validation(string s, string math_word)
        {
            int k = 0;
            string valid = math_word + "1234567890-+*/,.()";
            for (int i = 0; i < s.Length; i++)
            {
                if (!valid.Contains(s[i]))
                { Console.WriteLine("uncorrect input 'unknown symbol'"); return false; }
                if (s[i] == ')')
                { k--; continue; }
                if (s[i] == '(')
                { k++; continue; }
                if (((s[i] == '.') || (s[i] == ',')) && (i < s.Length - 1))
                    if ((s[i + 1] == '.') || (s[i + 1] == ','))
                    { Console.WriteLine("uncorrect input '..'"); return false; }
                if (s[i] == math_word[0])
                {
                    for (int j = 1; j < math_word.Length; j++)
                    {
                        if (!(i + j < s.Length - 1))
                        { Console.WriteLine("uncorrect input 'unknown word'"); return false; }
                        if (s[i + j] != math_word[j])
                        { Console.WriteLine("uncorrect input 'unknown word'"); return false; }
                    }
                }
            }
            if (k != 0) { Console.WriteLine("uncorrect input '()'"); return false; }
            return true;
        }
        public static string transform(string s, string math_word)
        {
            string oper = "^*/+-";
            string oper1 = "543322";
            string numbers = "0123456789.";

            List<char> stack = new List<char>();

            int st = 0;
            string out_line = "";
            int l = 0, k = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '(')
                {
                    if (st < stack.Count)
                        stack[st] = s[i];
                    else
                        stack.Add(s[i]);
                    st++;
                    if (l > 0) k++;
                    continue;
                }
                if (oper.Contains(s[i]))
                {
                    if (st > 0)
                    {
                        if (stack[st - 1] != '(')
                            if (oper1[oper.IndexOf(stack[st - 1]) + 1] >= oper1[oper.IndexOf(s[i]) + 1])
                            {
                                st--;
                                out_line += stack[st];
                            }
                    }
                    if (s[i] == '-')
                    {
                        if (i == 0)
                            out_line += "0_";
                        else
                            if (!numbers.Contains(s[i - 1]))
                            out_line += "0_";
                    }
                    if (st < stack.Count)
                        stack[st] = s[i];
                    else
                        stack.Add(s[i]);
                    st++;
                    continue;
                }
                if (s[i] == ')')
                {
                    while (stack[st - 1] != '(')
                    {
                        st--;
                        out_line += stack[st];
                    }
                    k--;
                    if ((l != 0) && (k == l - 1))
                    {
                        l--;
                        out_line += 'm';
                    }

                    st--;
                    continue;
                }
                if (s[i] == ',')
                {
                    while (stack[st - 1] != '(')
                    {
                        st--;
                        out_line += stack[st];
                    }
                    continue;
                }
                if (numbers.Contains(s[i]))
                {
                    out_line += read_number(s, ref i, true) + "_";
                    continue;
                }
                if (math_word[0] == s[i])
                {
                    i += math_word.Length - 1;
                    l++;
                    continue;
                }
            }
            st--;
            if (st != -1)
                while (st != -1)
                { 
                    out_line += stack[st]; 
                    st--; 
                }
            return out_line;
        }
        public static double calculate(string s, string math_word)
        {
            string oper = "m^*/+-";
            string numbers = "0123456789,.";
            List<double> nums = new List<double>();
            int n = -1;
            for (int i = 0; i < s.Length; i++)
            {
                if (numbers.Contains(s[i]))
                {
                    n++;
                    if (n <= nums.Count - 1)
                        nums[n] = read_number(s, ref i, false);
                    else
                        nums.Add(read_number(s, ref i, false));
                    i++;
                    continue;
                }
                if (oper.Contains(s[i]))
                {
                    operations(s[i], ref nums, ref n);
                }
                //for (int j = 0; j < nums.Count; j++)
                //    Console.Write(nums[j] + " ");
                //Console.WriteLine();
            }
            return nums[0];
        }
        static void operations(char o, ref List<double> nums, ref int n)
        {
            try
            {
                switch (o)
                {
                    case '-':
                        nums[n - 1] = nums[n - 1] - nums[n]; n--;
                        break;
                    case '+':
                        nums[n - 1] = nums[n] + nums[n - 1]; n--;
                        break;
                    case '*':
                        nums[n - 1] = nums[n] * nums[n - 1]; n--;
                        break;
                    case '/':
                        if (nums[n] == 0) throw new DivideByZeroException();
                        nums[n - 1] = nums[n - 1] / nums[n]; n--;
                        break;
                    case 'm':
                        if ((nums[n - 1] < 0) || (nums[n] < 0)) throw new ArgumentException();
                        nums[n - 1] = Math.Log10(nums[n]) / Math.Log10(nums[n - 1]); n--;
                        break;
                }
            }
            catch (Exception exc)
            {
                mes = exc.Message;
            }

        }
        static public void main()
        {
            while (true)
            {
                mes = "no";
                string s;
                string math_word = "log";
                //Stopwatch stopwatch = new Stopwatch();
                Console.Write("expression: ");
                s = Console.ReadLine();
                //s = "log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.";
                //Console.WriteLine("expression length\n" + s.Length);

                //stopwatch.Reset();
                //stopwatch.Start();
                if (!validation(s, math_word))
                    continue;
                //stopwatch.Stop();
                //Console.WriteLine("validation time: " + stopwatch.Elapsed);
                
                //stopwatch.Reset();
                //stopwatch.Start();
                string out_line = transform(s, math_word);
                //stopwatch.Stop();
                //Console.WriteLine("transform time: " + stopwatch.Elapsed);
                
                //string[] lines = out_line.Split('_');
                //Console.Write("transformed string: ");
                //for (int i = 0; i < lines.Length; i++)
                //    Console.Write(lines[i]);
                //Console.WriteLine();
                //Console.WriteLine(out_line);

                //stopwatch.Reset();
                //stopwatch.Start();
                double res = calculate(out_line, math_word);
                //stopwatch.Stop();
                //Console.WriteLine("calculate time: " + stopwatch.Elapsed);

                Console.WriteLine("result: " + res.ToString().Replace(',','.'));
                Console.WriteLine("error: " + mes);
            }
        }
    }
}
//log(1+1,log(10,100)-2+0.25)+5*7+10/2
//log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.+log(10,1.+2.2*3.1)*(3.+4.)-5.+5.*2.