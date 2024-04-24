
using Microsoft.EntityFrameworkCore;

namespace MvcApp.Models;
[Keyless]
public class vwModelsandAdmins
{
    public List<vwSavedModel> SavedModels { get; set; }
    public AdminDetails LoginUserDetails { get; set; }
    public List<AdminDetails> adminCreds { get; set; }
}