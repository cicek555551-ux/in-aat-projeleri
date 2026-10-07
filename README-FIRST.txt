İNŞAAT PROJELERİ - ASP.NET Core 8 / MySQL
===============================================

Bu paket kaynak projedir. MonsterASP.NET Git Deploy ile derlenmek üzere hazırlanmıştır.

Özellikler
----------
- Ana sayfada banner, proje adına göre arama, 3 sütunlu proje kartları.
- Kart içi fotoğraf sliderı.
- Detay modalı ve büyük fotoğraf galerisi.
- MySQL veritabanından proje ve fotoğraf okuma.
- Gizli yönetim adresi: /yonetim-8f42
- Yeni proje ekleme.
- Proje düzenleme.
- Proje silme.
- Birden fazla fotoğraf yükleme.
- Tek tek fotoğraf silme.
- İlk çalıştırmada veritabanı boşsa 6 örnek proje ve her proje için 3 örnek fotoğraf otomatik eklenir.
- Fotoğraflar proje_fotograflari.fotograf alanında LONGBLOB olarak tutulur.

VERİTABANI
----------
Server   : db72254.databaseasp.net
Port     : 3306
Database : db72254
User     : db72254

Bağlantı appsettings.json içindedir.

ÖNEMLİ
-----
1) Bu klasör doğrudan wwwroot'a kopyalanacak "publish" değildir.
2) .NET SDK bulunmadığı için burada derlenmiş DLL üretilemedi.
3) MonsterASP.NET panelindeki "Git deploy" özelliğiyle bu kaynak repo bağlanmalı.
4) Deployment platformu projeyi dotnet restore / dotnet publish ile derlemelidir.
5) Site ilk kez açıldığında tablolar mevcutsa kullanır; projeler tablosu boşsa örnek kayıtları ekler.
6) Örnek kayıtlar bir kere eklendikten sonra tekrar eklenmez.
7) Test bittikten sonra veritabanı şifresini değiştirmeniz önerilir.

YÖNETİM
-------
https://SITE-ADRESINIZ/yonetim-8f42

Bu sürümde login yoktur; URL'yi bilen kişi yönetim paneline erişebilir.
