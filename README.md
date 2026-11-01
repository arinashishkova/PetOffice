# 🐾 PetOffice

PetOffice on WPF-rakendus, kus erinevad kontoriloomad töötavad samas kontoris, kasutavad oma erivõimeid ja reageerivad teiste loomade tegevustele.

Projekt kasutab pärilust, liideseid, polümorfismi, geneerilist repositooriumi, sündmusi, LINQ-i, MVVM-i ja asünkroonset programmeerimist.

## 🚀 Käivitamine

1. Klooni repository.
2. Ava `PetOffice.slnx` Visual Studios.
3. Käivita `PetOffice.WpfApp`.

## 🧩 Põhifunktsioonid

Rakenduses saab:

- lisada, valida ja eemaldada kontoriloomi;
- kasutada `Work`, `CrazyAction` ja liidestest sõltuvaid tegevusi;
- jälgida energiat, ülesandeid ja reaktsioone;
- otsida ja sorteerida loomi;
- vaadata kokkuvõtet ja tegevuste logi.

Loomi hoitakse geneerilistes `Repository<T>` repositooriumides.  
`GetAll()` tagastab read-only vaate.

## 🧮 CPU Analysis

`CPU Analysis` vahelehel saab valida vähemalt 3 looma ja võrrelda kahte arvutusrežiimi:

- **Sequential** – objektid arvutatakse ükshaaval;
- **Parallel** – arvutused käivitatakse `Task.Run()` abil ja oodatakse `Task.WhenAll()`-iga.

Mõlemas režiimis kasutatakse samu snapshot-andmeid.

Arvutuste aega mõõdetakse `Stopwatch` abil.

Raportis kuvatakse:

- looma nimi;
- staatus;
- tulemus või veateade;
- arvutuse kestus;
- Sequential ja Parallel koguaeg.

Võimalikud staatused:

- `Success`
- `Error`
- `Cancelled`

Kõik arvutused kasutavad ühist `CancellationToken`-it ning töötava analüüsi saab `Cancel` nupuga katkestada.

Ühe objekti viga ei katkesta teiste objektide arvutusi.

## ✅ Kontrollitud

Kontrollitud on:

1. Sequential ja Parallel arvutused.
2. Mõlema režiimi tulemuste võrdlus.
3. Ühe objekti `Error`, samal ajal kui teised jätkavad.
4. Arvutuse katkestamine `Cancel` nupuga.
5. Kasutajaliides jääb arvutuse ajal kasutatavaks.

## 🤝 Git-koostöö

Kaasüliõpilane: **Anna Levchenko**

Anna lisas projekti `ParrotReceptionist` alamklassi eraldi branch'is.

Issue:  
https://github.com/arinashishkova/PetOffice/issues/1

Pull Request:  
https://github.com/arinashishkova/PetOffice/pull/2

## 🤖 AI kasutamine

Projekti tegemisel kasutati **ChatGPT** C#, WPF-i, MVVM-i, asünkroonse programmeerimise ja GitHubi töövoo selgitamiseks, koodivigade leidmise ja parandamise abistamiseks ning README koostamise toetamiseks.

Lahendust kontrolliti Visual Studios ja funktsionaalsust testiti käsitsi.
## 🛠️ Tehnoloogiad

- C#
- .NET 10
- WPF
- MVVM
- LINQ
- async / await
- Git
- GitHub