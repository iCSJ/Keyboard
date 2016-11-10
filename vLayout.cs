using System;
using System.Xml;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Resources;
using System.Windows.Input;

namespace OpenKeyboard
{
    public abstract class vLayout
    {
        public static FontFamily mIconFont = new FontFamily(new Uri("pack://application:,,,/fonts/#FontAwesome"), "./#FontAwesome");
        public static List<tButton> tButtonList = new List<tButton>();
        public static string keyboard;
        public static bool Load(string fName, Grid uiGrid, Window uiWindow, Point? location=null)
        {
            try
            {
                //..........................................
                //Load up Layout XML
                vLayout.keyboard = fName;
                Uri uriResource = new Uri(string.Format("Layouts/{0}.xml", fName), UriKind.Relative);
                StreamResourceInfo resourceStream = Application.GetResourceStream(uriResource);
                XmlDocument xml = new XmlDocument();
                xml.Load(resourceStream.Stream);

                XmlElement root = xml.DocumentElement;
                if (root.ChildNodes.Count == 0) return false;

                //..........................................
                //Set window size and position
                double sHeight = SystemParameters.WorkArea.Height;
                double sWidth = SystemParameters.WorkArea.Width;

                uiWindow.Width = double.Parse(root.GetAttribute("width"));
                uiWindow.Height = double.Parse(root.GetAttribute("height"));

                switch (root.GetAttribute("vpos"))
                {
                    case "top": uiWindow.Top = 20; break;
                    case "center": uiWindow.Top = (sHeight - uiWindow.Height) / 2; break;
                    case "bottom": uiWindow.Top = sHeight - uiWindow.Height; break;
                    case "manual":
                        uiWindow.Top = location.HasValue ? location.Value.X : sHeight - uiWindow.Height; break;
                }//switch

                switch (root.GetAttribute("hpos"))
                {
                    case "left": uiWindow.Left = 20; break;
                    case "center": uiWindow.Left = (sWidth - uiWindow.Width) / 2; break;
                    case "right": uiWindow.Left = sWidth - uiWindow.Width; break;
                    case "manual":
                        uiWindow.Left = location.HasValue ? location.Value.Y : sWidth - uiWindow.Width; break;
                }//switch

                //..........................................
                string sMargin = root.GetAttribute("margin");
                if (!String.IsNullOrEmpty(sMargin))
                {
                    String[] aryMargin = sMargin.Split(',');
                    if (aryMargin.Length == 4)
                    {
                        uiGrid.Margin = new Thickness(
                            int.Parse(aryMargin[0])
                            , int.Parse(aryMargin[1])
                            , int.Parse(aryMargin[2])
                            , int.Parse(aryMargin[3])
                       );
                    }//if
                }//if

                //..........................................
                //Reset UI Grid
                uiGrid.Children.Clear();
                uiGrid.RowDefinitions.Clear();
                uiGrid.ColumnDefinitions.Clear();
                tButtonList.Clear();

                //Create all the rows on the main UI Grid
                for (int i = 0; i < root.ChildNodes.Count; i++) uiGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

                //..........................................
                //Reset UI Grid
                int iRow = 0, iKey = 0;
                Grid rGrid;

                foreach (XmlNode row in root.ChildNodes)
                {
                    //Create Key Row Container
                    rGrid = CreateGrid();
                    Grid.SetRow(rGrid, iRow);
                    Grid.SetColumn(rGrid, 0);
                    uiGrid.Children.Add(rGrid);

                    //Create Keys
                    iKey = 0;
                    double gLen = 0;
                    string sgLen = "";
                    string toggle = "";
                    foreach (XmlElement key in row.ChildNodes)
                    {
                        sgLen = key.GetAttribute("weight");
                        toggle = key.GetAttribute("toggle");
                        gLen = (String.IsNullOrEmpty(sgLen)) ? 1 : Double.Parse(sgLen);

                        rGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(gLen, GridUnitType.Star) });
                        if (toggle == "true")
                        {
                            rGrid.Children.Add(CreateToggleButton(key, iKey));
                        }
                        else
                        {
                            rGrid.Children.Add(CreateButton(key, iKey));
                        }
                        iKey++;
                    }//for
                    iRow++;
                }//for

                return true;
            }
            catch (Exception e)
            {
                vLogger.Exception("vLayout.Load", e, fName);
            }//try

