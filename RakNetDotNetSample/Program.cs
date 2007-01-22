using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace RakNetDotNetSample
{
    internal static class Program
    {
        #region Fields
        /// <summary>
        /// The one (and only) form in this sample app
        /// </summary>
        private static frmMain mainForm;

        /// <summary>
        /// Our sample server
        /// </summary>
        private static Server dotNETServer;

        /// <summary>
        /// Our sample client
        /// </summary>
        private static Client dotNETClient;

        public static Apple apple = new Apple();
        #endregion

        #region Properties
        /// <summary>
        /// Gets the main form
        /// </summary>
        public static frmMain MainForm
        {
            get { return mainForm; }
        }

        /// <summary>
        /// Gets the Server
        /// </summary>
        public static Server DotNETServer
        {
            get { return dotNETServer; }
        }

        /// <summary>
        /// Gets the Client
        /// </summary>
        public static Client DotNETClient
        {
            get { return dotNETClient; }
        }
        #endregion

        #region Main
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            mainForm = new frmMain();
            dotNETServer = new Server();
            dotNETClient = new Client();
            Application.Run(mainForm);
        }
        #endregion
    }
}