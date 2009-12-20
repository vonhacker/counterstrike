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
using System.Xml.Serialization;
using System.Globalization;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Media.Animation;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

using System.Runtime.Remoting;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Win32;

using Keys=System.Windows.Forms.Keys;

namespace Starter
{
    
    public partial class Window1 : Window
    {
        public static Window1 _This;
        public string txt = "";
        XmlSerializer xmls = new XmlSerializer(typeof(DB));
        List<Item> files = new List<Item>();

        Point mouse;
        public IEnumerable<Ikonka> ikonki { get { return stackPanel.Children.Cast<Ikonka>(); } }

        public DB db = new DB();

        public Window1()
        {

            _This = this;
            InitializeComponent();
        }
        protected override void OnInitialized(EventArgs e)
        {
            if (Assembly.GetCallingAssembly() == null) return;
            Logging.Setup();
            base.OnInitialized(e);
            EnableHotkey();
            //EnableAltTabHotkey();
            ShowTrayIcon();
            LoadXml();
            UpdateDirs();
            Dispatcher.StartUpdate(Update2);
            UpdateSearch();
        }
        Point oldm;
        void Update2()
        {

            if (this.IsActive)
            {
                
                var p = ManagedWinapi.Crosshair.MousePosition;
                mouse = new Point(p.X - Left, p.Y - Top);
                if (mouse != oldm && oldm != default(Point))
                {
                    onmousemove();
                }
                oldm = mouse;
            }
        }
        private void LoadXml()
        {
            xmls.Deserialize("db.xml", ref db);
        }

        ManagedWinapi.Hooks.LowLevelKeyboardHook _LowLevelKeyboardHook = new ManagedWinapi.Hooks.LowLevelKeyboardHook();
       
        private void EnableHotkey()
        {

            _LowLevelKeyboardHook.KeyIntercepted += new ManagedWinapi.Hooks.LowLevelKeyboardHook.KeyCallback(OnGlobalKeyDown);
            _LowLevelKeyboardHook.StartHook();
        }


        Keys oldkey;
        void OnGlobalKeyDown(int msg, int vkCode, int scanCode, int flags, int time, IntPtr dwExtraInfo, ref bool handled)
        {
           
            if (flags == 1 || flags == 0)
            {
                Keys key = (Keys)vkCode;
                if (oldkey == Keys.LWin && key == Keys.Space)
                {
                    handled = true;
                    UpdateSearch();
                    show();
                }
                oldkey = (Keys)vkCode;
            }

        }

   




       



        public static BitmapSource loadBitmap(System.Drawing.Bitmap source)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(source.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
        }

        DateTime dtmove;