            return false;
        }//func

        private static Grid CreateGrid()
        {
            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            return grid;
        }//func

        private static vButton CreateButton(XmlElement elm, int col)
        {
            string code, shCode, shText
                , title = elm.GetAttribute("text")
                , fsize = elm.GetAttribute("fsize");

            vButton btn = new vButton() { };
            Grid.SetRow(btn, 0);
            Grid.SetColumn(btn, col);
            btn.FontFamily = mIconFont;
            btn.FontSize = 20.0;
            btn.Title = title;
            if (!String.IsNullOrEmpty(fsize)) btn.FontSize = double.Parse(fsize);

            switch (elm.Name)
            {
                //.........................................
                case "key":
                    code = elm.GetAttribute("code");
                    shCode = elm.GetAttribute("shcode");
                    shText = elm.GetAttribute("shtext");

                    if (!String.IsNullOrEmpty(code)) btn.KBCommand.KBKeys = code.Split(' ');
                    if (!String.IsNullOrEmpty(shCode)) btn.KBCommand.KBShKeys = shCode.Split(' ');
                    if (!String.IsNullOrEmpty(shText)) btn.ShiftText = shText;

                    btn.KBCommand.SendString = elm.GetAttribute("string");
                    btn.KBCommand.shSendString = elm.GetAttribute("shstring");
                    btn.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(vLayout.vBtnTouch_Down), true);
                    btn.AddHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(vLayout.vBtnTouch_Up), true);
                    btn.PreviewTouchDown += new EventHandler<TouchEventArgs>(vLayout.vBtnTouch_Down);
                    btn.PreviewTouchUp += new EventHandler<TouchEventArgs>(vLayout.vBtnTouch_Up);

                    break;
                //.........................................
                case "menu":
                    ContextMenu menu = new ContextMenu();
                    KeyboardCommand kbCmd;
                    MenuItem mItem;

                    foreach (XmlElement itm in elm.ChildNodes)
                    {
                        kbCmd = new KeyboardCommand();
                        title = itm.GetAttribute("text");
                        code = itm.GetAttribute("code");

                        if (!String.IsNullOrEmpty(code)) kbCmd.KBKeys = code.Split(' ');
                        kbCmd.SendString = itm.GetAttribute("string");

                        mItem = new MenuItem() { Header = title, Tag = kbCmd };
                        mItem.Click += OnMenuClick;
                        menu.Items.Add(mItem);
                    }//for

                    btn.ContextMenu = menu;
                    btn.Click += OnMenuButtonPress;
                    break;
            }//switch

            return btn;
        }//func
        private static tButton CreateToggleButton(XmlElement elm, int col)
        {
            string text = elm.GetAttribute("text");
            string fsize = elm.GetAttribute("fsize");
            string keydown = elm.GetAttribute("keydown");
            tButton tButton = new tButton();
            tButton.FontSize = 20.0;
            Grid.SetRow(tButton, 0);
            Grid.SetColumn(tButton, col);
            tButton.FontFamily = vLayout.mIconFont;
            tButton.Title = text;
            bool flag = !string.IsNullOrEmpty(fsize);
            if (flag)
            {
                tButton.FontSize = double.Parse(fsize);
            }
            string code = elm.GetAttribute("code");
            if (!String.IsNullOrEmpty(code)) tButton.KBCommand.KBKeys = code.Split(' ');
            if (keydown == "true")
            {
                tButton.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(vLayout.tBtnTouch_Down);
                tButton.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(vLayout.tBtnTouch_Up);
                tButton.PreviewTouchDown += new EventHandler<TouchEventArgs>(vLayout.tBtnTouch_Down);
                tButton.PreviewTouchUp += new EventHandler<TouchEventArgs>(vLayout.tBtnTouch_Up);
            }
            else
            {
                tButton.Click += new RoutedEventHandler(vLayout.tBtnTouch);
                tButton.TouchUp += new EventHandler<TouchEventArgs>(vLayout.tBtnTouch);
            }
            vLayout.tButtonList.Add(tButton);
            return tButton;
        }
        public static void tBtnTouch(object sender, EventArgs e)
        {
            vKeyboard.ProcessCommand((sender as tButton).KBCommand, null);
        }

        public static void tBtnTouch_Down(object sender, EventArgs e)
        {
            vKeyboard.ProcessCommand((sender as tButton).KBCommand, new bool?(false));
        }

        public static void tBtnTouch_Up(object sender, EventArgs e)
        {
            vKeyboard.ProcessCommand((sender as tButton).KBCommand, new bool?(true));
        }

        public static void vBtnTouch_Down(object sender, EventArgs e)
        {
            KeyLoopHandler.BeginKeypress((sender as vButton).KBCommand);
        }

        public static void vBtnTouch_Up(object sender, EventArgs e)
        {
            KeyLoopHandler.EndKeypress();
        }

        public static void OnMenuClick(object sender, RoutedEventArgs e)
        {
            vKeyboard.ProcessCommand((KeyboardCommand)(sender as MenuItem).Tag, null);
        }

        public static void OnMenuButtonPress(object sender, RoutedEventArgs e)
        {
            (sender as vButton).ContextMenu.IsOpen = true;
        }
        private static string RootPath(string relativePath)
        {
            string rtn = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            if (!rtn.EndsWith("\\")) rtn += "\\";

            return rtn + relativePath;
        }//func

        public static string[] GetLayoutList()
        {
            string[] rtn = System.IO.Directory.GetFiles(RootPath("Layouts"), "*.xml");

            for (int i = 0; i < rtn.Length; i++) rtn[i] = System.IO.Path.GetFileNameWithoutExtension(rtn[i]);

            return rtn;
        }//func
    }//cls
}//ns
