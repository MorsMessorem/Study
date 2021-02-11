using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Tafya
{
    class Block
    {
        public int columns;
        public int rows;
        public int i = 0;
        int max_heigth;
        int max_width;
        public Element[] elements;
        public void init()
        {
            max_heigth = 24;
            max_width = 80;
            columns = 1;
            rows = 0;
        }
        public void init_elements()
        {
            if (columns * rows > 0)
                elements = new Element[columns * rows];
            else
                elements = new Element[columns + rows];
        }
    }
    class Element
    {
        public int color = 15;
        public int bgcolor = 0;
        public h_align halign = h_align.left;
        public v_align valign = v_align.top;
        public int height = -1;
        public int width = -1;
        public string text = "";
        public Block block;
        public Element(h_align halign,v_align valign,int color,int bgcolor,int width)
        {
            this.bgcolor = bgcolor;
            this.halign = halign;
            this.valign = valign;
            this.width = width;
            this.color = color;
        }
        public Element(int height, h_align halign, v_align valign, int color, int bgcolor)
        {
            this.bgcolor = bgcolor;
            this.halign = halign;
            this.valign = valign;
            this.height = height;
            this.color = color;
            this.text = text;
        }
        public Element()
        { }
        public void merge(Element element)
        {
            this.bgcolor = element.bgcolor;
            this.color = element.color;
            if (element.halign != h_align.left) 
                this.halign = element.halign;
            if (element.valign != v_align.top)
            this.valign = element.valign;
            if (element.height != -1)
                this.height = element.height;
            if (element.width != -1) 
                this.width = element.width;
            this.text = element.text;
            this.block = element.block;
        }
        public Element(Element element)
        {
            this.bgcolor = element.bgcolor;
            this.color = element.color;
            if (element.halign != h_align.left)
                this.halign = element.halign;
            if (element.valign != v_align.top)
                this.valign = element.valign;
            if (element.height != -1)
                this.height = element.height;
            if (element.width != -1)
                this.width = element.width;
            this.text = element.text;
            this.block = element.block;
        }
    }
    public enum h_align
    {
        left = 1,
        center = 2,
        right = 3
    }
    public enum v_align
    {
        top = 1,
        center = 2,
        bottom = 3
    }
    class Lab6
    {
        static bool row = false, col = false;
        static int used_width = 0;
        static int used_height = 0;
        static public void main()
        {
            Block block = new Block();
            block.init();
            read_xml(ref block);
            int a = 0, b = 0;
            Element ele = new Element(h_align.left, v_align.top, 15, 0, 80);
            ele.height = 24;
            print(block, ref a, ref b, 0, 0, ele, false);
            Console.BackgroundColor = getColor(0);
            Console.ForegroundColor = getColor(15);
        }
        static void read_xml(ref Block block)
        {
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(@"../../test.xml");
            XmlElement xmlBlock = xmldoc.DocumentElement;
            int columns;
            int rows;
            if (xmlBlock.Attributes.Count > 0)
            {
                if (xmlBlock.Attributes.GetNamedItem("columns") != null)
                    columns = int.Parse(xmlBlock.Attributes.GetNamedItem("columns").Value);
                else columns = 1;
                if (xmlBlock.Attributes.GetNamedItem("rows") != null)
                    rows = int.Parse(xmlBlock.Attributes.GetNamedItem("rows").Value);
                else rows = 0;
                block.columns = columns;
                block.rows = rows;
            }
            block.init_elements();
            foreach (XmlNode xmlnode in xmlBlock)
            {
                bool flag = false;
                if (xmlnode.Name == "column")
                {
                    col = true;
                    row = false;
                    read_xml_column(xmlnode, ref block, ref flag);
                }
                if (xmlnode.Name == "row")
                {
                    col = false;
                    row = true;
                    read_xml_row(xmlnode, ref block, ref flag);
                }
                if (xmlnode.Name == "block")
                {
                    //read_xml_column(xmlnode);
                    Console.WriteLine("???");
                    throw new FormatException("???");
                }
            }
        }
        static Block read_xml_block(XmlNode xmlBlock)
        {
            Block block = new Block();
            {
                int columns = -1;
                int rows = -1;
                {
                    if (xmlBlock.Attributes.GetNamedItem("columns") != null)
                        columns = int.Parse(xmlBlock.Attributes.GetNamedItem("columns").Value);
                    else { Console.WriteLine("uncorrect xml"); throw new FormatException("no column field in block"); }
                    if (xmlBlock.Attributes.GetNamedItem("rows") != null)
                        rows = int.Parse(xmlBlock.Attributes.GetNamedItem("rows").Value);
                    else { Console.WriteLine("uncorrect xml"); throw new FormatException("no row field in block"); }
                    if ((columns == -1) || (rows == -1))
                    { Console.WriteLine("uncorrect xml"); throw new FormatException("uncorrect fields in block"); }
                    block.columns = columns;
                    block.rows = rows;
                }
                block.init_elements();
            }
            foreach (XmlNode xmlnode in xmlBlock)
            {
                bool flag = false;
                if (xmlnode.Name == "column")
                {
                    col = true;
                    row = false;
                    block.elements[block.i] = new Element();
                    block.elements[block.i].merge(read_xml_column(xmlnode, ref block, ref flag));
                    if (!flag)
                        block.i++;
                }
                if (xmlnode.Name == "row")
                {
                    col = false;
                    row = true;
                    block.elements[block.i] = new Element();
                    block.elements[block.i].merge(read_xml_row(xmlnode, ref block,ref flag));
                    if (!flag)
                        block.i++;
                }
                if (xmlnode.Name == "block")
                {
                    //block.elements[i] = read_xml_block(xmlnode);
                    //i++;
                    Console.WriteLine("???");
                    throw new FormatException("???");
                }
                if (xmlnode.Name == "#text")
                {
                    Console.WriteLine("???");
                    throw new FormatException("???");
                    Console.WriteLine(xmlnode.Value);
                }
            }
            return block;
        }
        static Element read_xml_column(XmlNode xmlColumn, ref Block block,ref bool flag)
        {
            Element element = new Element();
            {
                int width = -1;
                int color;
                int bgcolor;
                h_align h;
                v_align v;
                {
                    if (xmlColumn.Attributes.GetNamedItem("width") != null)
                        width = int.Parse(xmlColumn.Attributes.GetNamedItem("width").Value);
                    else 
                    {
                        //Console.WriteLine("uncorrect xml");
                        //throw new FormatException("no width field in column");
                    }
                    if (xmlColumn.Attributes.GetNamedItem("textcolor") != null)
                        color = int.Parse(xmlColumn.Attributes.GetNamedItem("textcolor").Value);
                    else color = 15;
                    if (xmlColumn.Attributes.GetNamedItem("bgcolor") != null)
                        bgcolor = int.Parse(xmlColumn.Attributes.GetNamedItem("bgcolor").Value);
                    else bgcolor = 0;
                    if (xmlColumn.Attributes.GetNamedItem("halign") != null)
                        switch (xmlColumn.Attributes.GetNamedItem("halign").Value)
                        {
                            case "left":
                                h = h_align.left;
                                break;
                            case "center":
                                h = h_align.center;
                                break;
                            case "right":
                                h = h_align.right;
                                break;
                            default:
                                Console.WriteLine("uncorrect xml");
                                throw new FormatException("uncorrect halign field in column");
                        }
                    else h = h_align.left;
                    if (xmlColumn.Attributes.GetNamedItem("valign") != null)
                        switch (xmlColumn.Attributes.GetNamedItem("valign").Value)
                        {
                            case "top":
                                v = v_align.top;
                                break;
                            case "center":
                                v = v_align.center;
                                break;
                            case "bottom":
                                v = v_align.bottom;
                                break;
                            default:
                                Console.WriteLine("uncorrect xml");
                                throw new FormatException("uncorrect valign field in column");
                        }
                    else v = v_align.top;
                }
                element = new Element(h, v, color, bgcolor, width);
            }
            foreach (XmlNode xmlnode in xmlColumn)
            {
                //done
                if (xmlnode.Name == "column")
                {
                    Console.WriteLine("???");
                    throw new FormatException("???");
                }
                //done
                if (xmlnode.Name == "row")
                {
                    col = false;
                    row = true;
                    flag = true;
                    block.elements[block.i] = new Element(element);
                    block.elements[block.i].merge(read_xml_row(xmlnode, ref block, ref flag));
                    block.i++;
                }
                //
                if (xmlnode.Name == "block")
                {
                    element.block = read_xml_block(xmlnode);
                }
                if (xmlnode.Name == "#text")
                {
                    element.text = xmlnode.Value.Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace("  ", "");
                }
            }
            return element;
        }
        static Element read_xml_row(XmlNode xmlRow,ref Block block,ref bool flag)
        {
            Element element = new Element();
            {
                int height = -1; 
                int color;
                int bgcolor;
                h_align h;
                v_align v;
                {
                    if (xmlRow.Attributes.GetNamedItem("height") != null)
                        height = int.Parse(xmlRow.Attributes.GetNamedItem("height").Value);
                    else
                    {
                        //Console.WriteLine("uncorrect xml");
                        //throw new FormatException("no height field in row");
                    }
                    if (xmlRow.Attributes.GetNamedItem("textcolor") != null)
                        color = int.Parse(xmlRow.Attributes.GetNamedItem("textcolor").Value);
                    else color = 15;
                    if (xmlRow.Attributes.GetNamedItem("bgcolor") != null)
                        bgcolor = int.Parse(xmlRow.Attributes.GetNamedItem("bgcolor").Value);
                    else bgcolor = 0;
                    if (xmlRow.Attributes.GetNamedItem("halign") != null)
                        switch (xmlRow.Attributes.GetNamedItem("halign").Value)
                        {
                            case "left":
                                h = h_align.left;
                                break;
                            case "center":
                                h = h_align.center;
                                break;
                            case "right":
                                h = h_align.right;
                                break;
                            default:
                                Console.WriteLine("uncorrect xml");
                                throw new FormatException("uncorrect halign field in row");
                        }
                    else h = h_align.left;
                    //else { Console.WriteLine("uncorrect xml"); throw new FormatException("no halign field in column"); }
                    if (xmlRow.Attributes.GetNamedItem("valing") != null)
                        switch (xmlRow.Attributes.GetNamedItem("valign").Value)
                        {
                            case "top":
                                v = v_align.top;
                                break;
                            case "center":
                                v = v_align.center;
                                break;
                            case "bottom":
                                v = v_align.bottom;
                                break;
                            default:
                                Console.WriteLine("uncorrect xml");
                                throw new FormatException("uncorrect valign field in row");
                        }
                    else v = v_align.top;
                }
                element = new Element(height, h, v, color, bgcolor);
            }
            foreach (XmlNode xmlnode in xmlRow)
            {
                //done
                if (xmlnode.Name == "column")
                {
                    col = true;
                    row = false;
                    flag = true;
                    block.elements[block.i] = new Element(element);
                    block.elements[block.i].merge(read_xml_column(xmlnode, ref block, ref flag));
                    block.i++;
                }
                //done
                if (xmlnode.Name == "row")
                {
                    Console.WriteLine("???");
                    throw new FormatException("???");
                }
                if (xmlnode.Name == "block")
                {
                    element.block = read_xml_block(xmlnode);
                }
                if (xmlnode.Name == "#text")
                {
                    element.text = xmlnode.Value.Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace("  ",""); ;
                }
            }
            return element;
        }
        static ConsoleColor getColor(int color)
        {
            switch (color)
            {
                case 0:
                    return ConsoleColor.Black;
                case 1:
                    return ConsoleColor.DarkBlue;
                case 2:
                    return ConsoleColor.DarkGreen;
                case 3:
                    return ConsoleColor.DarkCyan;
                case 4:
                    return ConsoleColor.DarkRed;
                case 5:
                    return ConsoleColor.DarkMagenta;
                case 6:
                    return ConsoleColor.DarkYellow;
                case 7:
                    return ConsoleColor.Gray;
                case 8:
                    return ConsoleColor.DarkGray;
                case 9:
                    return ConsoleColor.Blue;
                case 10:
                    return ConsoleColor.Green;
                case 11:
                    return ConsoleColor.Cyan;
                case 12:
                    return ConsoleColor.Red;
                case 13:
                    return ConsoleColor.Magenta;
                case 14:
                    return ConsoleColor.Yellow;
                case 15:
                    return ConsoleColor.White;
                default:
                    return ConsoleColor.Black;
            }
        }
        static void print(Block block, ref int t_w, ref int t_h, int min_w, int min_h,Element ele,bool final)
        {
            int max_height = ele.height;
            int max_width = ele.width;
            int color = ele.color;
            int bgcolor = ele.bgcolor;
            h_align align_h = ele.halign;
            v_align align_v = ele.valign;
            for (int m = 0; m < block.elements.Length; m++) 
            {
                bool last = (m == block.elements.Length - 1);
                if (block.elements[m].block != null)
                {
                    block.elements[m].width = block.elements[m].width == -1 ? t_w : block.elements[m].width;
                    block.elements[m].height = block.elements[m].height == -1 ? t_h : block.elements[m].height;
                        print(block.elements[m].block, ref t_w, ref t_h, used_width - t_w, t_h, block.elements[m], last);
                }
                else
                {
                    if (used_width >= 80)
                        used_width = min_w;
                    if (used_height >= 24)
                        used_height = min_h;
                    if (block.elements[m].bgcolor != 0)
                        Console.BackgroundColor = getColor(block.elements[m].bgcolor);
                    else
                        Console.BackgroundColor = getColor(bgcolor);
                    if (block.elements[m].color != 15)
                        Console.ForegroundColor = getColor(block.elements[m].color);
                    else
                        Console.ForegroundColor = getColor(color);
                    string txt = block.elements[m].text;
                    int w = (block.elements[m].width == -1) ? max_width : block.elements[m].width;
                    int h = (block.elements[m].height == -1) ? max_height : block.elements[m].height;
                    if ((t_w != w))
                    {
                        used_width += w;
                    }
                    if ((t_h != h))
                    {
                        used_height += h;
                    }
                    if (used_height > 24)
                    {
                        h = h - (used_height - 24);
                        used_height = 24;
                    }
                    if (used_width > 80)
                    {
                        w = w - (used_width - 80);
                        used_width = 80;
                    }
                    int s_h = 0;
                    int s_w = 0;
                    block.elements[m].valign = (block.elements[m].valign == v_align.top) ? align_v : block.elements[m].valign;
                    block.elements[m].halign = (block.elements[m].halign == h_align.left) ? align_h : block.elements[m].halign;
                    int k = 1;
                    if (last)
                    {
                        if ((final)||((max_width-min_w==80)&&(max_height-min_h==24)))
                        {
                            if (col)
                                h = t_h;
                            if (row)
                                w = t_w;
                        }
                        used_height = ele.height + min_h;
                        used_width = ele.width + min_w;
                    }
                    t_h = h;
                    t_w = w;
                    if (used_width > 80)
                        used_width = 80;
                    if (used_height > 24)
                        used_height = 24;
                    while (txt.Length / k > w)
                        k++;
                    {
                        if (block.elements[m].halign == h_align.left)
                        {
                            s_w = used_width - w;
                        }
                        else if (block.elements[m].halign == h_align.center)
                        {
                            s_w = w / 2 + 1 + used_width - w - (txt.Length / k / 2);
                        }
                        else if (block.elements[m].halign == h_align.right)
                        {
                            s_w = used_width - (txt.Length) / k;
                        }
                        if (block.elements[m].valign == v_align.bottom)
                        {
                            s_h = used_height;
                        }
                        else if (block.elements[m].valign == v_align.center)
                        {
                            s_h = h / 2 + 1 + used_height - h;
                        }
                        else if (block.elements[m].valign == v_align.top)
                        {
                            s_h = used_height - h + 1;
                        }
                    }
                    Console.SetCursorPosition(used_width - w, used_height - h);
                    
                    int l = k;
                    for (int i = 0; i < h; i++)
                    {
                        Console.SetCursorPosition(used_width - w, used_height - h + i);
                        for (int j = 0; j < w; j++)
                        {
                            if (k != 0)
                                if ((s_h - k == i + used_height - h) && (s_w == j + used_width - w))
                                {
                                    if ((l - k + 1) * txt.Length / l + (l - k) * txt.Length / l < txt.Length)
                                        Console.Write(txt.Substring((l - k) * txt.Length / l, (l - k + 1) * txt.Length / l));
                                    else
                                        Console.Write(txt.Substring((l - k) * txt.Length / l));
                                    j += txt.Length / l;
                                    k--;
                                }
                            if (j < w)
                                Console.Write(" ");
                        }
                        Console.WriteLine();
                    }
                }
            }
        }
    }
}