        TaskbarIcon ti;
        private void ShowTrayIcon()
        {

            ti = new TaskbarIcon();
            ti.Icon = new System.Drawing.Icon("default.ico");

            ti.PreviewTrayContextMenuOpen += new RoutedEventHandler(OnTaskbarIcon);
        }
        string destkoptxt = "DeskTops.txt";
        void OnTaskbarIcon(object sender, RoutedEventArgs e)
        {
            ti.ContextMenu = new ContextMenu();
            foreach (string s in File.ReadAllLines(destkoptxt))
            {
                var a = new MenuItem();
                a.Header = s;
                a.PreviewMouseDown += new MouseButtonEventHandler(SetDesktop);
                ti.ContextMenu.Items.Add(a);
            }

            var setDesktop = new MenuItem() { Header = "Set Desktop From Clipboard" };
            setDesktop.Click += new RoutedEventHandler(addDesktop);
            ti.ContextMenu.Items.Add(setDesktop);

            var Exit = new MenuItem() { Header = "Exit" };
            Exit.Click += new RoutedEventHandler(delegate { Close(); });
            ti.ContextMenu.Items.Add(Exit);

        }
        void addDesktop(object sender, RoutedEventArgs e)
        {
            if (Clipboard.GetText() != "" && Clipboard.GetText()!=null)
                AddDesktopToTxt(Clipboard.GetText());
        }        
        void SetDesktop(object sender, MouseButtonEventArgs e)
        {
            MenuItem a = (MenuItem)sender;
            string s = (string)a.Header;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Process.Start(s);
            }
            else
            {
                SetDesktop(s);
            }

        }
        private void SetDesktop(string fn)
        {

            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders\",
                "Desktop", fn);
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders\",
                    "Desktop", fn);
            AddDesktopToTxt(fn);
            Process.GetProcessesByName("explorer")[0].Kill();
        }

        private void AddDesktopToTxt(string fn)
        {
            List<string> l = File.ReadAllLines(destkoptxt).ToList();
            l.Add(fn);
            File.WriteAllLines(destkoptxt, l.Distinct().Take(10).ToArray());
        }

        
        protected override void OnClosed(EventArgs e)
        {
            //ni.Visible = false;
            base.OnClosed(e);
        }
        private void UpdateDirs()
        {
            foreach (var a in db.folders)
                GetDirs(a.path, a.level);

        }
        void Update()
        {

            Update(GetMousePos(this.stackPanel));
        }
        void Update(Point pos)
        {

            double minvalue = double.MaxValue;
            Ikonka i = null;
            foreach (Ikonka ik in ikonki)
            {
                Vector p = VisualTreeHelper.GetOffset(ik);

                double a = Math.Min(minvalue, Math.Abs(p.X - pos.X + ik.Width / 2));
                if (a < minvalue)
                {
                    i = ik;
                    minvalue = a;
                }
                var dest = 150;
                var b = Math.Max(0, dest - Math.Abs((pos.X - (ik.Width / 2)) - p.X)) / dest * .8;
                ik._ScaleTransform.ScaleX = ik._ScaleTransform.ScaleY = 1 + b;
                ik._ScaleTransform.CenterX = ik.Width / 2;
                ik._ScaleTransform.CenterY = ik.Height / 2;
                ik.Width = ik.ow + ik.ow * b;
                ik.Height = ik.oh + ik.oh * b;
            }
            if (i != null && selectedicon != i)
                i.onmouseenter();

        }

        private void UpdateCursorpos(int offset)
        {

            if (offset < 0 && ikonki.Prev(selectedicon) != null) ikonki.Prev(selectedicon).onmouseenter();
            else if(ikonki.Next(selectedicon) != null) ikonki.Next(selectedicon).onmouseenter();
        }



        public void UpdateSearch()
        {

            IEnumerable<Item> list = NewMethod();

            //list = _DB.favs.Where(a => comp(a.keyword)).OrderBy(a => a.dt).Select(a => a.path).Concat(list);
            this.stackPanel.Children.Clear();
            foreach (var item in list.Distinct().Take(8))
            {
                Ikonka li = new Ikonka();
                li.item = item;
                li.Load();
            }
            if (ikonki.Count() > 0)
                ikonki.First().onmouseenter();
        }

        private IEnumerable<Item> NewMethod()
        {
            IEnumerable<Item> list = db.favs.Concat(files).Where(a => Check(a.keyword)).OrderBy(delegate(Item a)
            {
                return getsortid(a.keyword);
            }).ThenByDescending(a => a.dt);
            return list;
        }

        public Ikonka selectedicon;
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                Item ikon = new Item() { path = Clipboard.GetText() };
                ikon.dt = DateTime.Now;
                ikon.keyword = txt;
                db.favs.Add(ikon);
                UpdateSearch();
                e.Handled = true;
            }
            base.OnPreviewKeyDown(e);
        }
        protected override void OnPreviewKeyUp(KeyEventArgs e)
        {
            //bool alt = e.SystemKey == Key.LeftAlt || e.Key == Key.LeftAlt;

            if (e.Key == Key.Escape)
                this.Hide();
            if (e.Key == Key.Left)
                UpdateCursorpos(-1);
            if (e.Key == Key.Right)
                UpdateCursorpos(1);
            if (e.Key == Key.Enter)
                if (selectedicon != null) selectedicon.Start();
            base.OnPreviewKeyUp(e);
        }

        bool isMouseOver()
        {
            return new Rect(0, 0, Width, Height).Contains(mouse);
        }
        protected override void OnDrop(DragEventArgs e)
        {

            if (e.Data is System.Windows.DataObject && ((System.Windows.DataObject)e.Data).ContainsFileDropList())
            {
                foreach (string filePath in ((System.Windows.DataObject)e.Data).GetFileDropList())
                {

                    Item ikon = new Item() { path = filePath };
                    ikon.dt = DateTime.Now;
                    ikon.keyword = txt + " " + replace(Ikonka.getname(filePath));
                    db.favs.Add(ikon);                    
                }
                UpdateSearch();
                SaveXml();
            }

        }
        //protected override void OnMouseLeave(MouseEventArgs e)
        //{
        //    Update(new Point(-1000, -1000));
        //    base.OnMouseLeave(e);
        //}

        //protected override void OnPreviewMouseMove(MouseEventArgs e)
        //{
        //    //onmousemove();            
        //    base.OnMouseMove(e);
        //}
        int oldtm;
        private void onmousemove()
        {

            if (!isMouseOver()) this.Hide();
            //if (Math.Abs(DateTime.Now.Millisecond - oldtm) < 200)
            if (DateTime.Now - dtmove > TimeSpan.FromSeconds(.2))
                Update();

            oldtm = DateTime.Now.Millisecond;
        }
        public Point GetMousePos(UIElement ui)
        {
            return this.TranslatePoint(mouse, ui);

        }
        private void Textbox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            //txt = TextBox1.Text.Replace(" ", " ?");
            txt = replace(TextBox1.Text);
            UpdateSearch();

        }


        private void GetDirs(string path, int level)
        {

            //try
            //{
            //    path=Environment.GetFolderPath((Environment.SpecialFolder)Enum.Parse(typeof(Environment.SpecialFolder), path, true));
            //}
            //catch { }
            var itm = new Item { path = path, keyword = replace(Ikonka.getname(path)) };
            files.Add(itm);
            if (level == 0) return;
            foreach (var b in Directory.GetFiles(path))
            {
                var ke = Ikonka.getname(b);
                if (!files.Any(a => Ikonka.getname(a.path) == ke))
                    files.Add(new Item { path = b, keyword = replace(ke) });
            }
            foreach (var b in Directory.GetDirectories(path))
                if (Directory.Exists(b))
                    GetDirs(b, level - 1);
        }



        public void SaveXml()
        {
            xmls.Serialize("db.xml", db);
        }
        protected override void OnDeactivated(EventArgs e)
        {
            this.Hide();
            base.OnDeactivated(e);
        }
      
        private void show()
        {
            dtmove = DateTime.Now;
            System.Drawing.Point p = ManagedWinapi.Crosshair.MousePosition;
            this.Show();
            this.Activate();
            this.TextBox1.Focus();
            this.TextBox1.SelectAll();
            UpdateLayout();
            Point s = new Point(Width / 2, Height / 2);
            if (selectedicon.Parent != null)
                s = selectedicon.TranslatePoint(new Point(selectedicon.Width / 2, selectedicon.Height / 2), this);
            Left = p.X - s.X;
            Top = p.Y - s.Y;
        }

        string replace(string s)
        {
            return Regex.Replace(s, "[^A-Z0-9 ]", "", RegexOptions.IgnoreCase);
        }
        private bool Check(string s)
        {
            return getsortid(s) != uint.MaxValue;
        }
        private uint getsortid(string a)
        {
            if (txt.Length == 0) return 0;
            uint c = uint.MaxValue;
            foreach (string b in txt.Split(" "))
            {
                c = (uint)Math.Min(c, a.IndexOf(b, StringComparison.CurrentCultureIgnoreCase));
                if (c == uint.MaxValue) return c;
            }
            return c;
        }
    }

}
//if (!db.favs.Any(a => a.path == filePath))