
namespace MvcApp.Models;

public class UploadModel
{
    // [Key]
    // public int Id { get; set; }
    // public string Model_Name { get; set; }
    // [NotNull]
    // public string Model { get; set; }
    // public string Model_Preprocessor { get; set; }

    public IFormFile file { get; set; }
    public string data { get; set; }

}
