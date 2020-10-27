using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using ExtractQueryCore;
using NLog;


namespace CamstarExtractService
{
    public partial class CamstarExtractService : ServiceBase
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        Timer timer = new Timer(); // name space(using System.Timers;)
        private CamstarExtractor csx;
        public CamstarExtractService()
        {
            InitializeComponent();
            this.ServiceName = "CamstarExtractService";
        }
        protected override void OnStart(string[] args)
        {

            timer.Interval = 500; //number in milisecinds  
            logger.Info(String.Format("Service {0} has been started at {1}, Timer set to {2}", this.ServiceName, DateTime.Now, timer.Interval));               // Manual Log
            try
            {
                logger.Info(String.Format("Running CamstarExtractor"));               // Manual Log
                csx = new CamstarExtractor();
                logger.Info(String.Format("CamstarExtractor object created successfully."));               // Manual Log
            }
            catch (Exception e)
            {
                logger.Fatal(String.Format("Service {0} has thrown an exception! {1} at {2}. Shuttting down.", this.ServiceName, e.Message, DateTime.Now));               // Manual Log
                ServiceController svc = new ServiceController("CamstarExtractService");
                svc.Stop();
            }
            logger.Info("Service is started at " + DateTime.Now);
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Enabled = true;
        }
        protected override void OnStop()
        {
            logger.Info(String.Format("Service {0} is stopping at {1}", this.ServiceName, DateTime.Now));               // Manual Log
            csx = null;
            logger.Info("Service is stopped at " + DateTime.Now);
        }
        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            logger.Info("Service {0} is recalled at {1}.", this.ServiceName, DateTime.Now); // Manual Log
            timer.Stop();
            timer.Enabled = false;
            TimeSpan ts = csx.RunUntilDone();
            timer.Interval = ts.TotalSeconds * 1000;
            timer.Enabled = true;
            timer.Start();

        }
#if false
        public void WriteToFile(string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.   
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }
#endif
    }
}
