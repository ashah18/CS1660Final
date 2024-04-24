
using Microsoft.AspNetCore.Mvc;
using MvcApp.Models;
using MvcApp.DataModel;
using MvcApp.Services;
using Newtonsoft.Json;


namespace MvcApp.Controllers;

public class AdminController : Controller
{
    private readonly ApplicationDBContext _db;
    private readonly AdminService service;
    private readonly IConfigurationRoot configurationBuilder;

    public AdminController(ApplicationDBContext db)
    {
        _db = db;
        service = new AdminService(db);
        configurationBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
    }
    [HttpGet]
    public vwModelsandAdmins Index()
    {
        var modelsandAdmins = new vwModelsandAdmins();
        modelsandAdmins.adminCreds = service.GetAdmins();
        modelsandAdmins.LoginUserDetails = service.GetCurrentUser();
        modelsandAdmins.SavedModels = service.getSavedModels();
        return modelsandAdmins;
    }

    [HttpPost("AddAdmin")]

    public ActionResult AddAdmin(AdminCreds admin)
    {
        try
        {
            var result = service.AddAdmin(admin);
            return View("~/Views/Admin/Index.cshtml", Index());
        }
        catch
        {
            return View("~/Views/Admin/Index.cshtml", Index());
        }
    }

    [HttpGet("VerifyUser")]
    public ActionResult VerifyUser(AdminCreds creds)
    {
        try
        {
            var result = service.ValidateAdmin(creds);
            if (result == null)
            {
                var values = new ContentResult() { Content = "Unauthorized", StatusCode = StatusCodes.Status401Unauthorized, ContentType = "application/json" };
                return View("~/Views/Home/LoginScreen.cshtml", new { Values = values });
            }
            else
            {
                var list_values = Index();
                // var values = new ContentResult() { Content = result.name, StatusCode = StatusCodes.Status200OK };
                return View("~/Views/Admin/Index.cshtml", list_values);
            }
        }
        catch (Exception e)
        {
            var values = new ContentResult() { Content = null, StatusCode = StatusCodes.Status400BadRequest };
            return View("~/Views/Home/LoginScreen.cshtml", new { Values = values });
        }
    }

    [HttpPost("UploadModel")]
    public ActionResult<vwModelsandAdmins> UploadModel(IFormFile file)
    {
        try
        {

            UploadModel model = new UploadModel();
            model.file = file;
            var cloud_data = new CloudCredsModel();
            cloud_data.project_id = configurationBuilder.GetValue<string>("CloudDetails:project_id");
            cloud_data.location = configurationBuilder.GetValue<string>("CloudDetails:location");
            // cloud_data.bucket_name = configurationBuilder.GetValue<String>("CloudDetails:bucket_name");
            // model.data = '{"project_id":"cloudcomputing-411615", "location":"us-central1"}';
            model.data = JsonConvert.SerializeObject(cloud_data);
            var result = service.UploadModel(model);
            ViewBag.Message = "Upload Successful";
            var vwModel = new vwModelsandAdmins();
            vwModel = Index();
            return View("~/Views/Admin/Index.cshtml", vwModel);
        }
        catch (Exception e)
        {
            ViewBag.Message = e;
            var vwModel = new vwModelsandAdmins();
            vwModel = Index();
            return View("~/Views/Admin/Index.cshtml", vwModel);
        }
    }

}
