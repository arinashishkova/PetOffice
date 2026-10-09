# 🐾 PetOffice

PetOffice on humoorikas WPF-rakendus, kus erinevad kontoriloomad töötavad samas kontoris, kuid käituvad erinevalt ja kasutavad oma erivõimeid.

Projekt kasutab pärilust, liideseid, polümorfismi ja ühist objektide kollektsiooni.

## 🚀 Käivitamine

1. Klooni repository.
2. Ava `PetOffice.slnx` Visual Studios.
3. Käivita `PetOffice.WpfApp`.

## 🧩 Klassid ja liidesed

Baasklass:

- `OfficePet`

Alamklassid:

- 🐱 `CatManager`
- 🐶 `DogIntern`
- 🐹 `HamsterIT`
- 🦜 `ParrotReceptionist`

Liidesed:

- 💬 `ICommunicate`
- ☕ `IMakeCoffee`
- 🛠️ `ITechSupport`

`DogIntern` kasutab kahte liidest: `ICommunicate` ja `IMakeCoffee`.

Kõiki objekte hoitakse ühises `ObservableCollection<OfficePet>` kollektsioonis.

## ⚡ Oleku reeglid ja CrazyAction

Igal loomal on nimi, energia ja lõpetatud ülesannete arv.

Energia algväärtus on 100. Tegevused vähendavad või taastavad energiat ning vigase tegevuse korral objekti olekut ei muudeta.

Igal alamklassil on oma `Work()` ja `CrazyAction()`:

- `CatManager` korraldab koosolekuid või puhkab, kui energiat on vähe.
- `DogIntern` proovib teha kõik ülesanded korraga.
- `HamsterIT` parandab serverit oma erilisel viisil.
- `ParrotReceptionist` kordab kontorivestlusi või puhkab, kui energiat on vähe.

## 🖥️ WPF kasutajaliides

Rakenduses saab:

- lisada, valida ja eemaldada kontoriloomi;
- kasutada `Work` ja `Crazy Action` tegevusi;
- kasutada liidestest sõltuvaid tegevusi;
- jälgida energiat ja lõpetatud ülesandeid;
- vaadata tegevuste logi.

Iseseisvalt õpitud WPF element on `ProgressBar`, mida kasutatakse valitud looma energia kuvamiseks. See sobib energia näitamiseks, sest väärtus muutub vahemikus 0–100.

## ✅ Kontrollitud kasutusjuhud

1. Õige nimega looma saab lisada nimekirja.
2. Tühja nimega looma ei lisata ja kuvatakse veateade.
3. `DogIntern` saab kasutada `Communicate` ja `Make coffee` tegevusi.
4. `HamsterIT` saab kasutada `Fix computer` tegevust.
5. `CrazyAction` muudab looma energiat või ülesannete arvu.
6. Ebapiisava energia korral kuvatakse veateade ja olek ei rikne.
7. Valitud looma saab eemaldada.
8. `ParrotReceptionist` saab töötada, suhelda ja kasutada oma `CrazyAction()` meetodit.

## 🤝 Git-koostöö

Kaasüliõpilane: **Anna Levchenko**

Anna lisas projekti uue `ParrotReceptionist` alamklassi eraldi branch'is.

Issue:  
https://github.com/arinashishkova/PetOffice/issues/1

Pull Request:  
https://github.com/arinashishkova/PetOffice/pull/2

Pull Request kontrolliti ja kiideti enne merge'i heaks.

## 🤖 AI kasutamine

Projekti tegemisel kasutati AI-d C# ja WPF-i kontseptsioonide ning GitHubi töövoo selgitamiseks, koodivigade leidmiseks ja parandamiseks ning README koostamise abistamiseks.

Lahendust kontrolliti Visual Studios ning rakenduse põhifunktsioone testiti käsitsi.

## 🛠️ Tehnoloogiad

- C#
- .NET 10
- WPF
- Git
- GitHub