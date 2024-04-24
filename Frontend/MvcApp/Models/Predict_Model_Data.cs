using Microsoft.AspNetCore.Mvc;

namespace MvcApp.Models;

public class PredictModelData
{
    public byte[] input_file { get; set; }
    public string bucket_name { get; set; }
    public string project_id { get; set; }
}