using InsaatProjeleri.Models;
using MySqlConnector;

namespace InsaatProjeleri.Services;

public class DatabaseService
{
    private readonly string _cs;
    private readonly IWebHostEnvironment _env;

    public DatabaseService(IConfiguration config, IWebHostEnvironment env)
    {
        _cs = config.GetConnectionString("DefaultConnection")
              ?? throw new InvalidOperationException("DefaultConnection bulunamadı.");
        _env = env;
    }

    private MySqlConnection OpenConnection() => new(_cs);

    public async Task InitializeAndSeedAsync()
    {
        await using var con = OpenConnection();
        await con.OpenAsync();

        var createSql = @"
CREATE TABLE IF NOT EXISTS projeler (
    id INT AUTO_INCREMENT PRIMARY KEY,
    proje_adi VARCHAR(250) NOT NULL,
    proje_aciklamasi TEXT,
    proje_degeri DECIMAL(18,2),
    eklenme_tarihi DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

CREATE TABLE IF NOT EXISTS proje_fotograflari (
    id INT AUTO_INCREMENT PRIMARY KEY,
    proje_id INT NOT NULL,
    dosya_adi VARCHAR(255),
    mime_type VARCHAR(100),
    fotograf LONGBLOB NOT NULL,
    sira_no INT NOT NULL DEFAULT 0,
    CONSTRAINT fk_proje_fotograflari_proje
        FOREIGN KEY (proje_id) REFERENCES projeler(id) ON DELETE CASCADE
) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;";
        await using (var cmd = new MySqlCommand(createSql, con))
            await cmd.ExecuteNonQueryAsync();

        await using var countCmd = new MySqlCommand("SELECT COUNT(*) FROM projeler;", con);
        var count = Convert.ToInt32(await countCmd.ExecuteScalarAsync());
        if (count > 0) return;

        var samples = new[]
        {
            new { Name="Etimesgut 24 Derslikli İlkokul", Desc="Modern eğitim standartlarına uygun olarak tamamlanan 24 derslikli ilkokul projesi; geniş bahçe alanı, çok amaçlı salon ve öğrenci etkinlik alanlarıyla tasarlanmıştır.", Value=48500000m, Prefix="p1" },
            new { Name="Sincan Anadolu Lisesi", Desc="32 derslikli, laboratuvarları, kütüphanesi ve spor alanları bulunan modern Anadolu lisesi projesidir.", Value=72000000m, Prefix="p2" },
            new { Name="Yenimahalle Anaokulu", Desc="12 derslikli, oyun alanları, güvenli açık bahçe ve çok amaçlı etkinlik salonuna sahip anaokulu projesidir.", Value=28300000m, Prefix="p3" },
            new { Name="Çankaya Ortaokulu", Desc="20 derslikli, kapalı spor salonu, fen laboratuvarı ve sosyal etkinlik alanları bulunan ortaokul projesidir.", Value=55750000m, Prefix="p4" },
            new { Name="Keçiören Mesleki ve Teknik Anadolu Lisesi", Desc="28 derslikli; atölye, laboratuvar ve uygulama sınıflarıyla mesleki eğitime yönelik olarak tamamlanan lise projesidir.", Value=68200000m, Prefix="p5" },
            new { Name="Mamak Fen Lisesi", Desc="24 derslikli, fen laboratuvarları, çalışma salonları ve bilimsel etkinlik alanlarıyla tamamlanan fen lisesi projesidir.", Value=61400000m, Prefix="p6" }
        };

        foreach (var p in samples)
        {
            await using var tx = await con.BeginTransactionAsync();

            var insert = new MySqlCommand(
                "INSERT INTO projeler(proje_adi,proje_aciklamasi,proje_degeri) VALUES(@ad,@acik,@deger); SELECT LAST_INSERT_ID();",
                con, tx);
            insert.Parameters.AddWithValue("@ad", p.Name);
            insert.Parameters.AddWithValue("@acik", p.Desc);
            insert.Parameters.AddWithValue("@deger", p.Value);
            var projectId = Convert.ToInt32(await insert.ExecuteScalarAsync());

            for (int i=1;i<=3;i++)
            {
                var file = Path.Combine(AppContext.BaseDirectory, "SeedImages", $"{p.Prefix}_{i}.jpg");
                if (!File.Exists(file))
                    file = Path.Combine(_env.ContentRootPath, "SeedImages", $"{p.Prefix}_{i}.jpg");
                if (!File.Exists(file)) continue;

                var bytes = await File.ReadAllBytesAsync(file);
                var pic = new MySqlCommand(
                    @"INSERT INTO proje_fotograflari(proje_id,dosya_adi,mime_type,fotograf,sira_no)
                      VALUES(@pid,@name,'image/jpeg',@data,@sira);", con, tx);
                pic.Parameters.AddWithValue("@pid", projectId);
                pic.Parameters.AddWithValue("@name", Path.GetFileName(file));
                pic.Parameters.AddWithValue("@data", bytes);
                pic.Parameters.AddWithValue("@sira", i);
                await pic.ExecuteNonQueryAsync();
            }

            await tx.CommitAsync();
        }
    }

    public async Task<List<Project>> GetProjectsAsync()
    {
        var result = new List<Project>();
        await using var con = OpenConnection();
        await con.OpenAsync();

        var cmd = new MySqlCommand(
            @"SELECT p.id,p.proje_adi,p.proje_aciklamasi,p.proje_degeri,p.eklenme_tarihi,
                     f.id AS foto_id,f.dosya_adi,f.mime_type,f.sira_no
              FROM projeler p
              LEFT JOIN proje_fotograflari f ON f.proje_id=p.id
              ORDER BY p.id DESC, f.sira_no, f.id;", con);

        await using var r = await cmd.ExecuteReaderAsync();
        var map = new Dictionary<int, Project>();
        while (await r.ReadAsync())
        {
            var id = r.GetInt32("id");
            if (!map.TryGetValue(id, out var p))
            {
                p = new Project {
                    Id=id,
                    ProjeAdi=r.GetString("proje_adi"),
                    ProjeAciklamasi=r.IsDBNull("proje_aciklamasi") ? "" : r.GetString("proje_aciklamasi"),
                    ProjeDegeri=r.IsDBNull("proje_degeri") ? 0 : r.GetDecimal("proje_degeri"),
                    EklenmeTarihi=r.GetDateTime("eklenme_tarihi")
                };
                map[id]=p;
                result.Add(p);
            }
            if (!r.IsDBNull("foto_id"))
            {
                p.Fotograflar.Add(new ProjectPhoto {
                    Id=r.GetInt32("foto_id"),
                    ProjeId=id,
                    DosyaAdi=r.IsDBNull("dosya_adi") ? "" : r.GetString("dosya_adi"),
                    MimeType=r.IsDBNull("mime_type") ? "image/jpeg" : r.GetString("mime_type"),
                    SiraNo=r.IsDBNull("sira_no") ? 0 : r.GetInt32("sira_no")
                });
            }
        }
        return result;
    }

    public async Task<Project?> GetProjectAsync(int id)
    {
        var all = await GetProjectsAsync();
        return all.FirstOrDefault(x => x.Id == id);
    }

    public async Task<(byte[] Data, string Mime, string Name)?> GetPhotoAsync(int id)
    {
        await using var con = OpenConnection();
        await con.OpenAsync();
        var cmd = new MySqlCommand("SELECT fotograf,mime_type,dosya_adi FROM proje_fotograflari WHERE id=@id;", con);
        cmd.Parameters.AddWithValue("@id", id);
        await using var r = await cmd.ExecuteReaderAsync();
        if (!await r.ReadAsync()) return null;
        return ((byte[])r["fotograf"],
                r.IsDBNull("mime_type") ? "image/jpeg" : r.GetString("mime_type"),
                r.IsDBNull("dosya_adi") ? "foto.jpg" : r.GetString("dosya_adi"));
    }

    public async Task<int> CreateProjectAsync(ProjectForm model, IEnumerable<IFormFile> photos)
    {
        await using var con = OpenConnection();
        await con.OpenAsync();
        await using var tx = await con.BeginTransactionAsync();

        var cmd = new MySqlCommand(
            "INSERT INTO projeler(proje_adi,proje_aciklamasi,proje_degeri) VALUES(@a,@d,@v); SELECT LAST_INSERT_ID();", con, tx);
        cmd.Parameters.AddWithValue("@a", model.ProjeAdi.Trim());
        cmd.Parameters.AddWithValue("@d", model.ProjeAciklamasi?.Trim() ?? "");
        cmd.Parameters.AddWithValue("@v", model.ProjeDegeri);
        var id = Convert.ToInt32(await cmd.ExecuteScalarAsync());

        await InsertPhotosAsync(con, tx, id, photos);
        await tx.CommitAsync();
        return id;
    }

    public async Task UpdateProjectAsync(ProjectForm model, IEnumerable<IFormFile> newPhotos)
    {
        await using var con = OpenConnection();
        await con.OpenAsync();
        await using var tx = await con.BeginTransactionAsync();

        var cmd = new MySqlCommand(
            "UPDATE projeler SET proje_adi=@a,proje_aciklamasi=@d,proje_degeri=@v WHERE id=@id;", con, tx);
        cmd.Parameters.AddWithValue("@a", model.ProjeAdi.Trim());
        cmd.Parameters.AddWithValue("@d", model.ProjeAciklamasi?.Trim() ?? "");
        cmd.Parameters.AddWithValue("@v", model.ProjeDegeri);
        cmd.Parameters.AddWithValue("@id", model.Id);
        await cmd.ExecuteNonQueryAsync();

        await InsertPhotosAsync(con, tx, model.Id, newPhotos);
        await tx.CommitAsync();
    }

    private static async Task InsertPhotosAsync(MySqlConnection con, MySqlTransaction tx, int projectId, IEnumerable<IFormFile> photos)
    {
        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "image/jpeg", "image/png", "image/webp" };
        var list = photos?.Where(x => x != null && x.Length > 0).Take(10).ToList() ?? new List<IFormFile>();
        if (list.Count == 0) return;

        var maxCmd = new MySqlCommand("SELECT COALESCE(MAX(sira_no),0) FROM proje_fotograflari WHERE proje_id=@id;", con, tx);
        maxCmd.Parameters.AddWithValue("@id", projectId);
        var order = Convert.ToInt32(await maxCmd.ExecuteScalarAsync());

        foreach (var file in list)
        {
            if (!allowed.Contains(file.ContentType) || file.Length > 5L*1024L*1024L) continue;
            await using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var cmd = new MySqlCommand(
                @"INSERT INTO proje_fotograflari(proje_id,dosya_adi,mime_type,fotograf,sira_no)
                  VALUES(@pid,@name,@mime,@data,@sira);", con, tx);
            cmd.Parameters.AddWithValue("@pid", projectId);
            cmd.Parameters.AddWithValue("@name", Path.GetFileName(file.FileName));
            cmd.Parameters.AddWithValue("@mime", file.ContentType);
            cmd.Parameters.AddWithValue("@data", ms.ToArray());
            cmd.Parameters.AddWithValue("@sira", ++order);
            await cmd.ExecuteNonQueryAsync();
        }
    }

    public async Task DeleteProjectAsync(int id)
    {
        await using var con = OpenConnection();
        await con.OpenAsync();
        var cmd = new MySqlCommand("DELETE FROM projeler WHERE id=@id;", con);
        cmd.Parameters.AddWithValue("@id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeletePhotoAsync(int id, int projectId)
    {
        await using var con = OpenConnection();
        await con.OpenAsync();
        var cmd = new MySqlCommand("DELETE FROM proje_fotograflari WHERE id=@id AND proje_id=@pid;", con);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@pid", projectId);
        await cmd.ExecuteNonQueryAsync();
    }
}
