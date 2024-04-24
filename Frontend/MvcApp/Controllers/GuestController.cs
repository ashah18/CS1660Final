
using Microsoft.AspNetCore.Mvc;
namespace MvcApp.Controllers;

using MvcApp.DataModel;
using MvcApp.Models;
using MvcApp.Services;


public class GuestController : Controller
{
    private readonly IConfigurationRoot configurationBuilder;
    private readonly ApplicationDBContext _db;
    private readonly GuestService service;
    public GuestController(ApplicationDBContext db)
    {
        _db = db;
        service = new GuestService(db);
        configurationBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
    }
    [HttpGet]
    public IActionResult Index()
    {
        List<vwSavedModel> models = service.getSavedModels();
        return View(models);
    }

    public ActionResult SetCurrentBucketName(string bucket_name)
    {
        service.set_current_model_bucket(bucket_name);
        return View("~/Views/Guest/model_page.cshtml");
    }
    [HttpPost("Predict_Output")]
    public async Task<ActionResult> Predict_Output(IFormFile file)
    {
        try
        {
            var modelData = new PredictModelData();
            if (file.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    modelData.input_file = ms.ToArray();
                }
            }
            var imageExtension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            modelData.bucket_name = service.GetCurrentModelBucketName();
            modelData.project_id = configurationBuilder.GetValue<String>("CloudDetails:project_id");
            var input_url = configurationBuilder.GetValue<String>("CloudURLs:input_url");
            var predict_url = configurationBuilder.GetValue<String>("CloudURLs:predict_url");
            var final_prediction = await service.SendInputToCloud(modelData, input_url, predict_url, imageExtension.Substring(1, imageExtension.Length - 1));
            ViewBag.Message = final_prediction.ToString();
            // var result = service.AddAdmin(admin);
            return View("~/Views/Guest/model_page.cshtml");
        }
        catch
        {
            ViewBag.Message = "Error while processing request";
            return View("~/Views/Guest/model_page.cshtml");
        }
    }

}
