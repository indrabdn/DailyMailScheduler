using MidNightMailSeder.Mail;
using MidNightMailSender.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class _Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Page.IsPostBack)
        {
            EmployeeEntities entities = new EmployeeEntities();
            int id = Convert.ToInt32(Request.QueryString["id"]);
            Employee employee = entities.Employees.Where(emp => emp.Id.Equals(id)).FirstOrDefault();
            if (employee != null)
            {
                employee.IsRead = true;
                entities.Entry(employee).State = EntityState.Modified;
                entities.SaveChanges();
            }
            string Msg = "Thank You.";
            MailUtility.SendEmail(employee.Email, "Acceptence of Daily Report of DailyMailSchedulerService on " + DateTime.Now.ToString("dd-MMM-yyyy"), Msg);
        }

    }
}