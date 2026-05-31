# Folium

Alternative open source et 100% locale à ilovePDF. Manipulez vos PDFs et images sans qu'aucune donnée ne quitte votre machine.

![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-blue)
![License](https://img.shields.io/badge/license-AGPL%20v3-green)
![.NET](https://img.shields.io/badge/.NET-10-purple)

---

## Fonctionnalités

### PDF
- **Fusionner** — combiner plusieurs fichiers PDF en un seul
- **Diviser** — extraire des pages ou des plages de pages
- **Compresser** — réduire la taille en recompressant les images embarquées
- **Pivoter** — pivoter des pages à 90°, 180° ou 270°

### Images
- **Convertir** — JPG, PNG, WebP, AVIF, TIFF
- **Redimensionner** — redimensionner en conservant le ratio d'aspect

---

## Téléchargement

Rendez-vous sur la page [Releases](../../releases) et téléchargez l'archive correspondant à votre système :

| Plateforme | Fichier |
|---|---|
| Windows 64-bit | `Folium-vX.X.X-win-x64.zip` |
| Linux 64-bit | `Folium-vX.X.X-linux-x64.tar.gz` |
| macOS Intel | `Folium-vX.X.X-osx-x64.tar.gz` |
| macOS Apple Silicon | `Folium-vX.X.X-osx-arm64.tar.gz` |

Aucune installation requise — extrayez et lancez.

> **macOS** : si Gatekeeper bloque le lancement, faites clic droit → Ouvrir.  
> **Linux** : rendez le fichier exécutable si nécessaire : `chmod +x Folium.Desktop`

---

## Stack technique

| Composant | Technologie |
|---|---|
| Runtime | .NET 10 |
| UI | Avalonia 12 |
| Pattern UI | MVVM + CommunityToolkit.Mvvm |
| PDF | iText7 9.6 |
| Images | Magick.NET-Q16-AnyCPU 14 |

---

## Développement

### Prérequis

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Lancer en local

```bash
git clone https://github.com/Jordan-182/folium.git
cd folium
dotnet run --project Folium.Desktop
```

### Build

```bash
dotnet build Folium.slnx
```

### Publier un binaire autonome

```bash
dotnet publish Folium.Desktop/Folium.Desktop.csproj \
  -c Release \
  -r win-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true \
  -o publish/
```

Remplacer `win-x64` par `linux-x64`, `osx-x64` ou `osx-arm64` selon la cible.

---

## Architecture

```
Folium.Core/      ← Logique métier pure (services PDF et image)
Folium.Desktop/   ← Interface Avalonia (MVVM)
```

`Core` ne dépend d'aucune bibliothèque UI. `Desktop` consomme `Core` via des interfaces injectées.

---

## Licence

Distribué sous licence **AGPL v3** — voir le fichier [LICENSE](LICENSE).

Ce projet utilise [iText7](https://itextpdf.com/) sous licence AGPL v3.
