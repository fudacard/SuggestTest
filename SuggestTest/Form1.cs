using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace SuggestTest
{
    public partial class Form1 : Form
    {
        Sgry.Azuki.WinForms.AzukiControl azuki;

        [DllImport("user32.dll")]
        private static extern bool GetCaretPos(out Point point);

        Dictionary<string, JSType> types;

        public Form1()
        {
            InitializeComponent();
            types = ReadXml("types.xml");
            azuki = new Sgry.Azuki.WinForms.AzukiControl();
            azuki.Dock = DockStyle.Fill;
            this.Controls.Add(azuki);
            char[] c = azuki.Document.EolCode.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                Console.WriteLine((int)c[i]);
            }
            azuki.Document.EolCode = "\r";

            int InputBegin = 0;
            int InputEnd = 0;
            string NewInputString = "";
            List<string> CurrentList = null;
            Dictionary<string, JSType> OriginalList = null;
            azuki.KeyPress += delegate(object sender, KeyPressEventArgs e)
            {
                Console.WriteLine((int)'.' + " " + (e.KeyChar == 0x08));
                if (listBox1.Visible)
                {

                    if (e.KeyChar == '\r')
                    {
                        // エンターキー
                        if (listBox1.SelectedIndex >= 0)
                        {
                            e.Handled = true;
                            azuki.Document.Replace((string)listBox1.SelectedItem, InputBegin, InputEnd);
                        }
                        listBox1.Visible = false;
                    }
                    else if (e.KeyChar == ' ')
                    {
                        // スペースキー
                        if (listBox1.SelectedIndex >= 0)
                        {
                            azuki.Document.Replace((string)listBox1.SelectedItem, InputBegin, InputEnd);
                        }
                        listBox1.Visible = false;
                    }
                    else if (e.KeyChar == '\t')
                    {
                        // タブキー
                        if (listBox1.SelectedIndex >= 0)
                        {
                            e.Handled = true;
                            azuki.Document.Replace((string)listBox1.SelectedItem, InputBegin, InputEnd);
                        }
                        listBox1.Visible = false;
                    }
                    else if (e.KeyChar == '.')
                    {
                        // ピリオド
                        if (listBox1.SelectedIndex >= 0)
                        {
                            azuki.Document.Replace((string)listBox1.SelectedItem, InputBegin, InputEnd);
                        }
                        listBox1.Visible = false;
                    }
                    else
                    {
                        if (e.KeyChar != 0x08)
                        {
                            NewInputString += e.KeyChar;
                            InputEnd++;
                        }
                        else if (e.KeyChar == 0x08)
                        {
                            if (NewInputString.Length > 0)
                            {
                                NewInputString = NewInputString.Substring(0, NewInputString.Length - 1);
                                InputEnd--;
                            }
                            else
                            {
                                listBox1.Visible = false;
                            }
                        }
                        // 入力候補を探して候補がある場合のみリスト更新
                        List<string> NewList = new List<string>();
                        foreach (string member in CurrentList)
                        {
                            if (member.StartsWith(NewInputString))
                            {
                                NewList.Add(member);
                            }
                        }
                        if (NewList.Count > 0)
                        {
                            listBox1.Items.Clear();
                            foreach (string member in NewList)
                            {
                                listBox1.Items.Add(member);

                            }
                            listBox1.SelectedIndex = 0;
                        }
                        else
                        {
                            listBox1.SelectedIndex = -1;
                        }
                    }
                }


                if (!listBox1.Visible)
                {
                    if (e.KeyChar == '.')
                    {
                        string src = azuki.Text.Substring(0, azuki.CaretIndex);
                        string token = lastToken(src, 1);
                        Console.WriteLine(token);
                        //string className = GetClass(src, token);
                        string className;
                        if (types.ContainsKey(token))
                        {
                            // クラス名
                            className = token;
                        }
                        else
                        {
                            // 変数名
                            className = GetClassEx(src);
                        }

                        if (className != null && types[className] != null)
                        {
                            //OriginalList = types[className].Members;
                            OriginalList = types[className].GetMembers();
                            if (OriginalList != null && OriginalList.Count > 0)
                            {

                                // リストボックスを所定の位置に移動
                                Point apiPoint;
                                GetCaretPos(out apiPoint);
                                listBox1.Left = apiPoint.X;
                                listBox1.Top = apiPoint.Y + (azuki.LineHeight + 2);

                                // 入力候補取得、表示
                                //OriginalList = GetMembers(className);

                                CurrentList = new List<string>(OriginalList.Keys);
                                CurrentList.Sort();
                                listBox1.Items.Clear();
                                foreach (string member in CurrentList)
                                {
                                    listBox1.Items.Add(member);
                                }
                                listBox1.SelectedIndex = 0;
                                listBox1.Visible = true;
                                int lineIndex, columnIndex;
                                azuki.GetSelection(out lineIndex, out columnIndex);
                                InputBegin = lineIndex + 1;
                                InputEnd = InputBegin;
                                NewInputString = "";

                                //Popup f = new Popup();
                                //f.Show(this);
                                //f.Size = new Size(300, 50);
                                //Rectangle clientRect = this.ClientRectangle;
                                //Point winP = this.PointToClient(this.Bounds.Location);
                                //f.Left = this.Left - winP.X + listBox1.Left - 300;
                                //f.Top = this.Top - winP.Y + listBox1.Top;

                                //azuki.Focus();
                            }
                        }


                    }
                    else if (('A' <= e.KeyChar && e.KeyChar <= 'Z') || ('a' <= e.KeyChar && e.KeyChar <= 'z'))
                    {

                        string src = azuki.Text.Substring(0, azuki.CaretIndex);
                        string token = lastToken(src, 1);
                        Console.WriteLine("[" + token + "]");
                        if (";(){}+-*/=new".IndexOf(token) >= 0)
                        {
                            CurrentList = GetLocalWords(src);
                            if (CurrentList.Count > 0)
                            {

                                // リストボックスを所定の位置に移動
                                Point apiPoint;
                                GetCaretPos(out apiPoint);
                                listBox1.Left = apiPoint.X;
                                listBox1.Top = apiPoint.Y + (azuki.LineHeight + 2);

                                // 入力候補取得、表示
                                //OriginalList = GetMembers(className);

                                CurrentList.Sort();
                                listBox1.Items.Clear();
                                foreach (string member in CurrentList)
                                {
                                    listBox1.Items.Add(member);
                                }
                                //listBox1.SelectedIndex = 0;
                                listBox1.Visible = true;
                                int lineIndex, columnIndex;
                                azuki.GetSelection(out lineIndex, out columnIndex);
                                InputBegin = lineIndex;
                                InputEnd = InputBegin + 1;
                                NewInputString = "" + e.KeyChar;

                            }
                        }
                    }
                }


            };
            azuki.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (listBox1.Visible && ((e.KeyData & Keys.Right) == Keys.Right))
                {
                    Console.WriteLine("Right");
                    listBox1.Visible = false;
                }
                else if (listBox1.Visible && ((e.KeyData & Keys.Left) == Keys.Left))
                {
                    Console.WriteLine("Left");
                    listBox1.Visible = false;
                }
                else if (listBox1.Visible && ((e.KeyData & Keys.Up) == Keys.Up))
                {
                    Console.WriteLine("Up");
                    if (listBox1.SelectedIndex > 0)
                    {
                        listBox1.SelectedIndex--;
                    }
                    e.Handled = true;
                }
                else if (listBox1.Visible && (e.KeyData & Keys.Down) == Keys.Down)
                {
                    if (listBox1.SelectedIndex < listBox1.Items.Count - 1)
                    {
                        listBox1.SelectedIndex++;
                    }
                    e.Handled = true;
                }
            };
            Console.WriteLine(azuki.LineHeight);
            //LineHeight = TextRenderer.MeasureText("test", textBox1.Font, textBox1.ClientSize, TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl).Height + 2;
            azuki.Text = ReadSrc("main.js");
        }

        private static string ReadSrc(string path)
        {
            StreamReader sr = new StreamReader(path);
            string text = sr.ReadToEnd();
            sr.Close();

            return text;
        }

        private static void WriteXml(string path, Dictionary<string, JSType> types)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            using (XmlWriter writer = XmlWriter.Create(path, settings))
            {
                writer.WriteStartElement("Classes");
                foreach (KeyValuePair<string, JSType> pair in types)
                {
                    writer.WriteStartElement("Class");
                    writer.WriteElementString("Name", pair.Value.Name);
                    writer.WriteStartElement("Members");
                    foreach (KeyValuePair<string, JSType> memberpair in pair.Value.Members)
                    {
                        writer.WriteStartElement("Member");
                        writer.WriteElementString("Name", memberpair.Key);
                        writer.WriteElementString("Type", memberpair.Value.Name);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
        }

        private static Dictionary<string, JSType> ReadXml(string path)
        {
            Dictionary<string, JSType> result = new Dictionary<string, JSType>();
            string currentClass = "";
            string currentMember = "";
            string target = "Class";
            using (XmlReader reader = XmlReader.Create(path))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        switch (reader.LocalName)
                        {
                            case "Class":
                                target = "Class";
                                break;
                            case "Members":
                                target = "Members";
                                break;
                            case "Name":
                                if (target == "Class")
                                {
                                    currentClass = reader.ReadString();
                                    if (!result.ContainsKey(currentClass))
                                    {
                                        result[currentClass] = new JSType(currentClass);
                                    }
                                    //result[currentClass].Name = reader.ReadString();
                                }
                                else
                                {
                                    currentMember = reader.ReadString();
                                }
                                break;
                            case "Type":
                                string type = reader.ReadString();
                                if (!result.ContainsKey(type))
                                {
                                    result[type] = new JSType(type);
                                }
                                result[currentClass].Members[currentMember] = result[type];

                                break;
                            case "Super":
                                string superClass = reader.ReadString();
                                if (!result.ContainsKey(superClass))
                                {
                                    result[superClass] = new JSType(superClass);
                                }
                                result[currentClass].Super = result[superClass];
                                break;
                        }

                    }
                }
            }
            return result;
        }

        private string GetClassEx(string src)
        {

            string token = lastToken(src, 1);
            string token2 = lastToken(src, 2);
            if (token2 == ".")
            {
                string token3 = lastToken(src, 3);
                return types[GetClass(src, token3)].Members[token].Name;
            }
            else
            {
                string className = GetClass(src, token);
                if (className != null)
                {
                    return types[className].Name;
                }
                else
                {
                    return null;
                }
            }
        }



        private void Form1_Load(object sender, EventArgs e)
        {
            listBox1.Visible = false;

        }

        private string lastToken(string str, int count)
        {
            Match m = null;
            for (int i = 0; i < count; i++)
            {
                if ((m = Regex.Match(str, @"\w+$")).Success)
                {
                    str = str.Substring(0, m.Index);
                }
                else if ((m = Regex.Match(str, @"\.$")).Success)
                {
                    str = str.Substring(0, m.Index);
                }
                else if ((m = Regex.Match(str, @"\s+$")).Success)
                {
                    str = str.Substring(0, m.Index);
                    i--;
                }
                else
                {
                    Console.WriteLine();
                }
            }

            return m.Value;
        }

        private List<Token> Tokenize(string src)
        {
            Match m;
            List<Token> tokens = new List<Token>();
            int count = 0;
            int i = 0;
            while (i < src.Length)
            {
                if (src[i] == '/' && src[i + 1] == '*')
                {
                    while (!(src[i] == '*' && src[i + 1] == '/'))
                    {
                        i++;
                    }
                    i += 2;
                }
                else if (src[i] == '/' && src[i] == '/')
                {
                    while (src[i] != '\n')
                    {
                        i++;
                    }
                    i++;
                }
                else if ((m = Regex.Match(src.Substring(i), @"^'.*?'")).Success)
                {
                    //Console.WriteLine("[" + m.Value + "]");
                    Token token = new Token();
                    token.body = m.Value;
                    tokens.Add(token);
                    i += m.Value.Length;
                }
                else if ((m = Regex.Match(src.Substring(i), "^\".*?\"")).Success)
                {
                    //Console.WriteLine("[" + m.Value + "]");
                    Token token = new Token();
                    token.body = m.Value;
                    tokens.Add(token);
                    i += m.Value.Length;
                }
                else if ((m = Regex.Match(src.Substring(i), @"^=+")).Success)
                {
                    //Console.WriteLine("[" + m.Value + "]");
                    Token token = new Token();
                    token.body = m.Value;
                    tokens.Add(token);
                    i += m.Value.Length;
                }
                else if ((m = Regex.Match(src.Substring(i), @"^\s+")).Success)
                {
                    i += m.Value.Length;
                }
                else if ((m = Regex.Match(src.Substring(i), @"^!==")).Success)
                {
                    // Console.WriteLine("[" + m.Value + "]");
                    Token token = new Token();
                    token.body = m.Value;
                    tokens.Add(token);
                    i += m.Value.Length;
                }
                else if ((m = Regex.Match(src.Substring(i), @"^[(){},.\!']")).Success)
                {
                    //Console.WriteLine("[" + m.Value + "]");
                    Token token = new Token();
                    token.body = m.Value;
                    tokens.Add(token);
                    i += m.Value.Length;
                }
                else if ((m = Regex.Match(src.Substring(i), @"^\w+")).Success)
                {
                    //Console.WriteLine("[" + m.Value + "]");
                    Token token = new Token();
                    token.body = m.Value;
                    tokens.Add(token);
                    i += m.Value.Length;
                    count++;
                    if (count > 2000)
                    {
                        //    break;
                    }
                }
                else if ((m = Regex.Match(src.Substring(i), @";")).Success)
                {
                    //Console.WriteLine("[" + m.Value + "]");
                    Token token = new Token();
                    token.body = m.Value;
                    tokens.Add(token);
                    i += m.Value.Length;
                }
                else
                {
                    Console.WriteLine(src[i]);
                    i++;
                }
            }

            return tokens;
        }

        private string GetClass(string src, string Name)
        {
            List<Token> tokens = Tokenize(src);

            for (int j = 0; j < tokens.Count - 3; j++)
            {
                // if (tokens[j + 1].body == "=" && tokens[j + 2].body == "new" && IsClass(tokens[j + 3].body))
                string className;
                if (tokens[j + 1].body == "=" && tokens[j + 2].body == "new" && (className = GetClassName(tokens, j + 3)) != null)
                {
                    if (tokens[j].body == Name)
                    {
                        return className;
                    }
                }
            }

            return null;
        }

        private string GetClassName(List<Token> tokens, int index)
        {
            if (types.ContainsKey(tokens[index].body) && types[tokens[index].body] != null)
            {
                if (index + 1 < tokens.Count && tokens[index + 1].body == ".")
                {
                    return GetClassName(tokens, index + 2);
                }
                else
                {
                    return tokens[index].body;
                }
            }
            return null;
        }

        private List<string> GetLocalWords(string src)
        {
            List<Token> tokens = Tokenize(src);
            List<string> words = new List<string>();

            for (int j = 0; j < tokens.Count - 1; j++)
            {
                if (tokens[j].body == "var")
                {
                    words.Add(tokens[j + 1].body);
                }
            }
            words.Add("if");
            words.Add("function");
            words.Add("while");
            words.Add("return");
            words.Add("var");
            words.Add("new");
            foreach (var a in types["enchant"].Members)
            {
                words.Add(a.Key);
            }


            words.Sort();
            return words;
        }
    }

}
