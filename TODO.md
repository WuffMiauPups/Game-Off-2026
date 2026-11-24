# TODO – Endspurt Game Off 2026

> **Stand:** 24.11.2026 – noch ca. 1 Woche bis zur Abgabe.
> **Hinweis:** Diese Punkte sind noch offen und müssen später von mir (WuffMiauPups) erledigt werden.

## 1. Zeitleiste bei den Antworten entfernen
- [ ] Die sichtbare Zeitleiste bei den Antworten soll **im finalen Spiel komplett weg**. Der **Herzschlag** ersetzt sie (Herzschlag ist schon eingebaut).
- **Bis dahin bleibt die Leiste als Debug-Hilfe drin.** ⚠️ Vor dem finalen Export unbedingt entfernen/deaktivieren!
- Wo im Code:
  - `Game/Voices/Assets/Scripts/UI/VoiceOverlay.cs` → `timerFill`, `SetTimer()`, `SetTimerVisible()`
  - `Game/Voices/Assets/Scripts/Core/SessionController.cs` → Frage-Schleife um `overlay.SetTimerVisible(timed)` / `overlay.SetTimer(fraction)`
- Achtung:
  - Nur die **Anzeige** entfernen, nicht den Countdown selbst – `left` steuert `sounds.UpdateHeartbeat(left)` und das Timeout der Frage.
  - Ohne Herzschlag-Clip fällt das Spiel auf `sfx.Tick()` zurück → sicherstellen, dass der Herzschlag-Clip immer zugewiesen ist.
  - Ohne Leiste gibt es **keinen visuellen** Zeit-Hinweis mehr. Wer ohne Ton spielt, merkt den Zeitdruck nicht. Ggf. eine kleine visuelle Herzschlag-Andeutung (Puls / Vignette) überlegen.
  - Das Timer-Objekt in der Szene entfernen oder deaktivieren, damit kein leerer Rahmen übrig bleibt.

## 2. Eigene UI-Grafiken designen (geht schnell)
- [ ] Textfelder unten (Dialoge der Ärzte)
- [ ] Textfelder für Beschreibungen usw.
- [ ] Das große **SHIFT**-Feld

## 3. Gesichter designen (4 Faces) – größter Aufwand
- [ ] Face 1
- [ ] Face 2
- [ ] Face 3
- [ ] Face 4
- Hinweis: In `Game/Voices/Assets/Art/Doctors/` liegen aktuell nur **3** Dateien (`headdoctor`, `olddoctor`, `youngdoctor`). Klären, welches das 4. Gesicht ist und wo es hingehört.

## 4. Tag 1–3 fertigstellen (Mindestumfang für die Abgabe)
- [ ] Tag 1 letzte Kleinigkeiten (fast fertig – `Data/Sessions/day1.json`, `Resources/Ink/day1.ink`)
- [ ] Tag 2 erstellen (gibt es noch nicht)
- [ ] Tag 3 erstellen (gibt es noch nicht)

## 5. Leichte Animationen (ganz am Ende)
- [ ] Augenbrauen hochziehen
- [ ] Atmen der Ärzte (Idle)

## 6. Rest
- [ ] Anpassungen / Changes
- [ ] Playtesting
- [ ] Export / Build
- [ ] **Zeitleiste vor dem finalen Build entfernen** (siehe Punkt 1)
- [ ] Fertigen Build selbst durchspielen und testen
- [ ] Upload auf itch.io und Abgabe beim Jam

---

### Reihenfolge / Plan
1. Tag 2 + 3 schreiben und spielbar machen (Tag 1 ist fast fertig)
2. Gesichter designen – dauert am längsten, deshalb früh anfangen
3. Textfelder + SHIFT-Feld (schnell)
4. **Spätestens 1–2 Tage vor Abgabe:** finalen Build exportieren und testen – dabei Zeitleiste entfernen!
5. Animationen ganz am Ende (am leichtesten zu streichen, falls die Zeit knapp wird)
