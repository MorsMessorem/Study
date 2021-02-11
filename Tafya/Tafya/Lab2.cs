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
    class Lab2
    {
        class Com
        {
            public int start = -1;
            public bool cont = false;
            public char symbol = ' ';
            public List<int> end = new List<int>();
            public List<bool> fin = new List<bool>();
            public Com(Com com)
            {
                start = com.start;
                cont = com.cont;
                symbol = com.symbol;
                end = com.end;
                fin = com.fin;
            }
            public Com()
            {
                end = new List<int>();
                fin = new List<bool>();
                cont = false;
                start = -1;
                symbol = ' ';
            }
            public override string ToString()
            {
                string str = "";
                if (cont)
                    str += 'f';
                else
                    str += 'q';
                str += "" + start + "," + symbol + "=";
                for (int i = 0; i < end.Count; i++)
                {
                    if (fin[i])
                        str += "f";
                    else
                        str += "q";
                    str += end[i] + " ";
                }
                return str;
            }
        }
        static int read_number(string str, ref int st)
        {
            string num_str = "0123456789";
            string num = "";
            if (!num_str.Contains(str[st]))
            {
                throw new FormatException("uncnown number");
            }
            while (num_str.Contains(str[st]))
            {
                num += str[st];
                st++;
                if (st >= str.Length) break;
            }
            st--;
            return int.Parse(num);
        }
        static List<Com> read_file(string path)
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
        static List<Com> parse(List<string> commands)
        {
            List<Com> comm = new List<Com>();
            for (int i = 0; i < commands.Count; i++)
            {
                if (commands[i] == "")
                    continue;
                bool f = false;
                Com c = new Com();
                int j = 0;
                if ((commands[i][j] == 'q') || (commands[i][j] == 'f'))
                {
                    if (commands[i][j] == 'q')
                    { j++; c.start = read_number(commands[i], ref j); j++; }
                    if (commands[i][j] == 'f')
                    { j++; c.start = read_number(commands[i], ref j); c.cont = true; j++; }
                }
                else throw new FormatException("uncnown start state");
                if ((commands[i][j] == ','))
                { j++; c.symbol = commands[i][j]; j++; }
                else throw new FormatException("no ',' simbol");
                if ((commands[i][j] == '='))
                {
                    j++;
                    if ((commands[i][j] == 'q') || (commands[i][j] == 'f'))
                    {
                        if ((commands[i][j] == 'f'))
                        { j++; c.end.Add(read_number(commands[i], ref j)); c.fin.Add(true); }
                        if ((commands[i][j] == 'q') && (c.start != -1))
                        { j++; c.end.Add(read_number(commands[i], ref j)); c.fin.Add(false); }
                    }
                    else throw new FormatException("uncnown end state");
                    j++;
                }
                else
                    throw new FormatException("no '=' symbol");
                for (int k = 0; k < comm.Count; k++)
                    if ((c.start == comm[k].start)&&(c.cont==comm[k].cont))
                        if (c.symbol == comm[k].symbol)
                        {
                            comm[k].fin.Add(c.fin[0]);
                            comm[k].end.Add(c.end[0]);
                            f = true;
                        }
                if (!f)
                    comm.Add(c);
            }
            return comm;
        }
        static bool contains(List<Com> commands, string str)
        {
            bool flag = false;
            int q = 0;
            for (int i = 0; i < str.Length; i++)
            {
                int k = 0;
                foreach (Com com in commands)
                {
                    if ((com.symbol == str[i]) && (q == com.start) && (flag == com.cont)) 
                    {
                        for (int j = 0; j < com.end.Count; j++)
                        {
                            if (!com.fin[j])
                            {
                                q = com.end[j];
                                flag = false;
                            }
                            else
                            {
                                if (i == str.Length - 1)
                                    return true;
                                else
                                {
                                    q = com.end[j];
                                    foreach (Com com1 in commands)
                                    {
                                        if ((com1.start == q) && (com1.cont))
                                            flag = true;
                                    }
                                    if (!flag)
                                        return false;
                                }
                            }
                            break;
                        }
                        break;
                    }
                    else k++;
                    if (k == commands.Count)
                        return false;
                }
            }
            return false;
        }
        static bool deterministic(List<Com> commands)
        {
            foreach (Com com in commands)
            {
                if (com.end.Count > 1)
                    return false;
            }
            return true;
        }
        static void determine(List<Com> commands, string path)
        {
            int s = 0;
            //alphabet - Σ 
            string Σ = "";
            //finite set of states Q (not final)
            List<int> Q = new List<int>();
            //set of accept states F (final)
            List<int> F = new List<int>();
            //delta - commands Q*Σ->2^Q
            //transition function
            ///change the original data
            foreach (Com com in commands)
            {
                for (int i = 0; i < com.end.Count; i++)
                {
                    if (com.fin[i])
                        if (!F.Contains(com.end[i]))
                            F.Add(com.end[i]);
                }
                if (com.cont)
                {
                    if (!F.Contains(com.start))
                        F.Add(com.start);
                }
                else if (!Q.Contains(com.start))
                    Q.Add(com.start);
                if (!Σ.Contains(com.symbol))
                    Σ += com.symbol + "";
            }
            int m = Q.Max();
            for (int i = 0; i < F.Count; i++)
                F[i] = F[i] + m + 1;
            foreach (Com com in commands)
            {
                for (int i = 0; i < com.end.Count; i++)
                {
                    if (com.fin[i])
                        com.end[i] += m + 1;
                }
                if (com.cont)
                    com.start += m + 1;
            }
            /*
               Пусть нам дан произвольный НКА: ⟨Σ,Q,s∈Q,T⊂Q,δ:Q×Σ→2^Q⟩.
               Построим по нему следующий ДКА: ⟨Σ,Qd,sd∈Qd,Td⊂Qd,δd:Qd×Σ→Qd⟩, где:
                           Qd={qd∣qd⊂2^Q},
                           sd={s},
                           Td={q∈Qd∣∃p∈T:p∈q},
                           δd(q,c)={δ(a,c)∣a∈q}.
            */
            //P  — очередь состояний, соответствующих множествам, состоящих из состояний НКА.
            //Qd — массив множеств, соответствующих состояниям ДКА.
            //s(q0) — стартовое состояние НКА. 
            List<KeyValuePair<KeyValuePair<int, int>, char>> deltad = new List<KeyValuePair<KeyValuePair<int, int>, char>>(); //in1 - > start, int2 -> end, char -> symbol
            Stack<List<int>> P = new Stack<List<int>>();
            List<List<int>> Qd;
            /*
             P.push({s})
               Qd = ∅
               while P ≠ ∅
                  P.pop(pd)
                  for c∈Σ
                     qd = ∅
                     for p∈pd
                        qd = qd∪{δ(p,c)}
                     δd(pd,qd) = c
                     if qd∉Qd
                        P.push(qd)
                        Qd.add(qd)           
               Td = {qd∈Qd∣∃p∈T:p∈qd}
               return ⟨Σ,Qd,{s},Td,δd⟩
             */
            P.Push(new List<int>() { s });              //P.push({ s})
            Qd = new List<List<int>>();   //Qd = ∅
            while (P.Count != 0)    //while P ≠ ∅
            {
                List<int> pd = P.Pop(); //P.pop(pd)
                foreach (char c in Σ) //alphabet symbol c∈Σ
                {
                    List<int> qd = new List<int>(); //qd = ∅
                    foreach (int p in pd) //for p∈pd
                    {
                        foreach (Com com in commands)
                        {
                            if ((com.symbol == c) && (com.start == p))
                                for (int i = 0; i < com.end.Count; i++)
                                    if (!qd.Contains(com.end[i]))
                                        qd.Add(com.end[i]); //qd = qd∪{δ(p,c)}
                        }
                    }
                    if ((!list_contains(Qd, qd)) && (qd.Count > 0))  //if qd∉Qd
                    {
                        P.Push(qd); //P.push(qd)
                        Qd.Add(qd); //Qd.add(qd)           
                    }
                }
            }
            if (!list_contains(Qd, new List<int>() { s }))
                Qd.Add(new List<int>() { s });
            foreach (List<int> q in Qd)
                P.Push(q);
            while (P.Count != 0)
            {
                int deltap = 0, deltaq = 0;
                List<int> pd = P.Pop();
                for (int i = 0; i < Qd.Count; i++)
                {
                    int k = 0;
                    if (Qd[i].Count == pd.Count)
                        foreach (int p in Qd[i])
                        {
                            foreach (int j in pd)
                                if (j == p)
                                    k++;
                        }
                    if (k == pd.Count)
                    { deltap = i; break; }
                }
                foreach (char c in Σ)
                {
                    deltaq = -1;
                    List<int> qd = new List<int>();
                    foreach (int p in pd)
                    {
                        foreach (Com com in commands)
                        {
                            if ((com.symbol == c) && (com.start == p))
                                for (int i = 0; i < com.end.Count; i++)
                                    if (!qd.Contains(com.end[i]))
                                        qd.Add(com.end[i]);
                        }
                    }
                    if (qd.Count > 0)
                    for (int i = 0; i < Qd.Count; i++)
                    {
                        int k = 0;
                        if (Qd[i].Count == qd.Count)
                            foreach (int q in Qd[i])
                                foreach (int j in qd)
                                    if (j == q)
                                        k++;
                        if (k == qd.Count)
                        {
                            deltaq = i; break;
                        }
                    }
                    if ((deltaq != -1) && (!pair_contains(deltad, new KeyValuePair<KeyValuePair<int, int>, char>(new KeyValuePair<int, int>(deltap, deltaq), c)))) 
                        deltad.Add(new KeyValuePair<KeyValuePair<int, int>, char>(new KeyValuePair<int, int>(deltap, deltaq), c));        //δd(pd,qd) = c
                }
            }
            List<int> Fd = new List<int>(); // indexes of final states from Qd
            //Fd = { qd∈Qd∣∃p∈F: p∈qd}
            int sd = 0;
            for (int i=0;i<Qd.Count;i++)
            {
                if ((Qd[i].Count == 1) && (Qd[i][0] == s))
                    sd = i; 
                foreach (int q in Qd[i])
                    foreach (int p in F)
                        if (q == p)
                            if (!Fd.Contains(i))
                                Fd.Add(i);
            }
            using (StreamWriter file = new StreamWriter(path + ".txt", false)) { }
            foreach (var delta in deltad)
            {
                string str = "";
                if (Fd.Contains(delta.Key.Key))
                    str += "f";
                else
                    str += "q";
                str += ((delta.Key.Key == sd) ? 0 : delta.Key.Key) + ","+delta.Value+"=";
                if (Fd.Contains(delta.Key.Value))
                    str += "f";
                else
                    str += "q";
                str += ((delta.Key.Value == sd) ? 0 : delta.Key.Value);
                write(str, path + ".txt"); //return ⟨Σ,Qd,{s},Td,δd⟩
            }
        }
        static bool pair_contains(List<KeyValuePair<KeyValuePair<int, int>, char>> deltad, KeyValuePair<KeyValuePair<int, int>, char> pair)
        {
            foreach (KeyValuePair<KeyValuePair<int,int>,char> pairs in deltad)
            {
                if ((pairs.Key.Key == pair.Key.Key) && (pairs.Key.Value == pair.Key.Value) && (pairs.Value == pair.Value))
                    return true;
            }
            return false;
        }
        static bool list_contains(List<List<int>> Qd, List<int> qd)
        {
            bool flag = true;
            bool flag1 = false;
            foreach (List<int> q in Qd)
            {
                int k = 0;
                if (q.Count == qd.Count)
                {
                    for (int i = 0; i < q.Count; i++)
                        if (q[i] == qd[i])
                            k++;
                    if (k == q.Count)
                        return true;
                }
                else
                    flag = false;
            }
            if ((Qd.Count == 0) || (qd.Count == 0)) 
                flag = false;
            return (flag||flag1);
        }
        static void write(string com, string path)
        {
            using (StreamWriter file = new StreamWriter(path, true))
            {
                file.WriteLine(com);
            }
        }
        static public void main()
        {
            string path = "D:\\4\\Tafya\\lab2.txt";
            List<Com> commands;
            try
            {
                commands = read_file(path);
            }
            catch (Exception exc)
            {
                Console.WriteLine("ERROR\n\n" + exc.Message+"\n");
                return;
            }
            Console.WriteLine("determ: " + deterministic(commands));
            if (!deterministic(commands))
            {
                Console.WriteLine("it was nondeterministic finite automaton");
                determine(commands, path);
                commands = read_file(path + ".txt");
            }
            if (deterministic(commands))
                Console.WriteLine("it is deterministic finite automaton");
            else
                Console.WriteLine("Something going wrong");
            foreach (Com com in commands)
                Console.WriteLine(com);
            string str;
            Console.WriteLine("test strings:\n");
            while (true)
            {
                str = Console.ReadLine();
                Console.Write("Contains: ");
                Console.WriteLine(contains(commands, str));
            }
        }
    }
}