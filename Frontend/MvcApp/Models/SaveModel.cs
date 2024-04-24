
using System.ComponentModel.DataAnnotations;

namespace MvcApp.Models;

public class SaveModel
{
    [Key]
    public int Id { get; set; }
    public string modelname { get; set; }
    public string model_id { get; set; }
    public string bucket_name { get; set; }

}