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
using System.Xml;
using System.Configuration;

namespace myUploader
{

    public partial class Form1 : Form
    {

        public static NotifyIcon notico;
        private ContextMenu contextMenu = new ContextMenu();
        private string FtpFolder;
        private static string FtpFolderFull;
        private FileVersionInfo fvi = null;


        public Form1()
        {
            InitializeComponent();

            readConfigFile();
            
            // NotifyIcon erzeugen
            notico = new NotifyIcon();
            notico.Icon = new Icon("c:\\temp\\dpk.ico"); // Eigenes Icon einsetzen
            notico.DoubleClick += new EventHandler(NotifyIconDoubleClick);

            if (textBox1.Text == "" || textBox2.Text == "")
            {
                return;
            }

            execute();

            Form1.ActiveForm.WindowState = FormWindowState.Minimized;
        }

        /// <summary>
        /// Menuitems dem Contextmenü im Notifyicon hinzufügen
        /// </summary>
        /// <param name="menuName"></param>
        /// <param name="option"></param>
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

        private void execute()
        {
            string unifaceVersion = null;
            string line = null;
            string pathToApplication = null;
            FileInfo fi = null;

            if(textBox1.Text == "" || textBox2.Text == "")
            {
                MessageBox.Show("Die Daten sind nicht komplett!");
                return;
            }

            if(timer1.Interval.Equals(100))
            {
                timer1.Interval = Convert.ToInt32(textBox2.Text) * 60000; //Sekunden
                timer1.Start();
            }


            pathToApplication = textBox1.Text;

            try
            {
                fi = new FileInfo(pathToApplication);
            }
            catch 
            {
                MessageBox.Show("Application ' {0} ' not found!", pathToApplication);
                notico.Dispose();
                Application.Exit();
            }

            try
            {
                fvi = FileVersionInfo.GetVersionInfo(pathToApplication);
            }
            catch
            {
                MessageBox.Show("Application ' {0} ' not found!" , pathToApplication);
                notico.Dispose();
                Application.Exit();
            }

            unifaceVersion = fvi.ProductPrivatePart.ToString();

            // FTP Objekt
            ftpUpload fu = new ftpUpload();

            // FTP Einstellungen
            fu.FtpAdress = "ftp://ftp.compuware.com/";
            fu.FtpPort = 21;
            fu.Username = "anonymous";
            fu.Password = "anonymous@anon.de";
            FtpFolder = "pub/uniface/patches/9602/w32/";

            fu.initializeFTP(FtpFolder);
            fu.ftpListDirectory();

            // Den Stream mit den Dateinamen lesen und auseinander nehmen
            StreamReader ftpDirList = fu.myStream;

            while((line = ftpDirList.ReadLine()) != null)
            {
                if (line.IndexOf(".exe") > 0)
                {
                    // Nur nach EXE Dateien im Filestream schauen und darstellen
                    string exeOnly = line.Substring(line.IndexOf(".exe")-3,3);
                    int x = Convert.ToInt32(exeOnly);
                    int y = Convert.ToInt32(unifaceVersion);
                    if (Convert.ToInt32(exeOnly) >= Convert.ToInt32(unifaceVersion))
                    {
                        // Dem Menü den Update-Punkt hinzufügen
                        addMenu(line+" ("+fi.Name+ " " +fvi.FileVersion+")","line");
                    }
                }
            }

            FtpFolderFull = fu.FtpAdress + FtpFolder;
            
            addMenu("-", "sep");        // Seperator-Bar (Trennlinie)
            addMenu("Beenden", "exit"); // Das ist das Beenden Button im Contextmenu

            notico.Text = "Uniface Update Check";   // Eigenen Text einsetzen
            notico.Visible = true;
            notico.ContextMenu = contextMenu;

            if (contextMenu.MenuItems.Count > 2) // Seperator + Beenden
            {
                notico.ShowBalloonTip(5, "Updates gefunden!", fi.Name+ " " +fvi.FileVersion+" ("+DateTime.Now.ToLocalTime().ToString()+")", ToolTipIcon.Info);
            }           
            

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
            int start = x.IndexOf("Text: ") + 6;
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
        
        /// <summary>
        /// Timer Event, so the Updates are checked for every X-Ticks
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            contextMenu.MenuItems.Clear();
            execute();
        }

        /// <summary>
        /// Config File einlesen und die Maske füllen
        /// Wenn die Daten korrekt gelesen wurden, dann kann das Programm auch danach starten.
        /// </summary>
        private void readConfigFile()
        {

            if (File.Exists("ufupdateconfig.xml"))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load("ufupdateconfig.xml");


                 XmlElement root = doc.DocumentElement;
                foreach (XmlNode @parameter in root.ChildNodes)
                {
                    textBox1.Text = @parameter.Attributes["param1"].InnerText;
                    textBox2.Text = @parameter.Attributes["param2"].InnerText;
                }

            }
            else
            {
                MessageBox.Show("Es wurden noch keine Konfiguration vorgenommen!");
            }
        }

        /// <summary>
        /// Applikation suchen und eintragen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "Uniface Applikation suchen...";
            openFileDialog1.Multiselect = false;
            openFileDialog1.InitialDirectory = "U:\\uniface\\bin\\";
            openFileDialog1.FileName = "Uniface.exe";
            openFileDialog1.ShowDialog();
            textBox1.Text = openFileDialog1.FileName;
        }

        /// <summary>
        /// Sichern der Einstellungen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            if(textBox1.Text == "")
            {
                MessageBox.Show("Pfad zur Uniface Executable wurde nicht eingegeben, abbruch!");
                return;
            }
                
            if(textBox2.Text == "")
            {
                MessageBox.Show("Intervallänge wurde nicht einegeben, abbruch!");
                return;
            }

            if (File.Exists("ufupdateconfig.xml"))
            {
                try
                {
                    File.Delete("ufupdateconfig.xml");
                }
                catch
                {
                    // Not yet.
                }

                
            }

            XmlDocument doc = new XmlDocument();
            XmlNode myRoot;
            XmlNode myNode;
            XmlAttribute myAttr;

            myRoot = doc.CreateElement("UFUpdateConfig");
            doc.AppendChild(myRoot);

            myNode = doc.CreateElement("parameter");

            myAttr = doc.CreateAttribute("param1");
            myAttr.InnerText = textBox1.Text;
            myNode.Attributes.Append(myAttr);
            
            myAttr = doc.CreateAttribute("param2");
            myAttr.InnerText = textBox2.Text;
            myNode.Attributes.Append(myAttr);

            myRoot.AppendChild(myNode);

            doc.Save("ufupdateconfig.xml");
            Form1.ActiveForm.WindowState = FormWindowState.Minimized;
            
            timer1.Stop();
            timer1.Interval = Convert.ToInt32(textBox2.Text) * 60000;
            timer1.Start();
            
            execute();

            

        }

    }
}
