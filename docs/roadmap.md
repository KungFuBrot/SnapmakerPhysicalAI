# Roadmap

## M0 – Grundgerüst ✅
- Typisierte Befehle mit G-Code-Erzeugung
- `SafetyGuard` mit Arbeitsraum, Vorschub und Laserregeln
- `SimulatedMachine` und `SafeMachine`, Tests, CLI-Demo

## M1 – Echte Verbindung
- `SnapmakerHttpConnection`: Der Snapmaker 2.0 bietet im WLAN eine HTTP-Schnittstelle (die Luban auch nutzt): Verbindung mit Bestätigung am Touchscreen, Status abfragen, G-Code ausführen, Dateien hochladen. Endpunkte und Ablauf erst an der eigenen Maschine mitschneiden und dokumentieren, nicht raten.
- Alternativ `SerialConnection` über USB (Marlin-G-Code).
- Zustand (Position, Werkzeugkopf, Gehäusetür) aus der Maschine lesen statt annehmen.
- Not-Aus-Befehl, der die Sicherheitsschicht umgeht und immer erlaubt ist.

## M2 – Wahrnehmung
- USB-Kamera über dem Arbeitsbereich (OpenCvSharp).
- Kalibrierung: Kamerapixel ↔ Maschinenkoordinaten über ArUco-Marker.
- Werkstück erkennen: Umriss, Lage, Größe.

## M3 – Agent
- Agent auf Basis von [LlmHarness](https://github.com/KungFuBrot/LlmHarness).
- Werkzeuge: `get_state`, `capture_image`, `plan_job`, `simulate_job`, `execute_job` (nur nach Freigabe).
- Bildeingabe direkt an ein multimodales Modell, Ausgabe als strukturierter Jobplan.

## M4 – Geschlossener Kreis
- Nach dem Job ein Foto machen und mit dem Plan vergleichen (Soll/Ist).
- Abweichungen ins Protokoll schreiben und daraus Kalibrierung nachführen.

## M5 – Vorzeigen
- Kurzvideo und Architektur-Schaubild in der README.
- Evaluationsaufgaben (z. B. "Graviere den Namen mittig auf das Brett") mit Erfolgsquote.
