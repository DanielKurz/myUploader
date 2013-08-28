using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;


namespace myUploader
{

    public partial class Form1 : Form
    {

        public static NotifyIcon notico;
        private ContextMenu contextMenu = new ContextMenu();
        private string FtpFolder;
        private static string FtpFolderFull;

        public Form1()
        {
            InitializeComponent();
            button1_Click();
        }


        public void addMenu(string menuName, string option)
        {
            if (menuName != "")
            {
                MenuItem menuitem = new MenuItem();
                int index = menuitem.Index;
                menuitem.Index = index + 1;
                menuitem.Text = menuName;
                if (option == "line")
                {
                    menuitem.Click += new System.EventHandler(Action1Click);
                }
                else
                {
                    if (option == "exit")
                    {
                        menuitem.Click += new System.EventHandler(ExitClick);
                    }
                }
                
                contextMenu.MenuItems.Add(menuitem);
            }
        }

        private void button1_Click()//(object sender, EventArgs e)
        {
            ftpUpload fu = new ftpUpload();

            fu.FtpAdress = "ftp://ftp.compuware.com/";
            fu.FtpPort = 21;
            fu.Username = "anonymous";
            fu.Password = "anonymous@anon.de";
            FtpFolder = "pub/uniface/patches/9602/w32/";
            
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo("U:\\uniface\\bin\\uniface.exe");

            string Uniface96Version = fvi.ProductPrivatePart.ToString();

            fu.initializeFTP(FtpFolder);
            fu.ftpListDirectory();

            StreamReader FtpDirList = fu.myStream;

            string line;
            while((line = FtpDirList.ReadLine()) != null)
            {
                if (line.IndexOf(".exe") > 0)
                {
                    string exeonly = line.Substring(line.IndexOf(".exe")-3,3);
                    int x = Convert.ToInt32(exeonly);
                    int y = Convert.ToInt32(Uniface96Version);
                    if(Convert.ToInt32(exeonly) >= Convert.ToInt32(Uniface96Version))
                    {
                        richTextBox1.Text += fu.FtpAdress+FtpFolder+line+"\n";
                        addMenu("Update Uniface 9.6.02: "+line,"line");
                    }
                }
            }

            FtpFolderFull = fu.FtpAdress + FtpFolder;
            
            addMenu("-", "sep");
            addMenu("Beenden", "exit");

            // NotifyIcon selbst erzeugen
            notico = new NotifyIcon();
            notico.Icon = new Icon("c:\\temp\\dpk.ico"); // Eigenes Icon einsetzen
            notico.Text = "Uniface Update Check";   // Eigenen Text einsetzen
            notico.Visible = true;
            notico.ContextMenu = contextMenu;
            notico.DoubleClick += new EventHandler(NotifyIconDoubleClick);

        }

        //==========================================================================
        private static void ExitClick(Object sender, EventArgs e)
        {
            notico.Dispose();
            Application.Exit();
        }

        //==========================================================================
        private static void Action1Click(Object sender, EventArgs e)
        {
            string x = Convert.ToString(sender);
            int start = x.IndexOf("Update Uniface 9.6.02: ") + 23;
            int stop = x.IndexOf(".exe")+4;
            int diff = stop-start;
            string y = x.Substring(start, diff);
            y = FtpFolderFull + y;
            Process.Start("iexplore.exe", y);
        }

        //==========================================================================
        private static void NotifyIconDoubleClick(Object sender, EventArgs e)
        {
        }
    }
}
