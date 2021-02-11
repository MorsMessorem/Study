using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Globalization;
using System.Management.Instrumentation;
using System.Linq.Expressions;
using System.Runtime.Serialization.Formatters;
using System.Net;

namespace Tafya
{

    class Lab4
    {
        static string source_txt = "";
        static bool lexical = false;
        static bool syntactic = false;
        static bool semantic = false;
        static bool error = false;
        static int ind = 0;
        static Dictionary<string, int> dict = new Dictionary<string, int>();
        class Command
        {
            public string text = "";
            public List<string> new_text = new List<string>();
            public override string ToString()
            {
                string str = "";
                str += text + ":";
                foreach (string s in new_text)
                    str += s + "|";
                return str.Substring(0, str.Length - 1);
            }
        }
        static List<Command> read_file(string path)
        {
            List<string> commands = new List<string>();
            if (File.Exists(path))
            {
                string[] lines = File.ReadAllLines(path);
                foreach (string line in lines)

                    commands.Add(line);
            }
            return parse(commands);
        }
        static List<Command> parse(List<string> commands)
        {
            List<Command> command = new List<Command>();
            for (int i = 0; i < commands.Count;) 
            {
                int j = 1;
                if (commands[i].Contains(':'))
                {
                    do {
                        if (i + j >= commands.Count)
                            break;
                        if (commands[i + j].Contains(':'))
                            break;
                        else
                            commands[i] += commands[i + j];
                        j++;
                    } while (true);
                }
                Command c = new Command();
                c.text = commands[i].Split(':')[0];
                commands[i] = commands[i].Substring(commands[i].IndexOf(':') + 1);
                c.new_text.AddRange(commands[i].Replace("'","").Replace(" ","").Split('|'));
                command.Add(c);
                i += j;
            }
            return command;
        }
        static List<Command> rewrite(List<Command> commands)
        {
            int i = 0;
            foreach (Command com in commands)
            {
                if ((i == 10) || (i == 13) || (i==9))
                    i++;
                dict.Add(com.text, i);
                i++;
            }
            foreach (var d in dict)
            {
                for (i = 0; i < commands.Count; i++)
                {
                    if (commands[i].text.Contains(d.Key))
                        commands[i].text = commands[i].text.Replace(d.Key, (char)d.Value + " ");
                    for (int j = 0; j < commands[i].new_text.Count; j++)
                        if (commands[i].new_text[j].Contains(d.Key))
                            commands[i].new_text[j] = commands[i].new_text[j].Replace(d.Key, (char)d.Value + "");
                }
            }


            return commands;
        }
        static string CheckGrammar(List<Command> commands, string str,ref string s,int k)
        {
            bool change_flag = false;
            while (s.Length != 0)
            {
                if (dict.ContainsValue((int)str[0]))
                {
                    int j = dict.Values.ToList().IndexOf((int)(str[0]));
                    {
                        if (commands[j].text[0] == str[0])
                        {
                            string t = s;
                            for (int i = 0; i < commands[j].new_text.Count; i++)
                            {
                                string tstr = CheckGrammar(commands, commands[j].new_text[i] + str.Substring(1), ref s, k + 1);
                                change_flag = false;
                                if (tstr != commands[j].new_text[i] + str.Substring(1))
                                    str = tstr;
                                if (t != s)
                                    break;
                                if (k == 0)
                                    if (i == commands[j].new_text.Count - 1)
                                    {
                                        if ((str[0] == 4) || (str[0] == 11))
                                            semantic = true;
                                        else if ((str[0]==6)||(str[0]==12))
                                            syntactic = true;
                                        else
                                            lexical = true;
                                        error = true;
                                    }
                            }
                        }
                    }
                }
                if ((str == "") && (s == ""))
                    return "";
                if (error)
                {
                    Console.WriteLine("Error in ind {0}", ind);
                    print_error(ref s, ref str);
                    change_flag = false;
                }
                if ((str == "") && (s == ""))
                    return "";
                if (!(s[0] == str[0]))
                {
                    if (!change_flag)
                        change_flag = true;
                    else 
                    {
                        lexical = true;
                        error = true;
                        change_flag = false;
                    }
                }
                if (k != 0)
                {
                    if (s.Length!=0)
                    {
                        try
                        {
                            while (s[0] == str[0])
                            {
                                change_flag = false;
                                if (s[0] == 'i')
                                {
                                    if ((s[1] != str[1]) && ((str[1] == 'f') || (str[1] == 'n')))
                                        break;
                                }
                                s = s.Substring(1);
                                str = str.Substring(1);
                                ind++;
                            }
                        }
                        catch (Exception e)
                        {
                            //if (s.Length == 0)
                            //    Console.WriteLine("WTF!?");
                            //if (s == "")
                            //    Console.WriteLine("WTF!?");
                            //if (s.Equals(""))
                            //    Console.WriteLine("WTF!?");
                        }
                    }
                    return str;
                }
            }
            return s;
        }
        static void print_error(ref string s, ref string str)
        {
            if (lexical)
            {
                Console.WriteLine("Lexic error");
                if (!dict.ContainsValue(str[0]))
                {
                    Console.Write("\tChanged: {0} -> {1}", s[0], str[0]);
                    if (!"{".Contains(str[0]))// str = str[0] + str;
                    str = str.Substring(1);
                    s = s.Substring(1);
                }
                else
                {
                    Console.Write("\tIgnored: ");
                    Console.Write(s[0]);
                    s = s.Substring(1);
                }
                ind++;                
            }
            int k = 0;
            if (semantic)
            {
                Console.WriteLine("Semantic error (bool):");
                Console.Write("\tIgnored: ");
                while ((s[0] != ';') && (s[0] != ')'))
                {
                    Console.Write(s[0]);
                    s = s.Substring(1);
                    k++;
                    ind++;
                }
                str = str.Substring(1);
            }
            if (syntactic)
            {
                Console.WriteLine("Syntactic error ('()'):");
                Console.Write("\tAdded symbol: ");
                {
                    Console.Write(str[1]);
                    s = str[1] + s;
                }
                //ind++;
                str = str.Substring(1);
            }
            Console.Write("\n\t");
            int l = 5;
            if (ind != 0)
            {
                for (int i = ind - l; i < ind + l+1; i++)
                {
                    if ((i >= 0) && (i < source_txt.Length-1))
                    {
                        if (lexical)
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            if (i == ind - 1) 
                                Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write(source_txt[i]);
                        }
                        if (syntactic)
                        {
                            if (i == ind)
                            {
                                Console.ForegroundColor = ConsoleColor.Blue;
                                Console.Write(s[0]);
                                ind--;
                            }
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write(source_txt[i]);
                        }
                        if (semantic)
                        {
                            if (i == ind - k )
                                Console.ForegroundColor = ConsoleColor.Yellow;
                            if ((source_txt[i] == ';') || (source_txt[i] == ')'))
                                Console.ForegroundColor = ConsoleColor.White;
                            Console.Write(source_txt[i]);
                        }
                    }
                }
            }
            Console.WriteLine();
            semantic = false;
            syntactic = false;
            error = false;
            lexical = false;
            if (s != "")
            {
                while (s[0] == str[0])
                {
                    if (s[0] == 'i')
                    {
                        if ((s[1] != str[1]) && ((str[1] == 'f') || (str[1] == 'n')))
                            break;
                    }
                    s = s.Substring(1);
                    str = str.Substring(1);
                    ind++;
                }
            }
        }
        public static void main()
        {
            List<Command> commands = new List<Command>();
            commands = read_file(@"D:4\Tafya\Clite.txt");
            Console.WriteLine("Commands:\n");
            foreach (Command c in commands)
                Console.WriteLine(c.ToString());
            commands = rewrite(commands);
            Console.WriteLine();
            string s = "";
            s = File.ReadAllText(@"D:4\Tafya\Clite_example.txt");
            Console.WriteLine("\n"+s);
            s = s.Replace("\n", "").Replace("\r", "").Replace("\t", "").Replace(" ", "~");
            source_txt = s;
            CheckGrammar(commands, "\0", ref s, 0);
        }
    }
}