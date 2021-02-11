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
using System.Runtime.Remoting.Messaging;

namespace Tafya
{
    class Lab5
    {
        static string source_txt = "";
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
                    do
                    {
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
                c.new_text.AddRange(commands[i].Replace("'", "").Replace(" ", "").Split('|'));
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
                if ((i == 10) || (i == 13) || (i == 9))
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
        static string CheckGrammar(List<Command> commands, string str, ref string s, int k)
        {
            while (s.Length != 0)
            {
                if (error)
                {
                    throw new FormatException("parse error");
                }
                if (str != "")
                    if (dict.ContainsValue((int)str[0]))
                    {
                        int j = dict.Values.ToList().IndexOf((int)(str[0]));
                        {
                            if (commands[j].text[0] == str[0])
                            {
                                bool flag = false;
                                if ((commands[j].text[0] == 7) || (commands[j].text[0] == 8) || (commands[j].text[0] == 17))
                                {
                                    flag = true;
                                }
                                string t = s;
                                int i = 0;
                                if (flag)
                                {
                                    if (s.Length < 2)
                                    {
                                        i = 0;
                                    }
                                    else
                                    {
                                        if (s[0] == '\"')
                                        {
                                            int m = 1;
                                            while (s[m] != '\"')
                                                m++;
                                            if (s[m + 1] == ',')
                                                i = 2;
                                            else
                                                i = 0;
                                        }
                                        else
                                        {
                                            if (",*/+-".Contains(s[1]))
                                            {
                                                if (str[2] == ',')
                                                {
                                                    i = 2;
                                                }
                                                else
                                                {
                                                    if ("+-".Contains(s[1]) && (commands[j].text[0] == 7))
                                                        if (s[1] == '-')
                                                            i = 1;
                                                        else
                                                            i = 2;
                                                    else if ("*/".Contains(s[1]) && (commands[j].text[0] == 8))
                                                        if (s[1] == '/')
                                                            i = 1;
                                                        else
                                                            i = 2;
                                                    else
                                                        i = 0;
                                                }
                                            }
                                            else
                                            {
                                                i = 0;
                                            }
                                        }
                                    }
                                }
                                for (; i < commands[j].new_text.Count; i++)
                                {

                                    string tstr = CheckGrammar(commands, commands[j].new_text[i] + str.Substring(1), ref s, k + 1);
                                    if (tstr != commands[j].new_text[i] + str.Substring(1))
                                        str = tstr;
                                    if (t != s)
                                        break;
                                    if (k == 0)
                                        if ((!flag)&&(i == commands[j].new_text.Count - 1))
                                        {
                                            error = true;
                                        }
                                }
                            }
                        }
                    }
                if (k != 0)
                {
                    if (s.Length != 0)
                    {
                        try
                        {
                            if (!((s[0] == '\"')&&(str[0]=='\"')))
                            {
                                while (s[0] == str[0])
                                {
                                    if (s[0] == 'i')
                                    {
                                        if ((s[1] != str[1]) && (s[1] == 'f'))
                                            break;
                                    }
                                    if (s[0] == 'f')
                                    {
                                        if ((s[1] != str[1]) && (s[1] == 'o'))
                                            break;
                                    }
                                    if (s[0] == 's')
                                    {
                                        if ((s[1] != str[1]) && (s[1] == 'c'))
                                            break;
                                    }
                                    if (s[0] == 'p')
                                    {
                                        if ((s[1] != str[1]) && (s[1] == 'r'))
                                            break;
                                    }
                                    s = s.Substring(1);
                                    str = str.Substring(1);
                                    ind++;
                                }
                            }
                            else
                            {
                                s = s.Substring(1);
                                while (s[0] != '\"')
                                    s = s.Substring(1);
                                s = s.Substring(1);
                                str = str.Substring(2);
                                
                            }
                        }
                        catch (Exception e)
                        {
                        }
                    }
                    return str;
                }
            }
            return s;
        }
        
        static Dictionary<string, int> parameters = new Dictionary<string, int>();
        static void run_code(string path)
        {
            string[] lines = File.ReadAllLines(path);
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].Replace("\t", "");
            }
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("if"))
                    run_code_if(lines, ref i);
                else if (lines[i].Contains("for"))
                    run_code_for(lines, ref i);
                else if (lines[i].Contains("scan"))
                    run_code_scan(lines[i]);
                else if (lines[i].Contains("print"))
                    run_code_print(lines[i]);
                else
                {
                    string expr = lines[i].Split('=')[1].Replace(" ", "").Replace(";", "");
                    string par = lines[i].Split('=')[0].Replace(" ", "");
                    if (parameters.ContainsKey(par))
                        parameters[par] = run_code_expression(expr);
                    else
                        parameters.Add(par, run_code_expression(expr));

                }
            }
        }
        static void run_code_if(string[] lines,ref int i)
        {
            string expr = lines[i].Replace(" ", "").Substring(2);
            if (run_code_boolexpression(expr))
            {
                i += 2;
                while (!lines[i].Contains("}"))
                {
                    if (lines[i].Contains("if"))
                        run_code_if(lines, ref i);
                    else if (lines[i].Contains("for"))
                        run_code_for(lines, ref i);
                    else if (lines[i].Contains("scan"))
                        run_code_scan(lines[i]);
                    else if (lines[i].Contains("print"))
                        run_code_print(lines[i]);
                    else
                    {
                        expr = lines[i].Split('=')[1].Replace(" ", "").Replace(";", "");
                        string par = lines[i].Split('=')[0].Replace(" ", "");
                        if (parameters.ContainsKey(par))
                            parameters[par] = run_code_expression(expr);
                        else
                            parameters.Add(par, run_code_expression(expr));
                    }
                    i++;
                }
                i += 2;
                int l = 0;
                do
                {
                    if (lines[i].Contains("{"))
                        l++;
                    if (lines[i].Contains("}"))
                        l--;
                    i++;
                } while (l != 0);
                i--;
            }
            else
            {
                i += 2;
                int l = 1;
                while (l!=0)
                {
                    if (lines[i].Contains("{"))
                        l++;
                    if (lines[i].Contains("}"))
                        l--;
                    i++;
                }
                i += 2;
                while (!lines[i].Contains("}"))
                {
                    if (lines[i].Contains("if"))
                        run_code_if(lines, ref i);
                    else if (lines[i].Contains("for"))
                        run_code_for(lines, ref i);
                    else if (lines[i].Contains("scan"))
                        run_code_scan(lines[i]);
                    else if (lines[i].Contains("print"))
                        run_code_print(lines[i]);
                    else
                    {
                        expr = lines[i].Split('=')[1].Replace(" ", "").Replace(";", "");
                        string par = lines[i].Split('=')[0].Replace(" ", "");
                        if (parameters.ContainsKey(par))
                            parameters[par] = run_code_expression(expr);
                        else
                            parameters.Add(par, run_code_expression(expr));
                    }
                    i++;
                }
            }
        }
        static void run_code_for(string[] lines,ref int i)
        {
            string t = lines[i].Substring(3).Replace(" ", "");
            string[] str = t.Split('=');
            string name = str[0];
            str = str[1].Replace("o", "").Split('t');
            int start = int.Parse(str[0]);
            int end = int.Parse(str[1]);
            int k = i;
            if (parameters.ContainsKey(name))
                parameters[name] = start;
            else
                parameters.Add(name, start);
            for (int j = start; j < end; j++)
            {
                parameters[name] = j;
                k = i + 2;
                while (!lines[k].Contains("}"))
                {
                    if (lines[k].Contains("if"))
                        run_code_if(lines, ref k);
                    else if (lines[k].Contains("for"))
                        run_code_for(lines, ref k);
                    else if (lines[k].Contains("scan"))
                        run_code_scan(lines[k]);
                    else if (lines[k].Contains("print"))
                        run_code_print(lines[k]);
                    else
                    {
                        string expr = lines[k].Split('=')[1].Replace(" ", "").Replace(";", "");
                        string par = lines[k].Split('=')[0].Replace(" ", "");
                        if (parameters.ContainsKey(par))
                            parameters[par] = run_code_expression(expr);
                        else
                            parameters.Add(par, run_code_expression(expr));
                    }
                    k++;
                }
            }
            i = k;
        }
        static void run_code_scan(string line)
        {
            line = line.Substring(5);
            line = line.Substring(0, line.Length - 1);
            Console.WriteLine("Read parameter: ", line);
            try
            {
                if (parameters.ContainsKey(line))
                    parameters[line] = int.Parse(Console.ReadLine());
                else
                    parameters.Add(line, int.Parse(Console.ReadLine()));
            }
            catch (Exception e)
            {
                Console.WriteLine("is it number?");
            }
        }
        static void run_code_print(string line)
        {
            line = line.Substring(5);
            line = line.Substring(0, line.Length - 1);
            string[] par = line.Replace(" ","").Split(',');
            foreach (string p in par)
            {
                if (p[0] == '\"')
                    Console.Write(p.Replace("\"", ""));
                else
                    Console.Write(run_code_expression(p));
            }
            Console.WriteLine();
            
        }
        static bool run_code_boolexpression(string expr)
        {
            
            int a, b;
            if (expr.Contains(">"))
            {
                a = run_code_expression(expr.Split('>')[0]);
                b = run_code_expression(expr.Split('>')[1]);
                return a > b;
            }
            else if (expr.Contains("<"))
            {
                a = run_code_expression(expr.Split('<')[0]);
                b = run_code_expression(expr.Split('<')[1]);
                return a < b;
            }
            else if (expr.Contains("=="))
            {
                a = run_code_expression(expr.Replace("==", "=").Split('=')[0]);
                b = run_code_expression(expr.Replace("==", "=").Split('=')[1]);
                return a == b;
            }
            else if (expr.Contains("!="))
            {
                a = run_code_expression(expr.Replace("!=", "=").Split('=')[0]);
                b = run_code_expression(expr.Replace("!=", "=").Split('=')[1]);
                return a != b;
            }
            return false;
        }
        /*
        <bool_expression>: <expression>~<relop>~<expression>
        <relop>: '<' | '>' | '==' | '!='
        <expression>: <term>
        | <term>'-'<expression>
        | <term>'+'<expression>
        <term>: <factor>
        | <factor>'/'< term >
        | <factor>'*'< term >
        <factor>: <identifier> | <number> | (<expression>)
        <identifier>: <character><id_end>
        */
        static int run_code_expression(string expr)
        {
            while (expr.Contains("("))
            {
                int s = 0, f = 0, l = 0;
                for (int i = 0; i < expr.Length; i++)
                {
                    if ((expr[i] == '('))
                    {
                        if (l == 0)
                            s = i;
                        l++;
                    }
                    if ((expr[i] == ')'))
                    {
                        l--;
                        if (l == 0)
                            f = i;
                    }
                    if ((s != f) && (l == 0))
                        break;
                        
                }
                expr = expr.Substring(0, s) + run_code_term(expr.Substring(s + 1, f - s - 1)) + expr.Substring(f + 1);
            }
            if (expr.Contains('+'))
            {
                int a = run_code_expression(expr.Substring(0, expr.IndexOf('+')));
                int b = run_code_expression(expr.Substring(expr.IndexOf('+') + 1));
                return a + b;
            }
            else if (expr.Contains('-'))
            {
                int a = run_code_expression(expr.Substring(0, expr.LastIndexOf('-')));
                int b = run_code_expression(expr.Substring(expr.LastIndexOf('-') + 1));
                return a - b;
            }
            else
                return run_code_term(expr);
        }
        static int run_code_term(string term)
        {
            if (term.Contains("("))
            {
                return run_code_factor(term);
            }
            if (term.Contains('*'))
            {
                int a = run_code_factor(term.Substring(0, term.LastIndexOf('*')));
                int b = run_code_term(term.Substring(term.LastIndexOf('*') + 1));
                return a * b;
            }
            else if (term.Contains('/'))
            {
                int a = run_code_factor(term.Substring(0, term.LastIndexOf('/')));
                int b = run_code_term(term.Substring(term.LastIndexOf('/') + 1));
                return a / b;
            }
            else
                return run_code_factor(term);
        }
        static int run_code_factor(string factor)
        {
            if (factor.Contains("("))
            {
                return run_code_expression(factor);
            }
            if ("1234567890".Contains(factor[0]) && (!factor.Contains('/')) && (!factor.Contains('*')) && (!factor.Contains('-')) && (!factor.Contains('+')))
                return int.Parse(factor);
            else if ((!factor.Contains('/')) && (!factor.Contains('*')) && (!factor.Contains('-')) && (!factor.Contains('+')))
                {
                if (parameters.ContainsKey(factor))
                    return parameters[factor];
                else
                    throw new Exception("uninitialized variable");
            }
            else
                return run_code_expression(factor);
        }
        public static void main()
        {
            List<Command> commands = new List<Command>();
            string path = @"D:4\Tafya\CBAS_example.txt";
            commands = read_file(@"D:4\Tafya\CBAS.txt");
            Console.WriteLine("Commands:\n");
            foreach (Command c in commands)
                Console.WriteLine(c.ToString());
            commands = rewrite(commands);
            Console.WriteLine();
            string s = "";
            s = File.ReadAllText(path);
            Console.WriteLine("\n" + s);
            s = s.Replace("\t", "").Replace(" ", "~").Replace("\n", "").Replace("\r", "");
            source_txt = s;
            try
            {
                CheckGrammar(commands, "\0", ref s, 0);
                Console.WriteLine("done\nCode:\n");
                run_code(path);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }
            //parameters.Add("a", 76);
            //parameters.Add("b", 10);
            //parameters.Add("c", 1);
            //Console.WriteLine(run_code_expression("((a+b)+(a+b)-c*a)/2/2"));
        }
    }
}
/*
scan a;
scan b;
c = 1;
for i = 1 to 10
{
	print i*10;
	if a > b
	{
		c = a+b+c;
		if c > 5
		{
			scan c;
		}
		else
		{
			scan c;
		}
	}
	else
	{
	c = a-b+c;
	c = 2+a*c;
	print "c = ",c;
		for i = 1 to 10
		{
			print "i*10";
		}
	}
}
 */