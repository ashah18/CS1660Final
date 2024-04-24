using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using MvcApp.DataModel;
using MvcApp.Models;

namespace MvcApp.Services;

public class AdminService : CommonService
{
    private readonly ApplicationDBContext db;
    public static AdminDetails UserDetails;
    public AdminService(ApplicationDBContext db)
    {
        this.db = db;
    }
    public ContentResult AddAdmin(AdminCreds admin)
    {
        try
        {
            AdminCreds getadmindetails = db.adminCreds.FirstOrDefault(x => x.username == admin.username && x.password == admin.password);
            if (getadmindetails == null)
            {
                db.adminCreds.Add(admin);
                db.SaveChanges();
                return new ContentResult() { StatusCode = StatusCodes.Status200OK };
            }
            else
            {
                return new ContentResult() { StatusCode = StatusCodes.Status302Found };
            }
        }
        catch (Exception e)
        {
            return new ContentResult() { Content = "", StatusCode = StatusCodes.Status502BadGateway };
        }
    }
    public AdminCreds ValidateAdmin(AdminCreds creds)
    {
        try
        {
            AdminCreds adminCreds = db.adminCreds.ToList().First(x => x.username == creds.username && x.password == creds.password);
            AdminDetails userdetails = new AdminDetails();

            userdetails.Id = adminCreds.Id;
            userdetails.name = adminCreds.name;
            userdetails.username = adminCreds.username;
            UserDetails = userdetails;
            return adminCreds;
        }
        catch (Exception e)
        {
            return null;
        }

    }
    public async Task<bool> UploadModel(UploadModel uploadmodel)
    {
        try
        {
            var savemodel = new SaveModel();
            var url = "https://cloud-api-x6r7sq65pq-uc.a.run.app/initialize_model_and_upload_model";
            using (var httpClient = new HttpClient())
            {
                var content = new MultipartFormDataContent();
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                content.Add(CreateFileContent(uploadmodel.file), "file", uploadmodel.file.FileName);
                content.Add(new StringContent(uploadmodel.data), "data");
                httpClient.Timeout = TimeSpan.FromMinutes(5);
                var response = await httpClient.PostAsync(url, content);
                var result = response.Content.ReadFromJsonAsync<SaveModel>().Result;

                // Model_ID, bucket_name
                // response.EnsureSuccessStatusCode();
                // var result = await response.Content.ReadAsStringAsync();
                // string result = response.Content.ReadAsStringAsync().Result;
            }
            db.SaveModel.Add(savemodel);
            db.SaveChanges();
            return true;
        }
        catch (Exception e)
        {
            return false;
        }

    }
    public AdminDetails GetCurrentUser()
    {
        return UserDetails;
    }
    public List<AdminDetails> GetAdmins()
    {
        List<AdminDetails> adminDetails = db.vwAdminDetails.ToList();
        return adminDetails;
    }
    public List<vwSavedModel> getSavedModels()
    {
        List<vwSavedModel> models = new List<vwSavedModel>();
        try
        {
            models = db.SavedModels.ToList();
            return models;
        }
        catch (Exception e)
        {
            return models;
        }
    }
}

