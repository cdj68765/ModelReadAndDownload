using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using FileOpera;
using Newtonsoft.Json;
using PmdFile.Pmd;
using Pmxfile;
using System.Threading.Tasks;

namespace Nico3D模型获取工具
{
    public partial class OpenForm : Form
    {
        public delegate void Showinfo(string value, int num);

        public static string BindataPath = Directory
            .CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache)).Parent.FullName;

        private string dispicurl = "";
        private List<string> DriveFile = new List<string>();
        private readonly List<string> DriveInfotemp = new List<string>();
        private Stream JsonStream;
        private string[] MODEL = new string[3];
        private readonly List<Data> ModelInfo = new List<Data>();

        private string path = "";
        private bool scendsearch = true;

        public Showinfo ShowInfo;

        private bool startGetDriveFile = false;

        private string td_num;
        private readonly HashSet<string> TexList = new HashSet<string>();

        public OpenForm()
        {
            InitializeComponent();
            ShowInfo = ShowInfoFun;
            Height = 140;
            Width = 464;
            TopMost = false;
            BringToFront();
            TopMost = true;
            GetDriveFile();
        }

        public void ShowInfoFun(string text, int num)
        {
            switch (num)
            {
                case 10:
                    listBox1.Items.Clear();
                    Application.DoEvents();
                    break;

                case 0:
                    MessageLabel.Text = text;
                    Application.DoEvents();
                    break;

                case 1:
                    if (!string.IsNullOrEmpty(text))
                    {
                        listBox1.Items.Add(text);
                        listBox1.SelectedIndex = listBox1.Items.Count - 1;
                    }
                    break;

                case 2:
                    if (!string.IsNullOrEmpty(text))
                        listBox2.SelectedIndex = Convert.ToInt32(text);
                    break;

                case 3:
                    if (!string.IsNullOrEmpty(text))
                    {
                        listBox3.Items.Add(text);
                        listBox3.SelectedIndex = listBox3.Items.Count - 1;
                    }
                    break;

                case 4:
                    Height = 140;
                    Width = 464;
                    listBox1.Visible = false;
                    listBox2.Visible = false;
                    listBox3.Visible = false;
                    Mod1.Visible = true;
                    Mod2.Visible = true;
                    label1.Text = text;
                    label1.Visible = true;
                    MessageLabel.Text = "等待";
                    textBox1.Visible = true;
                    button1.Visible = true;
                    if (text == "无法找到模型，请用IE浏览器加载完毕模型后再继续")
                        Process.Start("iexplore.exe",
                            @"3d.nicovideo.jp/externals/embedded?id=td" + td_num);
                    Application.DoEvents();
                    break;

                case 5:
                    toolStripProgressBar1.Value++;
                    break;

                case 55:
                    toolStripProgressBar1.Maximum = Convert.ToInt32(text);
                    toolStripProgressBar1.Value = 0;
                    break;
                case 555:
                    toolStripProgressBar1.Value = Convert.ToInt32(text);
                    break;
                case 6:
                    listBox1.Visible = false;
                    listBox2.Visible = true;
                    listBox3.Visible = true;
                    foreach (var temp in ModelInfo)
                        listBox2.Items.Add(temp.texname);
                    listBox2.SelectedIndex = 0;
                    break;

                case 7:
                    label1.Visible = false;
                    textBox1.Visible = false;
                    button1.Visible = false;
                    Mod1.Visible = false;
                    Mod2.Visible = false;
                    Height = 240;
                    Width = 624;
                    listBox1.Visible = true;
                    break;

                case 33:
                    listBox3.Items.Clear();
                    break;
            }
        }

        private void GetDriveInfo()
        {
            startGetDriveFile = true;

            DriveInfotemp.AddRange(new DriveInfo(BindataPath).EnumerateFiles().ToArray());
            foreach (var Temp in DriveInfotemp.Where(Temp => Temp.Contains(BindataPath)))
                DriveFile.Add(Temp);
            startGetDriveFile = false;
        }

        private void GetDriveFile()
        {
            ThreadStart ts = GetDriveInfo;
            var th = new Thread(ts);
            th.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TexList.Clear();
            ModelInfo.Clear();
            MODEL = new string[3];
            if (startGetDriveFile != true)
            {
                DriveFile.Clear();
                GetDriveFile();
            }
            try
            {
                var downloadsArr = textBox1.Text.Split(Convert.ToChar("d"));
                int.Parse(downloadsArr[downloadsArr.Length - 1]);
                td_num = downloadsArr[downloadsArr.Length - 1];
            }
            catch (Exception)
            {
                MessageBox.Show("输入的网址有误", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var save = new FolderBrowserDialog
            {
                ShowNewFolderButton = true,
                Description = "请选择保存目录"
            };

            save.ShowDialog();
            if (save.SelectedPath == "")
            {
                return;
            }
            Invoke(ShowInfo, "", 7);
            if (!Downloadjson()) return;
            path = save.SelectedPath;
            if (Mod2.Checked)
            {
                var newThread = new Thread(Start);
                newThread.SetApartmentState(ApartmentState.STA);
                newThread.Start();
            }
            else
            {
                var GetWebInfo = true;
                var Browser = new WebBrowser
                {
                    ScriptErrorsSuppressed = true
                };
                Browser.DocumentCompleted += delegate
                {
                    GetWebInfo = false;
                };
                Browser.Navigate("http://3d.nicovideo.jp/externals/embedded?id=td" + td_num, "_self", null,
                    "User-Agent: Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko");

                Task.Factory.StartNew(() =>
                {
                    if (AnalyseJson())
                    {
                        if (MessageLabel.Text == "Json反序列化成功")
                        {
                            Invoke(ShowInfo, "", 10);
                            Invoke(ShowInfo, "正在等待网络响应", 0);
                            Invoke(ShowInfo, "100", 55);
                            while (GetWebInfo)
                            {
                                if (toolStripProgressBar1.Value == 100)
                                {
                                    Invoke(ShowInfo, "100", 55);
                                }
                                Thread.Sleep(10);
                                Invoke(ShowInfo, "", 5);
                            }
                            Invoke(ShowInfo, "正在下载模型数据中", 0);
                            var DownloadModel = new DownloadHelper();
                            ThreadPool.QueueUserWorkItem(obj =>
                            {
                                while (DownloadModel.DownloadStatus != "正在下载模型数据中")
                                {
                                    Invoke(ShowInfo, DownloadModel.DownloadStatus, 0);
                                    Thread.Sleep(100);
                                }
                                Invoke(ShowInfo, DownloadModel.End.ToString(), 55); Invoke(ShowInfo, DownloadModel.DownloadStatus, 0);

                                while (DownloadModel.DownloadStatus != "下载完成")
                                {
                                    Invoke(ShowInfo, DownloadModel.current.ToString(), 555);
                                    Thread.Sleep(50);
                                }
                            });
                            if (DownloadModel.DownloadFile(
                                $"http://3d.nicovideo.jp/upload/limited_contents/td{td_num}/{MODEL[1].Replace("[1]", "")}.{MODEL[2]}",
                                out byte[] ModelData))
                            {
                                if (Decrypt(ModelData))
                                {
                                    Invoke(ShowInfo, ModelInfo.Count.ToString(), 55);
                                    Invoke(ShowInfo, MODEL[0] + "->下载成功", 1);
                                    Invoke(ShowInfo, "正在下载贴图数据中", 0);
                                    foreach (var TexInfo in ModelInfo)
                                    {
                                        Invoke(ShowInfo, "", 5);
                                        if (new DownloadHelper().DownloadFile(
                                            $"http://3d.nicovideo.jp/upload/limited_contents/td{td_num}/{TexInfo.texurl0.Replace("[1]", "")}.{TexInfo.texurl1}",
                                            out byte[] TexData))
                                        {
                                            var Addpath = "";
                                            if (TexList != null)
                                                foreach (var temp in TexList)
                                                {
                                                    var TEMP = temp.Split('\\');
                                                    if (TEMP.Length != 1)
                                                        if (TEMP[1] == TexInfo.texname)
                                                        {
                                                            if (!Directory.Exists(path + @"\" + TEMP[0]))
                                                            {
                                                                var dir = new DirectoryInfo(path);
                                                                dir.CreateSubdirectory(TEMP[0]);
                                                            }
                                                            Addpath = temp;
                                                            break;
                                                        }
                                                }
                                            if (Addpath != "")
                                                using (var SAVE = new FileStream($"{path}\\{Addpath}",
                                                    FileMode.CreateNew))
                                                {
                                                    SAVE.WriteAsync(TexData, 0, TexData.Length).Wait();
                                 
                                                }
                                            else
                                                using (var SAVE = new FileStream($"{path}\\{TexInfo.texname}",
                                                    FileMode.CreateNew))
                                                {
                                                    SAVE.WriteAsync(TexData, 0, TexData.Length).Wait();
                                                }
                                            Invoke(ShowInfo, TexInfo.texname + "->下载成功", 1);

                                        }
                                        else
                                        {
                                            Invoke(ShowInfo, TexInfo.texname + "->下载失败", 1);
                                        }
                                    }
                                    Invoke(ShowInfo, "模型解析成功", 4);
                                }
                                else
                                {
                                    Invoke(ShowInfo, "模型解析失败", 4);
                                }
                            }
                        }
                    }
                });
            }
        }

        private void Start()
        {
            var newThread = new Thread(AnalyseModel);
            newThread.SetApartmentState(ApartmentState.STA);
            if (AnalyseJson())
            {
                if (MessageLabel.Text == "Json反序列化成功")
                {
                    Invoke(ShowInfo, "", 10);
                    Invoke(ShowInfo, DriveFile.Count.ToString(), 55);
                    newThread.Start();
                }
            }
        }

        private void CopyTex()
        {
            try
            {
                Invoke(ShowInfo, "", 6);
                Invoke(ShowInfo, "正在搜索并复制贴图", 0);
                Invoke(ShowInfo, ModelInfo.Count.ToString(), 55);

                for (var j = 0; j < ModelInfo.Count; j++)
                {
                    Invoke(ShowInfo, j.ToString(), 2);
                    Invoke(ShowInfo, ModelInfo.Count.ToString(), 5);
                    var newThread = new Thread(ShowSearch);
                    newThread.SetApartmentState(ApartmentState.MTA);
                    if (new FileInfo(path + @"\" + ModelInfo[j].texname).Exists != true)
                    {
                        newThread.Start();
                        foreach (string file in DriveFile)
                            try
                            {
                                var newfile = new FileInfo(file);
                                if (newfile.Name == ModelInfo[j].texurl0 + "." + ModelInfo[j].texurl1)
                                {
                                    Thread.Sleep(500);
                                    newThread.Abort();
                                    var Addpath = "";
                                    if (TexList != null)
                                        foreach (var temp in TexList)
                                        {
                                            var TEMP = temp.Split('\\');
                                            if (TEMP.Length != 1)
                                                if (TEMP[1] == ModelInfo[j].texname)
                                                {
                                                    if (Directory.Exists(path + @"\" + TEMP[0]) == false)
                                                    {
                                                        var dir = new DirectoryInfo(path);
                                                        dir.CreateSubdirectory(TEMP[0]);
                                                    }
                                                    Addpath = temp;
                                                    break;
                                                }
                                        }
                                    if (Addpath != "")
                                        File.Copy(newfile.FullName, path + @"\" + Addpath, true);
                                    else
                                        File.Copy(newfile.FullName, path + @"\" + ModelInfo[j].texname, true);
                                    break;
                                }
                            }
                            catch (Exception)
                            {
                            }
                    }
                    else
                    {
                        newThread.Abort();
                    }
                }
            }
            catch (Exception)
            {
            }
            try
            {
                Invoke(ShowInfo, "模型解析完毕", 4);
            }
            catch (Exception)
            {
            }
        }

        public void AnalyseModel()
        {
            try
            {
                Invoke(ShowInfo, "正在搜索模型", 0);
                var newThread = new Thread(ShowModelSearch);
                newThread.SetApartmentState(ApartmentState.MTA);
                newThread.Start();
                for (var i = 0; i < DriveFile.Count; i++)
                {
                    try
                    {
                        
                        if (new FileInfo(DriveFile[i]).Name == MODEL[1] + "." + MODEL[2])
                        {
                            Thread.Sleep(1000);
                            newThread.Abort();
                            Invoke(ShowInfo, DriveInfotemp[i], 1);
                            if (Decrypt(File.ReadAllBytes(new FileInfo(DriveFile[i]).FullName)))
                            {
                                Invoke(ShowInfo, "模型解析成功", 0);
                                ThreadStart ts = CopyTex;
                                var th = new Thread(ts);
                                th.Start();
                                break;
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                    if (i == DriveFile.Count - 1)
                    {
                        newThread.Abort();
                        if (scendsearch)
                        {
                            scendsearch = false;
                            DriveFile = new List<string>(DriveInfotemp);
                            AnalyseModel();
                        }
                        else
                        {
                            Invoke(ShowInfo, "无法找到模型，请用IE浏览器加载完毕模型后再继续", 4);
                        }
                        break;
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        bool Decrypt(byte[] file)
        {
            var timeout = 0;
            string saveextsion = null;
            Invoke(ShowInfo, "正在解析模型", 0);
            switch (MODEL[2])
            {
                case "plg":
                    timeout = 33249;
                    saveextsion = "mqo";
                    break;
                case "xs3":
                    timeout = 80128;
                    saveextsion = "pmx";
                    break;
                case "mod":
                    timeout = 27731;
                    saveextsion = "obj";
                    break;
                case "plgx":
                    timeout = 114514;
                    saveextsion = "pmd";
                    break;
                case "ncsc":
                    timeout = 215191;
                    saveextsion = "cscu";
                    break;
                case "plm":
                    timeout = 536900;
                    saveextsion = "plm";
                    break;
                default:
                    var filename = $"{path}\\{MODEL[0].Replace(":", "")}.{MODEL[2]}";
                    if (File.Exists(filename))
                    {
                        Invoke(ShowInfo, "模型已经存在", 4);
                        return false;
                    }
                    using (var fs = new FileStream(filename, FileMode.CreateNew))
                    {
                        var w = new BinaryWriter(fs);
                        w.Write(file);
                    }
                    return false;
            }
            var cGMLZMA = new CGMLZMA(file.Length ^ timeout);
            using (var memoryStream = new MemoryStream(file))
            {
                using (var memoryStream2 = new MemoryStream())
                {
                    while (true)
                    {
                        var num = (int)Math.Min(memoryStream.Length - memoryStream.Position, 4L);
                        if (num <= 0)
                            break;
                        var array = new byte[4];
                        memoryStream.Read(array, 0, num);
                        if (num == 4)
                        {
                            var num2 = BitConverter.ToUInt32(array, 0) ^ cGMLZMA.NextUInt32();
                            array = BitConverter.GetBytes(num2);
                        }
                        memoryStream2.Write(array, 0, num);
                    }
                    try
                    {
                        memoryStream2.Position = 0L;
                        switch (saveextsion)
                        {
                            case "pmx":
                                var pmxfile = PmxFile.FromStream(memoryStream2, null);
                                foreach (var MaterialListList in pmxfile.MaterialList)
                                {
                                    if (MaterialListList.Tex != "")
                                        if (!TexList.Contains(MaterialListList.Tex))
                                            TexList.Add(MaterialListList.Tex);
                                    if (MaterialListList.Sphere != "")
                                        if (!TexList.Contains(MaterialListList.Sphere))
                                            TexList.Add(MaterialListList.Sphere);
                                    if (MaterialListList.Toon != "")
                                        if (!TexList.Contains(MaterialListList.Toon))
                                            TexList.Add(MaterialListList.Toon);
                                }
                                break;
                            case "pmd":
                                var pmdfile = new MMDModel(memoryStream2, 1.0f);
                                foreach (var MaterialListList in pmdfile.Materials)
                                {
                                    if (MaterialListList.TextureFileName != "")
                                        if (!TexList.Contains(MaterialListList.TextureFileName))
                                            TexList.Add(MaterialListList.TextureFileName);
                                    if (MaterialListList.SphereTextureFileName != "")
                                        if (!TexList.Contains(MaterialListList.SphereTextureFileName))
                                            TexList.Add(MaterialListList.SphereTextureFileName);
                                }
                                foreach (var ToonFileNamesList in pmdfile.ToonFileNames
                                    .Where(ToonFileNamesList => ToonFileNamesList != "")
                                    .Where(ToonFileNamesList => !TexList.Contains(ToonFileNamesList)))
                                    TexList.Add(ToonFileNamesList);
                                break;
                        }
                        var filename = path + @"\" + MODEL[0].Replace(":", "") + "." + saveextsion;
                        if (File.Exists(filename))
                        {
                            Invoke(ShowInfo, "模型已经存在", 4);
                            return false;
                        }
                        using (var fs = new FileStream(filename, FileMode.CreateNew))
                        {
                            var w = new BinaryWriter(fs);
                            w.Write(memoryStream2.ToArray());
                        }
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        private void ShowModelSearch()
        {
            foreach (var dir in DriveInfotemp)
                listBox1.Invoke(ShowInfo, dir, 1);
        }

        private void ShowSearch()
        {
            try
            {
                Invoke(ShowInfo, "", 33);
                foreach (var dir in DriveInfotemp)
                    Invoke(ShowInfo, dir, 3);
            }
            catch (Exception)
            {
            }
        }

        private bool AnalyseJson()
        {
            try
            {
                Invoke(ShowInfo, "正在解析Json数据", 0);
                {
                    var restex = false;
                    JsonReader reader =
                        new JsonTextReader(new StringReader(new StreamReader(JsonStream, Encoding.GetEncoding("utf-8"))
                            .ReadToEnd())); //无论如何 都从服务器取得一份json
                    var temp = new List<jsondata>();
                    var Texname = new List<string>();
                    var Urlname = new List<string>();
                    var Urlest = new List<string>();
                    while (reader.Read())
                        temp.Add(new jsondata(reader.Path, reader.Value));
                    Invoke(ShowInfo, temp.Count.ToString(), 55);
                    foreach (var Temp in temp)
                    {
                        Invoke(ShowInfo, "", 5);
                        Invoke(ShowInfo, Temp.Path + "\t" + Temp.Value, 1);
                        var stra = Temp.Path;
                        var getname = Convert.ToString(Temp.Value);
                        var sArr = stra.Split('.');
                        if (restex)
                            foreach (var texname in from s in sArr
                                                    where s.Equals("url")
                                                    where getname != "url"
                                                    select getname.Split('/')
                                                    into urlname
                                                    select urlname[6].Split('.'))
                            {
                                Urlname.Add(texname[0] + "[1]");
                                Urlest.Add(texname[1]);
                                restex = false;
                            }
                        foreach (var s in sArr.Where(s => s.Equals("name")).Where(s => getname != "name"))
                        {
                            Texname.Add(getname);
                            restex = true;
                        }

                        if (stra == "work.title")
                            if (getname != "title")
                            {
                                getname = getname.Replace(":", "").Replace((char) 34, (char) 45);
                                MODEL[0] = getname;
                                var dir = new DirectoryInfo(path);
                                dir.CreateSubdirectory(getname);
                                path = path + @"\" + getname.Replace(":", "");
                                if (new FileInfo(path + @"\" + @"folder.jpg").Exists == false)
                                    if (dispicurl != "")
                                    {
                                        var newThread = new Thread(Downloaddispic);
                                        newThread.SetApartmentState(ApartmentState.STA);
                                        newThread.Start();
                                    }
                            }
                        if (stra == "components[0].url")
                            if (getname != "url")
                            {
                                var name = getname.Split('/')[6].Split('.');
                                MODEL[1] = name[0] + "[1]"; //模型全的乱码
                                MODEL[2] = name[1]; //模型的后缀
                            }
                        if (stra == "components[0].display_url")
                            if (getname != "display_url")
                                dispicurl = getname;
                    }
                    for (var i = 0; i < Urlname.Count; i++)
                        if (Texname[i] != null)
                            if (ModelInfo.All(x => x.texname != Texname[i]))
                                ModelInfo.Add(new Data(Texname[i], Urlname[i], Urlest[i]));
                }
            }
            catch (Exception)
            {
                try
                {
                    Invoke(ShowInfo, "Json解析失败，请重试", 4);
                }
                catch (Exception)
                {
                }
                return false;
            }
            Invoke(ShowInfo, "Json反序列化成功", 0);
            return true;
        }

        private void Downloaddispic()
        {
            try
            {
                var client = new WebClient();
                client.DownloadFile(new Uri(dispicurl), path + @"\" + @"folder.jpg");
            }
            catch (Exception)
            {
            }
        }

        private bool Downloadjson()
        {
            try
            {
                MessageLabel.Text = "正在下载json数据中";

                var request =
                    (HttpWebRequest) WebRequest.Create("http://3d.nicovideo.jp/works/td" + td_num + "/components.json");
                request.Method = "GET";
                var response = (HttpWebResponse) request.GetResponse();
                JsonStream = response.GetResponseStream();
                return true;
            }
            catch (Exception)
            {
                label1.Text = "下载配置数据错误，请检查地址的正确性再重试";
            }
            return false;

            /*  try
              {
                  if (new FileInfo(Jsonpath).Exists)
                  {
                      File.Delete(Jsonpath);
                  }
                  WebClient client = new WebClient();
                  client.DownloadFile("http://3d.nicovideo.jp/works/td" + td_num + "/components.json", Jsonpath);
                  return true;
              }
              catch (Exception)
              {
                  this.Invoke(Changeform, "Json下载失败，请确认后重试");
              }*/
        }

        private class jsondata
        {
            public readonly string Path;
            public readonly string Value;

            public jsondata(string path, object value)
            {
                Path = path;
                Value = value?.ToString() ?? "";
            }
        }

        public class CGMLZMA
        {
            private uint w;
            private uint x;
            private uint y;
            private uint z;

            public CGMLZMA(int seed)
            {
                x = (uint) seed;
                y = 362436069u;
                z = 521288629u;
                w = 88675123u;
            }

            public uint NextUInt32()
            {
                var num = x ^ (x << 11);
                x = y;
                y = z;
                z = w;
                return w = w ^ (w >> 19) ^ num ^ (num >> 8);
            }
        }

        private class Data
        {
            public readonly string texname;
            public readonly string texurl0;
            public readonly string texurl1;

            public Data(string path, string value, string v)
            {
                texname = path;
                texurl0 = value;
                texurl1 = v;
            }
        }
    }
}