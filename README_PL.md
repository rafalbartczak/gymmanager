# GymManager — System zarządzania klubem fitness

Aplikacja webowa typu SPA stworzona jako projekt inżynierski w ramach studiów z zakresu inżynierii oprogramowania.

Projekt został obroniony na ocenę **5 z wyróżnieniem**.

GymManager umożliwia zarządzanie klubem fitness, w tym obsługę użytkowników, karnetów, płatności, rezerwacji zajęć, wejść do klubu, ogłoszeń oraz panelu administratora.

Aplikacja została zbudowana w architekturze klient-serwer:

- **ASP.NET Core 8 Web API** — backend
- **Blazor WebAssembly** — frontend
- **SQL Server 2022** — baza danych uruchamiana w kontenerze Docker
- **Entity Framework Core** — ORM i migracje bazy danych
- **JWT Bearer** — uwierzytelnianie i autoryzacja

---

## Spis treści

1. [Stos technologiczny](#stos-technologiczny)
2. [Funkcjonalności](#funkcjonalności)
3. [Wymagania systemowe](#wymagania-systemowe)
4. [Instalacja narzędzi](#instalacja-narzędzi)
5. [Uruchomienie bazy danych](#uruchomienie-bazy-danych)
6. [Uruchomienie aplikacji](#uruchomienie-aplikacji)
7. [Pierwsze logowanie i konto administratora](#pierwsze-logowanie-i-konto-administratora)
8. [Struktura projektu](#struktura-projektu)
9. [Endpointy API](#endpointy-api)
10. [Rozwiązywanie problemów](#rozwiązywanie-problemów)
11. [Autor](#autor)

---

## Stos technologiczny

| Warstwa | Technologia | Wersja |
|---|---|---|
| Backend | ASP.NET Core Web API | 8.0 |
| Frontend | Blazor WebAssembly | 8.0 |
| ORM | Entity Framework Core | 9.0 |
| Baza danych | Microsoft SQL Server | 2022 |
| Konteneryzacja | Docker Compose | v2 |
| Uwierzytelnianie | JWT Bearer | - |
| Hashowanie haseł | PBKDF2 | - |
| Dokumentacja API | Swagger / OpenAPI | 3.0 |

> Projekt korzysta z **.NET 8**, natomiast pakiety Entity Framework Core są w wersji **9.x**.

---

## Funkcjonalności

### Użytkownik

- rejestracja i logowanie,
- uwierzytelnianie JWT z refresh tokenami,
- edycja profilu użytkownika,
- eksport danych użytkownika,
- usunięcie konta,
- zakup karnetu,
- przegląd aktywnych i historycznych karnetów,
- przegląd harmonogramu zajęć grupowych,
- rezerwacja i anulowanie rezerwacji na zajęcia,
- historia wejść do klubu,
- obsługa kodu QR,
- przegląd ogłoszeń.

### Administrator

- przegląd listy użytkowników,
- szczegóły kont użytkowników,
- przypisywanie i anulowanie karnetów,
- zarządzanie typami zajęć,
- zarządzanie sesjami zajęć,
- przegląd list obecności,
- ręczna rejestracja wejść,
- weryfikacja wejść użytkowników,
- zarządzanie ogłoszeniami,
- publikowanie i ukrywanie ogłoszeń.

---

## Wymagania systemowe

| Wymaganie | Opis |
|---|---|
| System operacyjny | Windows 10/11 64-bit, macOS lub Linux |
| RAM | Minimum 8 GB |
| Wolne miejsce | Około 5 GB |
| Przeglądarka | Chrome, Edge lub Firefox z obsługą WebAssembly |
| Docker | Wymagany do uruchomienia SQL Servera |
| .NET SDK | Wersja 8.x |

SQL Server uruchamiany w Dockerze wymaga minimum około 2 GB RAM.

---

## Instalacja narzędzi

Na czystym komputerze należy zainstalować trzy narzędzia:

1. .NET 8 SDK
2. Docker Desktop
3. Entity Framework Core CLI

---

### 1. .NET 8 SDK

SDK zawiera kompilator, narzędzia CLI `dotnet` oraz środowisko uruchomieniowe.

1. Pobierz instalator ze strony Microsoft:
   - `.NET SDK 8.0`
2. Uruchom instalator i przejdź przez kreator.
3. Otwórz nowy terminal i sprawdź instalację:

```bash
dotnet --version
```

Oczekiwany wynik:

```text
8.0.xxx
```

---

### 2. Docker Desktop

Docker jest potrzebny do uruchomienia bazy danych SQL Server 2022 w kontenerze.

1. Pobierz i zainstaluj Docker Desktop.
2. Na Windowsie może być wymagane włączenie WSL 2.
3. Po instalacji zrestartuj komputer.
4. Uruchom Docker Desktop i poczekaj, aż status będzie ustawiony na `Running`.

Sprawdź instalację:

```bash
docker --version
docker compose version
```

Oczekiwany wynik:

```text
Docker version 2x.x.x
Docker Compose version v2.x.x lub nowsza
```

Jeśli Docker Desktop nie uruchamia się i wyświetla błąd dotyczący wirtualizacji, należy włączyć ją w BIOS-ie/UEFI. Zazwyczaj opcja nazywa się `Intel VT-x` albo `AMD-V`.

---

### 3. Entity Framework Core CLI

Po zainstalowaniu .NET SDK zainstaluj globalne narzędzie EF Core CLI w wersji 9.x:

```bash
dotnet tool install --global dotnet-ef --version 9.0.*
```

Sprawdź wersję:

```bash
dotnet ef --version
```

Oczekiwany wynik:

```text
9.0.x
```

Jeśli zainstalowana jest inna wersja, np. 10.x, odinstaluj narzędzie i zainstaluj ponownie właściwą wersję:

```bash
dotnet tool uninstall --global dotnet-ef
dotnet tool install --global dotnet-ef --version 9.0.*
```

---

## Uruchomienie bazy danych

### 1. Uruchomienie kontenera SQL Server

Otwórz terminal w katalogu głównym rozwiązania, czyli tam, gdzie znajduje się plik `docker-compose.yml`, i wykonaj:

```bash
docker compose up -d
```

Polecenie pobierze obraz SQL Server 2022 i uruchomi kontener o nazwie:

```text
gym_sql
```

Baza danych będzie dostępna na porcie:

```text
1433
```

Sprawdź, czy kontener działa:

```bash
docker ps
```

W wynikach powinna pojawić się pozycja `gym_sql` ze statusem `Up`.

---

### 2. Zastosowanie migracji

Przejdź do katalogu projektu API:

```bash
cd GymManager.Api
```

Następnie zastosuj migracje:

```bash
dotnet ef database update
```

Polecenie utworzy bazę danych:

```text
GymManagerDb
```

oraz wszystkie wymagane tabele, m.in.:

- `Users`
- `Passes`
- `Payments`
- `PassTypes`
- `ClassTypes`
- `ClassSessions`
- `ClassReservations`
- `Entries`
- `RefreshTokens`
- `Announcements`

Oczekiwany wynik końcowy:

```text
Done.
```

---

## Uruchomienie aplikacji

Aplikacja składa się z dwóch projektów, które należy uruchomić jednocześnie w osobnych oknach terminala:

- `GymManager.Api` — backend
- `GymManager.Client` — frontend

---

### 1. Uruchomienie API

W pierwszym terminalu przejdź do katalogu API:

```bash
cd GymManager.Api
```

Uruchom projekt:

```bash
dotnet run --launch-profile https
```

API uruchomi się pod adresami:

```text
https://localhost:7048
http://localhost:5048
```

Swagger UI będzie dostępny pod adresem:

```text
https://localhost:7048/swagger
```

Przy pierwszym uruchomieniu .NET może wygenerować lokalny certyfikat HTTPS. Jeśli przeglądarka wyświetli ostrzeżenie o certyfikacie, można zaakceptować wyjątek albo zaufać certyfikatowi globalnie:

```bash
dotnet dev-certs https --trust
```

---

### 2. Uruchomienie klienta Blazor

W drugim terminalu przejdź do katalogu klienta:

```bash
cd GymManager.Client
```

Uruchom projekt:

```bash
dotnet run --launch-profile https
```

Frontend uruchomi się pod adresem:

```text
https://localhost:7132
```

Otwórz ten adres w przeglądarce.

---

### 3. Szybki start

Terminal 1 — baza danych i migracje:

```bash
docker compose up -d

cd GymManager.Api
dotnet ef database update
dotnet run --launch-profile https
```

Terminal 2 — klient:

```bash
cd GymManager.Client
dotnet run --launch-profile https
```

Przeglądarka:

```text
https://localhost:7132
```

---

## Pierwsze logowanie i konto administratora

### 1. Rejestracja konta użytkownika

1. Otwórz aplikację pod adresem:

```text
https://localhost:7132
```

2. Kliknij `Zarejestruj się`.
3. Wypełnij formularz rejestracji.
4. Zaakceptuj regulamin i politykę prywatności.
5. Po rejestracji nastąpi automatyczne zalogowanie.

Nowo utworzone konto otrzymuje domyślną rolę:

```text
user
```

---

### 2. Nadanie roli administratora

Aby nadać użytkownikowi rolę administratora, należy zmienić jego rolę bezpośrednio w bazie danych.

Połącz się z bazą przez `sqlcmd` uruchomiony w kontenerze Dockera:

```bash
docker exec -it gym_sql /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Change_This_Password_123!" -C -Q "UPDATE GymManagerDb.dbo.Users SET Role = 'admin' WHERE Email = 'twoj@email.com';"
```

Zastąp:

```text
twoj@email.com
```

adresem e-mail konta, które ma zostać administratorem.

Po zmianie roli wyloguj się i zaloguj ponownie. Nowy token JWT będzie zawierał zaktualizowaną rolę.

---

### 3. Dodanie typów karnetów

Aby użytkownicy mogli kupować karnety, administrator musi dodać typy karnetów do bazy danych.

Przykładowe typy karnetów można dodać poleceniem:

```bash
docker exec -it gym_sql /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Change_This_Password_123!" -C -Q "INSERT INTO GymManagerDb.dbo.PassTypes (PassTypeId,Name,Description,DurationDays,Price,Currency,IsActive,CreatedAt) VALUES (NEWID(),'Miesieczny','Karnet na 30 dni',30,99.00,'PLN',1,SYSUTCDATETIME()),(NEWID(),'Kwartalny','Karnet na 90 dni',90,249.00,'PLN',1,SYSUTCDATETIME()),(NEWID(),'Polroczny','Karnet na 180 dni',180,449.00,'PLN',1,SYSUTCDATETIME()),(NEWID(),'Roczny','Karnet na 365 dni',365,799.00,'PLN',1,SYSUTCDATETIME());"
```

---

## Struktura projektu

```text
GymManager/
  docker-compose.yml
  GymManager.sln

  GymManager.Api/
    Program.cs
    appsettings.json
    appsettings.Development.json

    Controllers/
      AuthController.cs
      ProfileController.cs
      PassesController.cs
      PassTypesController.cs
      ClassesController.cs
      EntriesController.cs
      AnnouncementsController.cs
      AdminUsersController.cs
      AdminPassesController.cs

    Data/
      AppDbContext.cs
      Configurations/

    Domain/
      Entities/
        User.cs
        RefreshToken.cs
        Pass.cs
        Payment.cs
        PassType.cs
        ClassType.cs
        ClassSession.cs
        ClassReservation.cs
        Entry.cs
        Announcement.cs

    Services/
      Security/
        PasswordService.cs
        JwtTokenService.cs
        RefreshTokenService.cs

    Dtos/
    Migrations/

  GymManager.Client/
    Program.cs
    App.razor

    Layout/
      MainLayout.razor

    Pages/
      Home.razor
      Login.razor
      Register.razor
      Profile.razor
      PassTypes.razor
      MyPasses.razor
      ClassesSchedule.razor
      CheckIn.razor
      MyQrCode.razor
      MyEntries.razor
      Announcements.razor

      Admin/
        AdminUsers.razor
        AdminUserDetails.razor
        AdminClasses.razor
        AdminScanner.razor
        AdminEntries.razor
        AnnouncementsAdmin.razor

    Services/
      ApiHttpClient.cs

      Auth/
        TokenStore.cs
        AuthApiClient.cs
        AuthorizedHandler.cs
        JwtHelper.cs

    Contracts/
    wwwroot/
```

---

## Endpointy API

Pełna interaktywna dokumentacja API jest dostępna po uruchomieniu backendu pod adresem:

```text
https://localhost:7048/swagger
```

---

### Endpointy publiczne

| Metoda | Endpoint | Opis |
|---|---|---|
| POST | `/auth/register` | Rejestracja nowego użytkownika |
| POST | `/auth/login` | Logowanie użytkownika |
| POST | `/auth/refresh` | Odświeżenie tokenu |
| POST | `/auth/logout` | Wylogowanie użytkownika |

---

### Endpointy użytkownika

Wymagają zalogowania.

| Metoda | Endpoint | Opis |
|---|---|---|
| GET | `/auth/me` | Dane zalogowanego użytkownika |
| GET | `/profile` | Dane profilu |
| PUT | `/profile` | Edycja profilu |
| GET | `/profile/export` | Eksport danych użytkownika |
| DELETE | `/profile` | Usunięcie konta |
| GET | `/passtypes` | Lista dostępnych typów karnetów |
| GET | `/passes/me` | Moje karnety |
| POST | `/passes/buy` | Inicjalizacja zakupu karnetu |
| POST | `/passes/confirm` | Potwierdzenie płatności |
| GET | `/classes/types` | Aktywne typy zajęć |
| GET | `/classes/schedule` | Harmonogram zajęć |
| POST | `/classes/sessions/{id}/reserve` | Rezerwacja na zajęcia |
| DELETE | `/classes/sessions/{id}/reserve` | Anulowanie rezerwacji |
| GET | `/classes/me` | Moje nadchodzące rezerwacje |
| POST | `/entries/checkin` | Samoobsługowe wejście |
| GET | `/entries/me` | Historia moich wejść |
| GET | `/announcements` | Opublikowane ogłoszenia |

---

### Endpointy administratora

Wymagają roli administratora.

| Metoda | Endpoint | Opis |
|---|---|---|
| GET | `/admin/users` | Lista użytkowników |
| GET | `/admin/users/{id}` | Szczegóły użytkownika |
| POST | `/admin/passes/assign` | Przypisanie karnetu |
| POST | `/admin/passes/cancel` | Anulowanie karnetu |
| GET | `/classes/types/all` | Wszystkie typy zajęć |
| POST | `/classes/types` | Dodanie typu zajęć |
| PUT | `/classes/types/{id}` | Edycja typu zajęć |
| DELETE | `/classes/types/{id}` | Usunięcie typu zajęć |
| POST | `/classes/sessions` | Dodanie sesji zajęć |
| PUT | `/classes/sessions/{id}` | Edycja sesji zajęć |
| PATCH | `/classes/sessions/{id}/cancel` | Odwołanie lub przywrócenie sesji |
| DELETE | `/classes/sessions/{id}` | Usunięcie sesji |
| DELETE | `/classes/sessions/{sid}/reservations/{uid}` | Wypisanie użytkownika z zajęć |
| GET | `/classes/sessions/{id}/attendance` | Lista obecności |
| GET | `/entries` | Historia wszystkich wejść |
| POST | `/entries/verify` | Weryfikacja kodu QR użytkownika |
| POST | `/entries/manual` | Ręczna rejestracja wejścia |
| GET | `/announcements/admin` | Wszystkie ogłoszenia |
| POST | `/announcements` | Dodanie ogłoszenia |
| PATCH | `/announcements/{id}/publish` | Publikacja lub ukrycie ogłoszenia |
| DELETE | `/announcements/{id}` | Usunięcie ogłoszenia |

---

## Rozwiązywanie problemów

### Docker: kontener nie startuje

Sprawdź logi kontenera:

```bash
docker logs gym_sql
```

Najczęstsza przyczyna to zbyt mała ilość pamięci RAM przypisana do Docker Desktop. SQL Server wymaga minimum około 2 GB RAM.

W Docker Desktop sprawdź:

```text
Settings -> Resources
```

---

### Docker Desktop nie uruchamia się po instalacji

Jeśli pojawia się błąd dotyczący wirtualizacji lub WSL:

1. Zrestartuj komputer.
2. Wejdź do BIOS-u/UEFI.
3. Włącz wirtualizację:
   - `Intel VT-x` dla procesorów Intel,
   - `AMD-V` dla procesorów AMD.
4. Zapisz ustawienia i uruchom komputer ponownie.

---

### Błąd połączenia z bazą danych przy migracji

Przykładowy komunikat:

```text
A connection was successfully established with the server, but then an error occurred
```

Sprawdź, czy kontener działa:

```bash
docker ps
```

Jeśli kontener został dopiero uruchomiony, SQL Server może potrzebować kilkunastu sekund na start. Poczekaj chwilę i spróbuj ponownie.

---

### Przeglądarka nie otwiera strony klienta poprawnie

Upewnij się, że oba projekty są uruchomione jednocześnie:

- `GymManager.Api`
- `GymManager.Client`

Klient Blazor wymaga działającego API do prawidłowego funkcjonowania.

---

### Ostrzeżenie o certyfikacie HTTPS

Przy pierwszym uruchomieniu w trybie deweloperskim .NET generuje samopodpisany certyfikat.

Aby zaufać certyfikatowi lokalnie:

```bash
dotnet dev-certs https --trust
```

Następnie zrestartuj przeglądarkę.

---

### Port 1433 jest już zajęty

Jeśli inny proces, np. lokalna instancja SQL Server, używa portu `1433`, zmień port w pliku `docker-compose.yml`:

```yml
ports:
  - "1434:1433"
```

Następnie zaktualizuj connection string w `appsettings.Development.json`:

```text
Server=localhost,1434;...
```

---

### Reset bazy danych

Aby usunąć kontener oraz dane i utworzyć bazę od nowa:

```bash
docker compose down -v
docker compose up -d

cd GymManager.Api
dotnet ef database update
```

---

### Błąd `Unable to retrieve project metadata`

Najczęstsza przyczyna to niezgodna wersja narzędzia `dotnet-ef`.

Sprawdź wersję:

```bash
dotnet ef --version
```

Jeśli wynik to `10.x` albo inna wersja niż `9.0.x`, odinstaluj narzędzie i zainstaluj poprawną wersję:

```bash
dotnet tool uninstall --global dotnet-ef
dotnet tool install --global dotnet-ef --version 9.0.*
```

---

## Autor

**Rafał Bartczak**  
Praca inżynierska, 2026  
Kierunek: Inżynieria oprogramowania
