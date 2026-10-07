namespace InsaatProjeleri.Models;

public class Project
{
    public int Id { get; set; }
    public string ProjeAdi { get; set; } = "";
    public string ProjeAciklamasi { get; set; } = "";
    public decimal ProjeDegeri { get; set; }
    public DateTime EklenmeTarihi { get; set; }
    public List<ProjectPhoto> Fotograflar { get; set; } = new();
}
