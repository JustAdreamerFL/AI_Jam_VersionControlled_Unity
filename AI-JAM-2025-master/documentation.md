# Dokumentácia k projektu AI JAM
**verzia:** 1.0.0 (November 2025)  
Fakulta elektrotechniky a informačných technológií,
Žilinská univerzita v Žiline  
[www.feitcity.sk/aijam](https://www.feitcity.sk/aijam/)

## Prehľad obsahu
Táto dokumentácia je podrobným prehľadom k projektu AI JAM.
Obsahuje návody a popis k rôznym častiam projektu vrátane
tvorby a tréningu agentov, dizajnu robotov, organizácie súbojov
a ich pravidiel.

### Projekty
- [Projekt pre tvorbu a tréning (UNITY)](#unityProject)
	- [ConstructionScene: Zostavenie robota](#robotConstructionScene)
	- [ArenaScene: Zostavenie arény](#arenaConstructionScene)
	- [TrainingScene: Tréning a testovanie](#trainingTestingScene)
- [Projekt pre dizajn (BLENDER)](#robotDesignProject)
- [Projekt pre súboje (UNITY)](#tournamentProject)
- [VS Code pracovné prostredie](#vscodeSetup)

<hr/>

## VS Code pracovné prostredie <a name="vscodeSetup"></a>
Práca na sieťovom disku `/Volumes/T7` spôsobovala zlyhávanie watcherov
(`ENOSPC`) a opakované upozornenia rozšírení (Codeium, CodeTime, Copilot SWE agent).
Preto bola pridaná konfigurácia v súbore `.vscode/settings.json`, ktorá
obmedzuje sledovanie veľkých priečinkov Unity a prepína VS Code na
`files.useExperimentalFileWatcher`. Zároveň sú vypnuté automatické git
operácie (`git.autorefresh`, `git.autofetch`), čo eliminuje zbytočné dotazy na
`git rev-parse` pri práci offline.

Rozšírenia, ktoré vyžadujú navrhované API VS Code, zostanú vypnuté, pokiaľ
nepoužijete VS Code Insiders so spustením `code-insiders --enable-proposed-api`.
Ak Insiders nepoužívate, odporúčame tieto rozšírenia v prostredí VS Code
zakázať, aby sa logy nezapĺňali chybami.

Git repozitár bol overený (`git rev-parse HEAD`, `git remote -v`) a smeruje na
`https://github.com/LuLa-90/AI-JAM-2025.git`. Ak by sa podobné chyby objavili
v budúcnosti, spustite `git rev-parse --is-inside-work-tree` z koreňa otvoreného
workspace a skontrolujte, či VS Code nesleduje nadradený priečinok bez `.git/`.

<hr/>

## Projekt pre tvorbu a tréning (UNITY) <a name="unityProject"></a>
Tento projekt slúži na zostavenie a tréning robotov
a na tvorbu vlastných arén. Celý projekt obsahuje niekoľko
priečinkov s assetami, ktoré sú potrebné pri tvorbe vlastného
robota/arény a pri tréningu agentov:
- [Scény](#scenesAssets)
- [Prefaby](#prefabsAssets)
- [Skripty](#scriptsAssets)
- [Ostatné](#otherAssets)

### Rozloženie okien
![Window Layout](img_UnityLayout.png)

### Scény <a name="scenesAssets"></a>
Scény sú oddelené prostredia, ktoré vám v projekte AI JAM budú
slúžiť na rôzne účely. V projekte sa nachádzajú 3 hlavné scény;
ConstructionScene, ArenaScene a TrainingScene.

### ConstructionScene: Zostavenie robota <a name="robotConstructionScene"></a>
Vašou hlavnou úlohou v AI JAM je zostaviť si vlastného robota do súboja
v aréne, buď z predpripravených komponentov (prefabov) alebo si môžete
vytvoriť vlastné komponenty v programe Blender, k čomu sa však viažu isté
pravidlá (pozri: [Pravidlá tvorby vlastných prefabov](#ownPrefabRules)).

Keďže robota bude ovládať neurónová sieť natrénovaná strojovým učením,
konkrétne [reinforcement learning](https://docs.unity3d.com/Packages/com.unity.ml-agents%403.0/manual/index.html),
okrem vizuálneho zostavenia robota je dôležitým krokom aj nastavenie
senzorov, ktoré budú robotovi slúžiť na vnímanie prostredia počas
tréningu a súbojov. Má to však malý háčik - počet senzorov pre robota
je obmedzený, preto je potrebné si dobre premyslieť, ktoré senzory
budú pre stratégiu vášho robota skutočne nevyhnutné.

V prvom rade si prejdeme, ako vyzerá hierarchia predpripravenej
scény, a čo je potrebné nastaviť, aby bol robot funkčný.

#### Hierarchy
- [Reward Editor](#RCrewardEditorGO)
- [Main Camera](#RCmainCameraGO)
- [Environment](#RCenvironmentGO)
	- [Arena](#RCarenaGO)
	- [Robot](#RCrobotGO)

#### Reward Editor GameObject <a name="RCrewardEditorGO"></a>
Tento GameObject slúži na nastavovanie koeficientov odmien pre agenta počas
tréningu. Nachádza sa síce aj v tejto scéne pre účely prvotných testov,
no podrobnejšie si jeho funkciu popíšeme v časti venovanej tréningu agentov
(pozri: [TrainingScene: Tréning a testovanie](#rewardEditorTraining)).

#### Main Camera GameObject <a name="RCmainCameraGO"></a>
V projekte AI JAM pracujeme na princípe niekoľkých typov kamier,
pre ktoré sú viditeľné iba zvolené vrstvy. Pre hlavnú kameru môžeme
vybrať režim určený na zostavovanie - pre kameru je viditeľné všetko,
čo sa nachádza v scéne, alebo druhý režim, vďaka ktorému vieme overiť,
ako sa bude náš robot vykresľovať počas súbojov.

Tretí režim kamery je určený ako senzor pre samotného agenta -
ten si popíšeme v časti venovanej hierarchii robota (pozri: [Robot GameObject](#RCrobotGO)).