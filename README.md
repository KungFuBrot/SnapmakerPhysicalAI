# SnapmakerPhysicalAI

**Physical AI am eigenen Schreibtisch:** Ein KI-Agent sieht über eine Kamera, was auf der Arbeitsfläche eines
Snapmaker 2.0 A350T liegt, plant daraus einen Job (lasern, fräsen, drucken) und führt ihn aus.
Dabei sitzt immer eine deterministische Sicherheitsschicht zwischen dem Agenten und der Maschine.

> Status: **Grundgerüst (M0)**. Kern, Sicherheitsschicht, Simulator und Tests stehen. Die Hardware-Anbindung folgt.
> Referenzprojekt, entwickelt in C# / .NET 10.

## Idee

Sprachmodelle können planen, aber sie dürfen keinen Laser direkt ansteuern. Das Projekt trennt deshalb strikt:

```
Kamera ──► Wahrnehmung ──► Agent (LLM) ──► MachineCommand ──► SafetyGuard ──► Maschine
             (Vision)       plant mit          (kleiner,         (Regeln,        (WLAN / USB /
                            Werkzeugen         geschlossener     kein Ermessen)   Simulator)
                                               Befehlssatz)
```

- **Geschlossener Befehlssatz:** Der Agent erzeugt keinen freien G-Code, sondern typisierte Befehle (`Home`, `MoveTo`, `LaserOn`, …). Nur was modelliert ist, lässt sich prüfen.
- **SafetyGuard:** Arbeitsraum je Werkzeugkopf, Vorschubgrenzen, Laser nur mit Laserkopf, bei geschlossenem Gehäuse und unter einer Leistungsgrenze. Abgelehnte Befehle erreichen die Maschine nie.
- **Simulator zuerst:** Jeder Agentenlauf geht erst gegen `SimulatedMachine`, das G-Code und Zustand protokolliert.

## Projektstruktur

| Pfad | Inhalt |
|---|---|
| `src/Snapmaker.Core` | Geometrie, Maschinenprofil, Befehle, `SafetyGuard`, `SafeMachine`, Simulator |
| `src/Snapmaker.Cli` | Demo: Quadrat lasern im Simulator, unzulässiger Befehl wird gestoppt |
| `tests/Snapmaker.Tests` | xUnit-Tests der Sicherheitsregeln |
| `docs/roadmap.md` | Meilensteine M0 bis M5 |

## Starten

```bash
dotnet test
```

```bash
dotnet run --project src/Snapmaker.Cli
```

## Zusammenspiel mit LlmHarness

Der Agent wird nicht mit einem fertigen Framework gebaut, sondern mit der eigenen Harness aus
[LlmHarness](https://github.com/KungFuBrot/LlmHarness). Die Maschinenbefehle werden dort als Werkzeuge registriert.
Freigaben durch den Menschen laufen über die Permission-Hooks der Harness.

## Sicherheit

Laser und Fräse können Menschen verletzen und Brände auslösen. Dieses Projekt ersetzt keine Aufsicht: Echte Jobs nur mit Schutzbrille,
geschlossenem Gehäuse und in Reichweite des Not-Aus.
