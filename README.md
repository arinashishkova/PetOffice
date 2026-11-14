# 🐾 PetOffice

PetOffice on C#-i ja WPF-iga loodud rakendus, kus erinevad kontoriloomad töötavad, kasutavad oma erivõimeid ja reageerivad teiste sama tüüpi loomade tegevustele.

Projektis kasutatakse pärilust, liideseid, polümorfismi, geneerilisi repositooriume, sündmusi, LINQ-i, MVVM-i, asünkroonset programmeerimist ja Dependency Injection'it.

## 🚀 Käivitamine

**Eeldused:** Windows, Visual Studio koos .NET-i töölauaarenduse (WPF) tööriistadega ning .NET 10 SDK.

1. Klooni repositoorium.
2. Ava `PetOffice.slnx` Visual Studios.
3. Vali käivitatavaks projektiks `PetOffice.WpfApp` ja käivita rakendus.

JSON-i ja XML-i salvestamiseks ei ole vaja eraldi andmebaasi. LocalDB kasutamiseks peab olema paigaldatud SQL Server Express LocalDB.

## 🧩 Põhifunktsioonid

Rakenduses saab:

- lisada, valida ja eemaldada kontoriloomi;
- kasutada `Work`, `CrazyAction` ja liidestest sõltuvaid eritegevusi;
- jälgida energiat, tehtud ülesandeid ja reaktsioone;
- määrata loomadele kontorirühmi: **Pawformance Hub**, **Creative Corner** ja **Tech Den**;
- otsida, sortida ning filtreerida loomi tüübi ja kontorirühma järgi;
- vaadata tegevuste logi ning võrrelda CPU arvutuste režiime;
- salvestada ja taastada kogu maailma olekut JSON-, XML- või LocalDB-salvestusest.

Iga loomatüübi objekte hoitakse eraldi geneerilises `Repository<T>` repositooriumis. `GetAll()` tagastab kirjutuskaitstud vaate. Repositooriumi sündmused võimaldavad sama tüüpi loomadel üksteise tegevustele reageerida.

## 💾 Maailma salvestamine ja taastamine

Vaheleht **Storage** võimaldab salvestada sama maailma olekut kolmel viisil:

- **JSON:** andmed salvestatakse kasutaja valitud `.json` faili.
- **XML:** andmed salvestatakse kasutaja valitud `.xml` faili.
- **LocalDB:** andmed salvestatakse SQL Server LocalDB andmebaasi ADO.NET-i abil.

Kõik kolm teostust kasutavad ühist `IWorldStorage` liidest ja `WorldDto` andmemudelit.

### Salvestatavad andmed

Iga looma kohta säilitatakse:

- unikaalne `Guid`-identifikaator (`Id`);
- konkreetne tüüp (`Type`) ja nimi (`Name`);
- kontorirühm (`GroupName`);
- energia (`Energy`);
- lõpetatud ülesannete arv (`TasksCompleted`);
- reaktsioonide arv (`ReactionsCount`);
- repositooriumi tunnus (`RepositoryName`) faili-DTO-s.

Salvestamiseks kasutatakse `WorldDto` ja `PetDto` klasse, mitte WPF-aknaid, käske ega sündmuste tellimusi.

Laadimisel luuakse DTO-de põhjal uuesti õiged loomatüübid ning taastatakse repositooriumide sündmuste seosed.

Enne senise maailma asendamist loetakse ja kontrollitakse laaditud andmeid. Kui fail on vigane või selles olevad andmed on sobimatud, kuvatakse veateade ning senine maailm säilib.

### JSON ja XML kasutamine

1. Lisa loomi, muuda nende energiat või tehtud ülesandeid ning kontorirühma.
2. Ava **Storage** ja vajuta `Save JSON` või `Save XML`.
3. Vali faili asukoht ning salvesta.
4. Sulge rakendus ja käivita see uuesti.
5. Vajuta `Load JSON` või `Load XML` ja vali salvestatud fail.
6. Kontrolli taastatud loomade ID-sid, tüüpe, nimesid, muutuvaid välju ja rühmi.

### LocalDB eeldused ja ühenduse seadistus

LocalDB jaoks peab arvutis olema **Microsoft SQL Server Express LocalDB** ja kasutajal õigus kohalikku andmebaasi luua.

Rakendus kasutab LocalDB eksemplari `(localdb)\MSSQLLocalDB` ning andmebaasi `PetOfficeDb`.

Eksemplari olemasolu saab kontrollida Windowsi terminalis:

```powershell
sqllocaldb info
sqllocaldb info MSSQLLocalDB
```

Vajaduse korral saab olemasoleva eksemplari käivitada käsuga:

```powershell
sqllocaldb start MSSQLLocalDB
```

Kui `sqllocaldb` käsku ei leita, tuleb kontrollida LocalDB paigaldust ja PATH-i.

Ühendus kasutab Windowsi autentimist (`Trusted_Connection=True`).

Ühendusstringid asuvad failis:

`PetOffice.WpfApp/Services/Storage/LocalDbWorldStorage.cs`

Teise eksemplari kasutamiseks tuleb neid seal kohandada.

### Andmebaasi ja tabelite loomine

`LocalDbWorldStorage` kontrollib esimesel andmebaasitoimingul, kas `PetOfficeDb` ja vajalikud tabelid on olemas, ning loob need vajaduse korral automaatselt.

