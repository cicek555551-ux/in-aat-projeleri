using System.ComponentModel.DataAnnotations;

namespace InsaatProjeleri.Models;

public class ProjectForm
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Proje adı zorunludur.")]
    [Display(Name = "Proje Adı")]
    public string ProjeAdi { get; set; } = "";

    [Display(Name = "Proje Açıklaması")]
    public string ProjeAciklamasi { get; set; } = "";

    [Range(0, 999999999999, ErrorMessage = "Geçerli bir proje değeri girin.")]
    [Display(Name = "Proje Değeri")]
    public decimal ProjeDegeri { get; set; }
}
