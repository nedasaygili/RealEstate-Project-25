# 🏠 Real Estate Management System (Emlak Yönetim Sistemi)

Bu proje, **ASP.NET Core MVC** mimarisi kullanılarak geliştirilmiş kapsamlı bir emlak yönetim platformudur. Sistem, emlak ilanlarının listelenmesi, detaylı görüntülenmesi ve emlak danışmanları (Agent) tarafından yönetilmesi süreçlerini kapsar.

## 🛠 Gereksinimler (Prerequisites)

Projenin sorunsuz çalışması için aşağıdaki bileşenlerin yüklü olması önerilir:
* Visual Studio 2022
* .NET SDK (.NET 6.0 veya üzeri)
* SQL Server (Visual Studio ile gelen **LocalDB** yeterlidir)

## 🚀 Kurulum ve Veritabanı Oluşturma (Installation)

Proje `appsettings.json` üzerinden evrensel **LocalDB** ayarlarına göre yapılandırılmıştır. Herhangi bir kod değişikliği yapmadan veritabanını oluşturmak için şu adımları izleyiniz:

1.  Projeyi **Visual Studio** ile açın.
2.  Üst menüden **Tools** > **NuGet Package Manager** > **Package Manager Console** yolunu izleyin.
3.  Açılan konsola aşağıdaki komutu yazıp `Enter` tuşuna basın:

```powershell
Update-Database

### ⚠️ ÖNEMLİ: İlk Çalıştırma ve Test Adımları

Veritabanı "Code-First" yaklaşımıyla sıfırdan oluşturulduğu için **içeriği boş** olarak gelecektir. Projenin fonksiyonlarını (Listeleme, Detay Sayfası, Filtreleme vb.) sağlıklı bir şekilde test edebilmek için uygulamayı başlattıktan sonra lütfen sırasıyla şu adımları uygulayınız:

1.  **Agent (Emlak Danışmanı) Oluşturma:**
    Sisteme giriş yapın veya ilgili panelden (Swagger/Arayüz) yeni bir Agent kaydı oluşturun.

2.  **İlan (Property) Ekleme:**
    Oluşturduğunuz Agent'ı kullanarak sisteme en az bir adet emlak ilanı ekleyin.

> **Not:** Veritabanında kayıtlı ilan olmadığı sürece anasayfa ve listeleme sayfaları boş görünecektir. Tam fonksiyonellik için yukarıdaki manuel veri girişi gereklidir.

---

### 💻 Kullanılan Teknolojiler

* **Backend:** ASP.NET Core MVC, C#
* **Veritabanı:** Entity Framework Core, MS SQL Server (LocalDB)
* **Frontend:** HTML5, CSS3, Bootstrap, JavaScript
* **Araçlar:** Swagger UI