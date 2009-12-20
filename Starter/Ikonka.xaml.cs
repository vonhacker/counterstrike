using doru;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.InteropServices;
using ManagedWinapi.Windows;
using System.Windows.Media.Animation;
using ShellLib;
using System.Threading;

namespace Starter
{

    public partial class Ikonka : UserControl
    {

        public string path { get { return item.path; } set { item.path = value; } }
        public string key { get { return item.keyword; } set { item.keyword = value; } }
        public DateTime dt { get { return item.dt; } set { item.dt = value; } }
        Process process { get { return wnd.Process; } }
        public SystemWindow wnd;
        public string name { set { txt.Text = value; } }
        public Item item = new Item();
        public Ikonka()
        {
            InitializeComponent();
        }
        public double ow, oh;
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ow = Width;
            oh = Height;
        
        }

        
        
        Point mouse { get { return Mouse.GetPosition(this); } }
        
        public void Load()
        {

            if (path != null && (File.Exists(path) || Directory.Exists(path)))
            {
                var bmp = GetIcon(path);
                if (bmp != null)
                    img.Source = bmp;

                txt.Text = getname(path);
                if (txt.Text == "") txt.Text = path;
            }
            else
                txt.Text = path;
            Window1._This.stackPanel.Children.Add(this);
        }
        public static string getname(string path)
        {
            var crs = new[] { '\\', '/' };
            return path.Substring(path.Trim(crs).LastIndexOfAny(crs)).Trim(crs);
        }
        Window1 window = Window1._This;
        Storyboard sb { get { return (Storyboard)Resources["OnMouseEnter1"]; } }

        IEnumerable<string> dest(IEnumerable<string> srs,string d)
        {
            foreach (var a in srs)
                yield return d+@"\"+Path.GetFileName(a);
        }
        
        protected override void OnDrop(DragEventArgs e)
        {
            if (e.Data is System.Windows.DataObject && ((System.Windows.DataObject)e.Data).ContainsFileDropList() && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                var srs =((System.Windows.DataObject)e.Data).GetFileDropList().Cast<string>();

                path = H.GetLnk(path) ?? path;
                if (Directory.Exists(path))
                {
                    ShellFileOperation fo = new ShellFileOperation();
                    fo.SourceFiles = srs.ToArray();
                    fo.DestFiles = dest(srs, path).ToArray();
                    fo.Operation = ShellLib.ShellFileOperation.FileOperations.FO_MOVE;
                    fo.DoOperation();
                    
                }
                e.Handled = true;
            }
            
           
            base.OnDrop(e);
            window.Hide();
        }
        public void onmouseenter()
        {
            window.TextBlock1.Text = path.Replace("\r", "", "\n", "");
            foreach (var a in window.ikonki)                                
                a.sb.Stop();
            sb.Begin();
            window.selectedicon = this;
        }
        protected override void OnMouseLeave(MouseEventArgs e)
        {            
            base.OnMouseLeave(e);
        }
        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            window.Hide();
            Clipboard.SetText(path);
            var a = new ManagedWinapi.KeyboardKey(System.Windows.Forms.Keys.LControlKey);
            var b = new ManagedWinapi.KeyboardKey(System.Windows.Forms.Keys.V);
            a.Press(); b.Press(); a.Release(); b.Release();
            Top();
            
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if(Keyboard.IsKeyDown(Key.LeftCtrl))
                Delete();
            else
                Start();
        }
        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern int SetForegroundWindow(IntPtr hWnd);

        public void Start()
        {
            
            if (wnd != null)
            { 
                if (IsIconic(wnd.HWnd))
                    ShowWindow(wnd.HWnd, 9);
                else
                    SetForegroundWindow(wnd.HWnd);
            }
            else
            {
                try
                {
                    Thread t =new Thread(delegate()
                    {
                        try
                        {
                        Process. Start(this.path);
                        }
                        catch (Exception e) { MessageBox.Show(e.Message); }
                    });
                    t.Start();
                    t.Join(1000);
                }
                catch (Win32Exception) { }
                Top();
            }
            window.Hide();
        }

        private void Top() 
        {  
            if (window.txt != "")
            {
                var b = Lamba1();
                if (b == null)  
                {
                    b = item.Clone();
                    db.favs.Add(b);
                    b.keyword = window.txt;
                }
                b.dt = DateTime.Now;
            }
            _Window1.UpdateSearch();
            _Window1.SaveXml();
        }

        private Item Lamba1()
        {
            var b = db.favs.FirstOrDefault(a => a.path == path && a.keyword.StartsWith(window.txt, true, null));
            return b;
        }
        public uint WM_COMMAND = 0x0112;

        public int WM_CLOSE = 0xF060;

        [DllImport("user32.dll")]

        public static extern int SendMessage(int hWnd, uint Msg, int wParam, int lParam);
        

        private void Delete()
        {
            if (wnd != null)
            {
                SendMessage((int)wnd.HWnd, WM_COMMAND, WM_CLOSE, 0);
                window.stackPanel.Children.Remove(this);
            }
            else
            {
                db.favs.Remove(this.item);
                _Window1.UpdateSearch();
                _Window1.SaveXml();
            }
        }
        
        public static ImageSource GetIcon(string s)
        {
            
            System.Drawing.Icon icon = ManagedWinapi.ExtendedFileInfo.GetIconForFilename(s, false);
            if (icon == null) return null;
            using (MemoryStream ms = new MemoryStream())
            {
                icon.Save(ms);
                
                var dec = new IconBitmapDecoder(ms, BitmapCreateOptions.PreservePixelFormat,BitmapCacheOption.Default);                
                return dec.Frames[0];
            }
        }
        Window1 _Window1 = Window1._This;
        DB db { get { return _Window1.db; } }
    }
}
