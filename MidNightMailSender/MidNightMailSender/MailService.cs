using MidNightMailSeder.Mail;
using MidNightMailSender.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace MidNightMailSender
{
    public partial class MailService : ServiceBase
    {
        private Timer timer1;
        private int getCallType;
        private string timeString;

        public MailService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            timer1.AutoReset = true;
            timer1.Enabled = true;
            ServiceLog.WriteErrorLog("Daily Reporting service started");
        }

        protected override void OnStop()
        {
            timer1.AutoReset = false;
            timer1.Enabled = false;
            ServiceLog.WriteErrorLog("Daily Reporting service stopped");
        }

        public void Scheduler()
        {
            InitializeComponent();
            int strTime = Convert.ToInt32(ConfigurationManager.AppSettings["callDuration"]);
            getCallType = Convert.ToInt32(ConfigurationManager.AppSettings["CallType"]);
            if (getCallType == 1)
            {
                timer1 = new System.Timers.Timer();
                double inter = (double)GetNextInterval();
                timer1.Interval = inter;
                timer1.Elapsed += new ElapsedEventHandler(ServiceTimer_Tick);
            }
            else
            {
                timer1 = new System.Timers.Timer();
                timer1.Interval = strTime * 1000;
                timer1.Elapsed += new ElapsedEventHandler(ServiceTimer_Tick);
            }
        }
        private double GetNextInterval()
        {
            timeString = ConfigurationManager.AppSettings["StartTime"];
            DateTime t = DateTime.Parse(timeString);
            TimeSpan ts = new TimeSpan();
            int x;
            ts = t - System.DateTime.Now;
            if (ts.TotalMilliseconds < 0)
            {
                ts = t.AddDays(1) - System.DateTime.Now;//Here you can increase the timer interval based on your requirments.   
            }
            return ts.TotalMilliseconds;
        }
        private void SetTimer()
        {
            try
            {
                double inter = (double)GetNextInterval();
                timer1.Interval = inter;
                timer1.Start();
            }
            catch (Exception ex)
            {
            }
        }
        private void ServiceTimer_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            EmployeeEntities entities = new EmployeeEntities();
            var employees = entities.Employees.Where(emp => emp.IsRead.Equals(false) && (emp.EmailSentDate == null || emp.EmailSentDate > DateTime.Now.AddDays(-3))).ToList();
            if (employees != null && employees.Count > 0)
            {
                string Msg = "Hi ! This is DailyMailSchedulerService mail. PLease click the <a href='http://localhost:56894/Default.aspx?id={0}' target='_blank'>link</a>";
                employees.ForEach(emp =>
                {
                    MailUtility.SendEmail(emp.Email, "Daily Report of DailyMailSchedulerService on " + DateTime.Now.ToString("dd-MMM-yyyy"), string.Format(Msg, emp.Id));
                });
                employees.Select(emp => { emp.EmailSentDate = DateTime.Now.Date; return emp; });
                entities.Entry(employees).State = System.Data.Entity.EntityState.Modified;
                entities.SaveChanges();
            }
            if (getCallType == 1)
            {
                timer1.Stop();
                System.Threading.Thread.Sleep(1000000);
                SetTimer();
            }
        }
    }
}
