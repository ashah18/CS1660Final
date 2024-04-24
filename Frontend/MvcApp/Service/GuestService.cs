
using System.Net.Http.Headers;
using MvcApp.DataModel;
using MvcApp.Models;
using System.Threading;
using Newtonsoft.Json;


namespace MvcApp.Services;
public class GuestService : CommonService
{
    private static string current_model_bucket_name;
    private readonly ApplicationDBContext db;

    public GuestService(ApplicationDBContext db)
    {
        this.db = db;
    }

    public void set_current_model_bucket(string bucket)
    {
        current_model_bucket_name = bucket;
    }
    public string GetCurrentModelBucketName()
    {
        return current_model_bucket_name;
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

    public async Task<string> SendInputToCloud(PredictModelData predictModelData, string input_url, string predict_url, string imageExtension)
    {
        try
        {
            using (var httpClient = new HttpClient())
            {
                var content = new MultipartFormDataContent();
                // content.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                var imageContent = new ByteArrayContent(predictModelData.input_file);
                imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/" + imageExtension);
                string imageBinaryString = Convert.ToBase64String(predictModelData.input_file);
                content.Add(imageContent, "file", "file." + imageExtension);
                content.Add(new StringContent(predictModelData.bucket_name), "bucket_name");
                content.Add(new StringContent(predictModelData.project_id), "project_id");
                // httpClient.Timeout = TimeSpan.FromMinutes(5);
                var response = await httpClient.PostAsync(input_url, content);
                var result = response.Content.ReadFromJsonAsync<SaveModel>().Result;
                var final_prediction = GetPrediction(predict_url);
                return final_prediction.Result.ToString();
                // Model_ID, bucket_name
                // response.EnsureSuccessStatusCode();
                // var result = await response.Content.ReadAsStringAsync();
                // string result = response.Content.ReadAsStringAsync().Result;
            }
        }
        catch (Exception e)
        {
            return null;
        }
    }

    public async Task<string> GetPrediction(string predict_url)
    {
        try
        {
            Thread.Sleep(10000);
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(predict_url);
                var result = response.Content.ReadAsStringAsync().Result;
                var result_1 = JsonConvert.DeserializeObject<PredictResponse>(result);
                if(result_1.data >= 0.5){
                    return "The image is a cat";
                }
                else{
                    return "The image is a dog";
                }
                return result.ToString();
            }
        }
        catch (Exception e)
        {
            return null;
        }

    }

}