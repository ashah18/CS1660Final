
using Microsoft.EntityFrameworkCore;

namespace MvcApp.Models;

[Keyless]
public class vwSavedModel
{
    public int id { get; set; }
    public string bucket_name { get; set; }
    public string model_name { get; set; }

}