Eraldi SQL-skripti käsitsi käivitamine ei ole tavapäraseks esmakäivituseks vajalik.

Andmebaasis on kaks seotud tabelit:

| Tabel | Olulisemad veerud | Otstarve |
| --- | --- | --- |
| `PetGroups` | `Id` (PK), `Name` (UNIQUE) | Kontorirühmad |
| `Pets` | `Id` (PK, GUID), `Type`, `Name`, `Energy`, `TasksCompleted`, `ReactionsCount`, `GroupId` (FK) | Loomade olek ja rühmakuuluvus |

`Pets.GroupId` viitab tabelile `PetGroups`.

SQL-päringutes kasutatakse parameetreid. Salvestamisel asendatakse andmebaasi senine maailm ühe transaktsiooni sees; vea korral tehakse `Rollback`.

### LocalDB kasutamine

1. Ava rakenduses **Storage**.
2. Vajuta `Save to LocalDB` ja kontrolli olekuteadet.
3. Sulge rakendus ning käivita uuesti.
4. Vajuta `Load from LocalDB`.
5. Võrdle taastatud loomade ID-sid, tüüpe, olekuid ja kontorirühmi.

**Märkus:** LocalDB on seotud kohaliku arvutiga. Andmebaasi sisu ei kandu GitHubi kaudu automaatselt teise arvutisse.

### Dependency Injection

`App.xaml.cs` registreerib JSON-i, XML-i ja LocalDB teostused sama `IWorldStorage` liidese all, kasutades eri võtmeid.

`MainViewModel` saab vajalikud sõltuvused konstruktori kaudu.

Faili asukoha valimiseks kasutatakse eraldi `IFileDialogService` teenust. Lugemise ja kirjutamise I/O toimub salvestusteenustes, mitte kasutajaliidese koodis.

## 🧮 CPU Analysis

Vahelehel **CPU Analysis** saab valida vähemalt kolm looma ja võrrelda kahte arvutusrežiimi:

- **Sequential:** arvutused tehakse ükshaaval.
- **Parallel:** arvutused käivitatakse `Task.Run()` abil ja nende lõppemist oodatakse `Task.WhenAll()`-iga.

Mõlemas režiimis kasutatakse samu loomade oleku hetktõmmiseid (snapshot). Aega mõõdetakse `Stopwatch` abil.

Raportis kuvatakse:

- looma nimi;
- staatus (`Success`, `Error` või `Cancelled`);
- tulemus või veateade;
- arvutuse kestus;
- Sequential ja Parallel koguaeg;
- mõlema režiimi tulemuste võrdlus.

Kõik arvutused kasutavad ühist `CancellationToken`-it ning töötava analüüsi saab `Cancel` nupuga katkestada.

Ühe objekti viga ei pea katkestama teiste objektide arvutusi.

### CPU Analysis kasutamine

1. Vali vähemalt kolm looma.
2. Vali koormus `Small`, `Medium` või `Large`.
3. Vajuta `Compare modes`, et käivitada Sequential ja Parallel arvutused.
4. Töötava arvutuse saab peatada `Cancel` nupuga.

## ✅ Kontrollimine ja demonstreerimine

Arenduse käigus on käsitsi kontrollitud CPU Analysis režiimide võrdlust, veakäsitlust ja katkestamist ning JSON-i, XML-i ja LocalDB salvestamist ja laadimist.

**Enne töö esitamist tuleb läbi proovida kogu demonstratsioon:**

1. Lisa loom, muuda tema olekut ja rühma ning salvesta JSON-i ja XML-i.
2. Taaskäivita rakendus, laadi failid ning võrdle ID-sid, tüüpe ja muutuvaid välju.
3. Käivita taastatud maailmas `CrazyAction` ning kontrolli reaktsioone ja kasutajaliidese toimimist.
4. Proovi vigast või sobimatut faili ning kontrolli, et ilmuks arusaadav veateade ja senine maailm jääks alles.
5. Salvesta LocalDB-sse, taaskäivita, taasta sama olek ning näita DI kaudu kasutatavaid salvestusteenuseid.

## 🤝 Git-koostöö

Kaasüliõpilane: **Anna Levchenko**

Anna lisas `ParrotReceptionist` alamklassi eraldi Git-harus.

- [Issue #1](https://github.com/arinashishkova/PetOffice/issues/1)
- [Pull Request #2](https://github.com/arinashishkova/PetOffice/pull/2)

## 🤖 AI kasutamine

Projekti tegemisel kasutati **ChatGPT-d** C#, WPF-i, MVVM-i, asünkroonse programmeerimise, andmete salvestamise ja GitHubi töövoo selgitamiseks, koodivigade leidmise ning parandamise abistamiseks ja README koostamise toetamiseks.

Lahendust kontrolliti Visual Studios ning funktsionaalsust testiti käsitsi.

## 🛠️ Tehnoloogiad

- C# ja .NET 10
- WPF ja MVVM
- LINQ, sündmused ning `async` / `await`
- JSON ja XML
- SQL Server LocalDB ja ADO.NET
- `Microsoft.Data.SqlClient`
- `Microsoft.Extensions.DependencyInjection`
- Git ja GitHub