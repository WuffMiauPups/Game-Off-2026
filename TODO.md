# TODO – Endspurt Game Off 2026

> **Stand:** 24.11.2026 – noch ca. 1 Woche bis zur Abgabe.
> **Hinweis:** Diese Punkte sind noch offen und müssen später von mir (WuffMiauPups) erledigt werden.

## 1. Zeitleiste bei den Antworten entfernen
- [ ] Die sichtbare Zeitleiste bei den Antworten soll **komplett weg**. Der **Herzschlag** ersetzt sie (Herzschlag ist schon eingebaut).
- Wo im Code:
  - `Game/Voices/Assets/Scripts/UI/VoiceOverlay.cs` → `timerFill`, `SetTimer()`, `SetTimerVisible()`
  - `Game/Voices/Assets/Scripts/Core/SessionController.cs` → Frage-Schleife um `overlay.SetTimerVisible(timed)` / `overlay.SetTimer(fraction)`
- Achtung:
  - Nur die **Anzeige** entfernen, nicht den Countdown selbst – `left` steuert `sounds.UpdateHeartbeat(left)` und das Timeout der Frage.
  - Ohne Herzschlag-Clip fällt das Spiel auf `sfx.Tick()` zurück → sicherstellen, dass der Herzschlag-Clip immer zugewiesen ist.
  - Ohne Leiste gibt es **keinen visuellen** Zeit-Hinweis mehr. Wer ohne Ton spielt, merkt den Zeitdruck nicht. Ggf. eine kleine visuelle Herzschlag-Andeutung (Puls / Vignette) überlegen.
  - Das Timer-Objekt in der Szene entfernen oder deaktivieren, damit kein leerer Rahmen übrig bleibt.

## 2. Eigene UI-Grafiken designen
- [ ] Textfelder unten (Dialoge der Ärzte)
- [ ] Textfelder für Beschreibungen usw.
- [ ] Das große **SHIFT**-Feld

## 3. Gesichter designen (4 Faces)
- [ ] Face 1
- [ ] Face 2
- [ ] Face 3
- [ ] Face 4
- Hinweis: In `Game/Voices/Assets/Art/Doctors/` liegen aktuell nur **3** Dateien (`headdoctor`, `olddoctor`, `youngdoctor`). Klären, welches das 4. Gesicht ist und wo es hingehört.

## 4. Tag 1–3 fertigstellen (Mindestumfang für die Abgabe)
- [ ] Tag 1 fertig machen (`Data/Sessions/day1.json`, `Resources/Ink/day1.ink`)
- [ ] Tag 2 erstellen (gibt es noch nicht)
- [ ] Tag 3 erstellen (gibt es noch nicht)

## 5. Leichte Animationen
- [ ] Augenbrauen hochziehen
- [ ] Atmen der Ärzte (Idle)

## 6. Rest
- [ ] Anpassungen / Changes
- [ ] Playtesting
- [ ] Export / Build
- [ ] Upload auf itch.io und Abgabe beim Jam

---

### Realistische Einschätzung
Für eine Woche ist das viel: 2 neue Tage schreiben, 4 Gesichter, UI-Grafiken **und** Animationen. Vorschlag zur Reihenfolge, falls die Zeit knapp wird:
1. Zeitleiste raus (klein, schnell erledigt)
2. Tag 1–3 spielbar (ohne Inhalt keine Abgabe)
3. Gesichter + UI-Grafiken
4. **Spätestens 1–2 Tage vor Abgabe:** Build exportieren und testen – auch wenn noch nicht alles fertig ist
5. Animationen zuletzt (sind „nice to have“ und am leichtesten zu streichen)
