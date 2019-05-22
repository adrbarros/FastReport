using System;
using Microsoft.AspNetCore.Mvc;
using FastReport.Web;
using System.IO;

namespace SPAOnlineDesigner.Controllers
{
    [Route("api/[controller]")]
    public class SampleDataController : Controller
    {

        [HttpGet("[action]")]
        public IActionResult Design()
        {
            WebReport WebReport = new WebReport();
            WebReport.Width = "1000";
            WebReport.Height = "1000";
            WebReport.Report.Load("App_Data/Master-Detail.frx"); //��������� ����� � ������ WebReport
            System.Data.DataSet dataSet = new System.Data.DataSet(); //������� �������� ������
            dataSet.ReadXml("App_Data/nwind.xml");  //��������� ���� ������ xml
            WebReport.Report.RegisterData(dataSet, "NorthWind"); //������������ �������� ������ � ������
            WebReport.Mode = WebReportMode.Designer; //������������� ����� ������� ��� ����� - ����������� ���������
            WebReport.DesignerLocale = "en";
            WebReport.DesignerPath = @"WebReportDesigner/index.html"; //������ URL ������ ���������
            WebReport.DesignerSaveCallBack = @"SampleData/SaveDesignedReport"; //������ URL ������������� ��� ������ ���������� ������
            ViewBag.WebReport = WebReport; //�������� ����� �� View
            return View();
        }


        [HttpPost]
        // call-back for save the designed report 
        public ActionResult SaveDesignedReport(string reportID, string reportUUID)
        {
            ViewBag.Message = String.Format("Confirmed {0} {1}", reportID, reportUUID); //������ ��������� ��� �������������

            Stream reportForSave = Request.Body; //���������� � ����� ��������� Post �������
            string pathToSave = @"App_Data/TestReport.frx"; //�������� ���� ��� ���������� �����
            using (FileStream file = new FileStream(pathToSave, FileMode.Create)) //������� �������� �����
            {
                reportForSave.CopyTo(file); //��������� ��������� ������� � ����
            }
            return View();
        }


    }
}
