using System;
using Microsoft.AspNetCore.Mvc;
using FastReport.Web;
using System.IO;

namespace SPAWebReport.Controllers
{
    [Route("api/[controller]")]
    public class SampleDataController : Controller
    {

        [HttpGet("[action]")]
        public IActionResult ShowReport()
        {
            WebReport WebReport = new WebReport();
            WebReport.Width = "1000";
            WebReport.Height = "1000";
            WebReport.Report.Load("App_Data/Master-Detail.frx"); //��������� ����� � ������ WebReport
            System.Data.DataSet dataSet = new System.Data.DataSet(); //������� �������� ������
            dataSet.ReadXml("App_Data/nwind.xml");  //��������� ���� ������ xml
            WebReport.Report.RegisterData(dataSet, "NorthWind"); //������������ �������� ������ � ������
            ViewBag.WebReport = WebReport; //�������� ����� �� View
            return View();
        }

    }
}
