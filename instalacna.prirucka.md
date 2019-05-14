# Inštalačná príručka

## Minimálne požiadavky

Pre použitie komponentu RessurectIT.Msi.Installer a jeho inštaláciu je potrebné spĺňať nasledovné požiadavky.

- nainštalovaný .Net Framework 4.6.1
- nainštalovaný Windows Installer 4.5 - https://support.microsoft.com/sk-sk/kb/942288

## Inštalácia

Inštalácia vytvorí nasledovné súbory, adresáre a nastavenia:

### Inštalácia - Bez parametrov

V prípade, že sa spustí RessurectIT.Msi.Installer-*.msi priamo z *Windows Explorer*, tak sa vykoná inštalácia s predvolenými hodnotami (bez možnosti ich zmeny). 
Inštalátor vyžaduje administrátorské oprávnenia pre jeho správny beh.
Inštalácia, alebo aktualizácia aplikácie bez parametrov vždy nastaví konfiguráciu na predvolené hodnoty aj napriek tomu, že v predchádzajúcej verzii mohli byť nastavené na iné hodnoty !!!
 - Inštalačná cesta aplikácie = C:\RessurectIT\RessurectIT.Msi.Installer
   - obsahuje dll, exe a konfiguračné súbory potrebné k behu aplikácie
 - Cesta ku konfiguračnému súboru = C:\RessurectIT\RessurectIT.Msi.Installer\RessurectIT.Msi.Installer.config.json
   - na úpravu tohto súboru sú potrebné administrátorské oprávnenia
 - Cesta k log súborom = C:\RessurectIT\RessurectIT.Msi.Installer\Log
   - na úpravu týchto súborov sú potrebné administrátorské oprávnenia
 - adresa pre získanie *json* so zoznamom aktualizácii - http://localhost/updates.json
 - časový interval (v ms) v akom kontrolovať dostupnosť nových aktualizácii - 300000
 - adresa pre vzdialené logovanie logov - http://localhost/api/log

### Inštalácia - S parametrami

V prípade, že sa spustí inštalácia z príkazového riadku pomocou msiexec 
(napríklad: msiexec /i RessurectIT.Msi.Installer-*.msi /q MENO_PARAMETRA=HODNOTA_PARAMETRA MENO_PARAMETRA2=HODNOTA_PARAMETRA2)

Parametre nie sú povinné (ak nie je napísané inak). Ak sa parameter nenastaví, tak sa použije predvolená hodnota pre daný parameter, ktorá je uvedená v časti "Inštalácia - Bez parametrov".

Zoznam parametrov:
 - INSTALLFOLDER - Cesta kam sa má nainštalovať aplikácia (príklad INSTALLFOLDER="C:\YourApp")
 - UPDATE_JSON_URL - Url adresa, na ktorej je možné pomocou GET získať JSON s dostupnými aktualizáciami (príklad UPDATE_JSON_URL=http://intranet/updates)
 - CHECK_INTERVAL - časový interval (v ms) v akom kontrolovať dostupnosť nových aktualizácii(príklad CHECK_INTERVAL=900000)
 - REMOTE_LOG_REST_URL - adresa pre vzdialené logovanie logov pomocou POST (príklad REMOTE_LOG_REST_URL=http://intranet/api/logs)
 - SERVICEACCOUNT - POVINNÝ parameter, názov účtu, pod ktorým bude služba spustená, musí mať Administrátorské oprávnenia a musí byť absolútny (príklad pre lokálne účty ".\UCET")
 - SERVICEPASSWORD - POVINNÝ parameter, heslo k účtu, pod ktorým bude služba spustená

### Inštalácia cez Group Policy

Inštalácia cez Group Policy umožňuje distribuovanú inštaláciu. Základná inštalácia cez Group Policy je vysvetlená tu http://www.advancedinstaller.com/user-guide/tutorial-gpo.html
Pokiaľ je potrebné inštalovať aplikáciu cez Group Policy a nastavovať parametre, je potrebné použiť transformácie definované v MST súbore, alebo
pomocou ORCA nástroja upraviť MSI súbor aby vyhovoval požiadavkam. Viac informácií tu http://windowsitpro.com/windows/q-can-i-specify-switches-msi-files-deployed-using-group-policy

Ideálne je vytvoriť mst súbor s transformáciami. Má to výhodu, že v prípade vydania novej verzie msi, nie je potrebné nič robiť iba zmeniť cestu k novému msi.

#### Inštalácia cez Group Policy s úpravou MSI

Treba nainštalovať orca nástroj. Návod je tu https://blogs.technet.microsoft.com/bernhard_frank/2010/03/15/how-to-install-orca-exe-from-the-windows-installer-development-tools/

1. Otvoriť RessurectIT.Msi.Installer-*.msi v Orca.exe
2. V ľavej časti obrazovky v Tables nájsť Property a klinúť na to
   1. Pridať nové záznamy pre parametre, ktoré chceme zmeniť/nastaviť (INSTALLFOLDER, SRV_URL)
3. Následne uložiť zmeny, kliknúť v menu na File -> Save
4. Použiť zmenené msi pre inštaláciu

#### Inštalácia cez Group Policy s využitím mst

Treba nainštalovať orca nástroj. Návod je tu https://blogs.technet.microsoft.com/bernhard_frank/2010/03/15/how-to-install-orca-exe-from-the-windows-installer-development-tools/

1. Otvoriť RessurectIT.Msi.Installer-*.msi v Orca.exe
2. Kliknúť v menu na Transform -> New Transform
3. V ľavej časti obrazovky v Tables nájsť Property a klinúť na to
   1. Pridať nové záznamy pre parametre, ktoré chceme zmeniť/nastaviť (INSTALLFOLDER, SRV_URL)
4. V menu kliknúť na Transform -> Generate Transform
5. Pomenovať súbor MST
6. Použiť súbor mst pre transformáciu msi
 - priamo z príkazového riadku msiexec /I RessurectIT.Msi.Installer-*.msi /q nazovsuboru.mst
 - alebo použiť ako modifications pri vytváraní group policy

V prípade potreby nastaviť inštalačnú cestu na 64 bitovom operačnom systéme na "C:\Program Files" namiesto "C:\Program Files (x86)" je potrebné nastaviť cestu na "C:\Progra~1" pretože "C:\Program Files"
fungovať nebude. Toto je však riskantné, pretože funguje len pokiaľ Microsoft nezruší podporu pre 8.3 dlhé názvy.

## Typy log súborov

AppDátum.logs - Obsahuje aplikačné logy

## Známe chyby pri inštalácii

Windows msi: error 1316: the specified account already exists
 - niekedy je potrebné aby bol .msi súbor s inštaláciou premenovaný vždy na rovnaké meno, napríklad RessurectIT.Msi.Installer.msi, oficiálne by to nemal byť problém, ale sem tam sa to stáva, že pri aktualizácii aplikácie keď sa inštalačné súbory volajú rôzne (obsahujú číslo verzie) tak inštalátor vyhodí túto chybu, premenovanie vždy na jeden názov by malo pomôcť