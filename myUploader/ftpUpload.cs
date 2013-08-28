using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Windows.Forms;
using System.IO;

namespace myUploader
{
    class ftpUpload
    {

        /// <summary>
        /// Variablen die von Außen gesetzt werden können
        /// und für die FTP Verbindung wichtig sind.
        /// </summary>
        public String FtpAdress { get; set; }
        public int FtpPort { get; set; }
        public String Username { get; set; }
        public String Password { get; set; }
        public StreamReader myStream { get; set; }
                    
        StreamReader ftp_response_sr = null;
        FtpWebResponse ftp_response = null;
        FtpWebRequest ftp_request = null;


        public void initializeFTP()
        {
            ftp_request = (FtpWebRequest)WebRequest.Create(FtpAdress);
            ftp_request.Credentials = new NetworkCredential(Username, Password);
            ftp_request.Method = WebRequestMethods.Ftp.ListDirectory;
            ftp_request.UseBinary = true;
        }

        public void initializeFTP(string ftpfolder)
        {
            string Adress = FtpAdress + ftpfolder;
            ftp_request = (FtpWebRequest)WebRequest.Create(Adress);
            ftp_request.Credentials = new NetworkCredential(Username, Password);
            ftp_request.Method = WebRequestMethods.Ftp.ListDirectory;
            ftp_request.UseBinary = true;
        }


        public void ftpListDirectory()
        {
            try
            {
                ftp_response = (FtpWebResponse)ftp_request.GetResponse();
                Stream ftp_response_stream = ftp_response.GetResponseStream();
                ftp_response_sr = new StreamReader(ftp_response_stream);
            }
            catch (WebException e)
            {
                MessageBox.Show("Error: " + e.Message + "\n" + e.Status + "\n" + "Überprüfen Sie Username und Passwort!");
            }

            myStream = ftp_response_sr;

        }
                  

    }
}
