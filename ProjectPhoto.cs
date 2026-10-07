namespace InsaatProjeleri.Models;

public class ProjectPhoto
{
    public int Id { get; set; }
    public int ProjeId { get; set; }
    public string DosyaAdi { get; set; } = "";
    public string MimeType { get; set; } = "image/jpeg";
    public int SiraNo { get; set; }
}
