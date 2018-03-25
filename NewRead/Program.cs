using System;
using System.Reflection;
using System.Windows.Forms;
using Nico3D模型获取工具;

namespace NewRead
{
    internal static class Program
    {
        /// <summary>
        ///     应用程序的主入口点。
        /// </summary>
        [STAThread]
        private static void Main()
        {
            EmbeddedAssembly.Load("Nico3D模型获取工具.Newtonsoft.Json.dll", "Newtonsoft.Json.dll");
            EmbeddedAssembly.Load("Nico3D模型获取工具.PEPlugin.dll", "PEPlugin.dll");
            EmbeddedAssembly.Load("Nico3D模型获取工具.PMDEditorLib.dll", "PMDEditorLib.dll");
            EmbeddedAssembly.Load("Nico3D模型获取工具.SlimDX.dll", "SlimDX.dll");
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new OpenForm());
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return EmbeddedAssembly.Get(args.Name);
        }
    }
}