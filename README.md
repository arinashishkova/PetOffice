# PetOffice

PetOffice on WPF-rakendus, kus erinevad kontoriloomad täidavad tööülesandeid ja kasutavad oma erivõimeid.

Projekt demonstreerib objektorienteeritud programmeerimise põhimõtteid:
- pärilus
- polümorfism
- abstraktne klass
- liidesed
- ühine objektide kollektsioon

## Tegelased

### CatManager
Kass-manager saab teha tööd, suhelda ja kasutada oma eritegevust.

### DogIntern
Koer-praktikant saab teha tööd, suhelda, teha kohvi ja kasutada oma eritegevust.

### HamsterIT
IT-hamster saab teha tööd, parandada arvuteid ja kasutada oma eritegevust.

## Funktsioonid

Rakenduses saab:
- lisada uusi kontoriloomi
- valida looma nimekirjast
- täita tööülesandeid
- kasutada looma erivõimeid
- jälgida energiataset
- jälgida lõpetatud ülesannete arvu
- vaadata tegevuste logi
- eemaldada looma nimekirjast

## Projekti struktuur

### PetOffice.Core

Sisaldab rakenduse põhiloogikat:

- `OfficePet`
- `CatManager`
- `DogIntern`
- `HamsterIT`
- `ICommunicate`
- `IMakeCoffee`
- `ITechSupport`

### PetOffice.WpfApp

Sisaldab WPF kasutajaliidest ja kasutaja tegevuste töötlemist.

## Kasutatud tehnoloogiad

- C#
- .NET 10
- WPF