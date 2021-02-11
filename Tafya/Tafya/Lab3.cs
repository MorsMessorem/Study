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
    
    class Lab3
    {
        class Command
        {
            public int state = -1;
            public string word = "";
            public string text = "";
            public int new_state = -1;
            public string new_text = "";
            public string Print()
            {
                //Console.WriteLine("δ(s" + state + "," + word + "," + text + ")");
                return "δ(s" + state + "," + word + "," + text + ")";
            }
            public override string ToString()
            {
                return "δ(s" + state + "," + word + "," + text + ")=(s" + new_state + "," + new_text + ")";
            }
            public bool check()
            {
                if ((word == "λ") && (text == "λ"))
                    return true;
                return false;
            }
        }
        static List<string> res = new List<string>();
        static List<Command> read_file(string path)
        {
            List<string> commands = new List<string>();
            if (File.Exists(path))
            {
                string[] lines = File.ReadAllLines(path);
                foreach (string line in lines)
                    commands.Add(line.Replace(" ", ""));
            }
            return parse(commands);
        }
        static List<Command> parse(List<string> commands)
        {
            string Z = "", P = "";
            int S = 0, F = 0;
            Console.WriteLine("S={s" + S + "}\nF={" + F + "}");
            int k = 0;
            foreach (string com in commands)
            {
                for (int i = 0; i < com.Length; i++)
                    if ((com[i] != '|') && (!Z.Contains(com[i]))) 
                        if (com[i] != '>')
                            Z += com[i];
                        else
                            k++;
            }
            if (k > commands.Count)
                Z += ">";
            Z += "h";
            string A = "QWERTYUIOPASDFGHJKLZXCVBNM";
            for (int i=0;i<Z.Length-1;i++)
            {
                if (!(A.Contains(Z[i].ToString())))
                    P += Z[i];
            }
            Console.WriteLine("P = " + P + "\nZ = " + Z + "\n");
            List<Command> comm = new List<Command>();
            foreach (string com in commands)
            {
                if (com == "")
                    continue;
                string[] s = com.Remove(0, com.IndexOf('>') + 1).Split('|');
                for (int i = 0; i < s.Count(); i++)
                {
                    Command c = new Command();
                    c.state = S;
                    c.new_state = F;
                    c.word = "λ";
                    c.text = com.Split('>')[0];
                    c.new_text = "";
                    for (int j = s[i].Length - 1; j >= 0; j--)
                    {
                        if (s[i][j] == '(')
                            c.new_text += ')';
                        else if (s[i][j] == ')')
                            c.new_text += '(';
                        else
                            c.new_text += s[i][j];
                    }
                    if (!com_contains(comm, c))
                        comm.Add(c);
                }
            }
            for (int i = 0; i < P.Length; i++)
            {
                Command c = new Command();
                c.state = S;
                c.new_state = F;
                c.word = P[i].ToString();
                c.text = P[i].ToString();
                c.new_text = "λ";
                if (!com_contains(comm, c))
                    comm.Add(c);
            }
            {
                Command c = new Command();
                c.state = S;
                c.new_state = F;
                c.word = "λ";
                c.text = "h";
                c.new_text = "λ";
                if (!com_contains(comm,c))
                    comm.Add(c);
            }
            return comm;
        }
        static bool com_contains(List<Command> commands, Command c)
        {
            if (commands.Count == 0)
                return false;
            int k = 0;
            foreach (Command com in commands)
            {
                if (com.ToString() != c.ToString())
                    k++;
            }
            if (k == commands.Count)
                return false;
            return true;
        }
        static bool contains(List<Command> commands, Command c, int step,int max_step)
        {
            if (c.word.Length == 0)
                return false;
            if (c.check())
                return true;
            if (step == max_step)
                return false;
            bool flag = false;
            //if ((c.word == "λ") && (c.text == "λ"))
            //    Console.WriteLine();
            foreach (Command com in commands)
            {
                if (com.word != "λ")
                    if (c.state == com.state)
                        if (c.word[0] == com.word[0])
                            if (c.text[c.text.Length - 1] == com.text[com.text.Length - 1])
                            {
                                Command t = new Command();
                                t.text = c.text.Substring(0, c.text.Length - 1);
                                t.word = c.word.Substring(1);
                                if (t.word.Length == 0)
                                    t.word = "λ";
                                t.state = c.state;
                                flag = contains(commands, t, step + 1, max_step);
                                if (flag)
                                {
                                    res.Add(t.Print());
                                    return flag;
                                }
                            }
            }
            foreach (Command com in commands)
            {
                if (com.word == "λ")
                    if (c.state == com.state)
                        //if (c.word[0] == com.word[0])
                        if (c.text[c.text.Length - 1] == com.text[com.text.Length - 1])
                        {
                            Command t = new Command();
                            t.text = c.text.Substring(0, c.text.Length - 1);
                            t.text += com.new_text;
                            t.state = c.state;
                            t.word = c.word;
                            flag = contains(commands, t, step + 1, max_step);
                            if (flag)
                            {
                                res.Add(t.Print());
                                return flag;
                            }
                        }
            }
            return flag;
        }
        public static void main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            List<Command> commands = new List<Command>();
            commands = read_file(@"D:4\Tafya\test1.txt");
            Console.WriteLine("Commands:\n");
            foreach (Command c in commands)
                Console.WriteLine(c.ToString());
            string s = "";
            while (true)
            {
                Console.Write("\nWrite word: ");
                s = Console.ReadLine();
                res.Clear();
                int max_step = Math.Max(s.Length * s.Length, 20);
                Command c = new Command();
                c.state = 0;
                c.word = s.Replace(" ", "~");
                c.text = "h" + "E";
                if (contains(commands, c, 0, max_step))
                {
                    res.Add(c.Print());
                    for (int i = res.Count - 1; i >= 0; i--)
                        Console.WriteLine(res[i]);
                    Console.WriteLine("Founded");//\t" + res.Count + "\t" + s.Length);
                }
                else { Console.WriteLine("Not founded or low max steps count"); }
            }
        }
    }
}
/*
E > E + T | T
T > T * F | F
F > ( E ) | a | ~
a > 1|2|3|4|5|6|7|8|9|0|aa
 */
