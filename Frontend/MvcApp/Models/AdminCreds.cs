using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace MvcApp.Models;

public class AdminCreds
{
    [Key]
    public int Id { get; set; }
    [NotNull]
    public string name { get; set; }
    [NotNull]
    public string username { get; set; }
    [NotNull]
    public string password { get; set; }

}