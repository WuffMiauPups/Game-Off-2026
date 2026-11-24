# UI-Maße (Stand 24.11.2026)

Alle Maße in Spielpixeln, gleiches Raster wie das Raumbild (320 × 180).
Position = linke obere Ecke, gemessen von der linken oberen Ecke des Bildes.
Im Spiel wird alles ganzzahlig vergrößert (6-fach bei 1080p), zusammen mit dem Raum.

## Spiel

| Feld | Größe | Position | Dateiname (Vorschlag) |
|---|---|---|---|
| Textbox unten | 296 × 37 | 12, 136 | `ui_textbox.png` |
| Gesichts-Box (in der Textbox) | 28 × 35 | 13, 137 | `ui_faceframe.png` |
| Stimmen-Overlay | 250 × 94 | 35, 28 | `ui_voicepanel.png` |
| Stimmen-Gesicht (im Overlay) | 80 × 80 | 42, 35 | `face_<stimme>.png` |
| Timer-Balken (im Overlay) | 147 × 2 | 130, 113 | `ui_timer.png` |
| Namenseingabe | 128 × 28 | 96, 66 | `ui_namebox.png` |
| Tages-Karte ("DAY 1") | 200 × 40 | 60, 70 | nur Text |

Arzt-Gesichter: höchstens 28 × 35, damit sie in die Gesichts-Box passen. Aktuell 21 × 29, 24 × 32, 20 × 27.

### Wo der Text sitzt

| Text | Bereich | Position |
|---|---|---|
| Sprechername (Textbox) | 283 × 8 | 19, 138 |
| Gesprochener Text (Textbox) | 283 × 25 | 19, 146 |
| Mit Gesicht rückt beides nach rechts auf | 258 breit | x 44 |
| "[Space]"-Hinweis | 33 × 6 | 270, 164 |
| Name der Stimme (Overlay) | 147 × 8 | 130, 36 |
| Antwort der Stimme (Overlay) | 147 × 57 | 130, 48 |
| Shift-Hinweis (Overlay) | 147 × 7 | 130, 106 |

## Hauptmenü

| Feld | Größe | Position |
|---|---|---|
| Titel | 234 × 30 | 43, 38 |
| Start | 60 × 14 | 130, 86 |
| Options | 60 × 14 | 130, 103 |
| Quit | 60 × 14 | 130, 120 |
| Content-Warnung | 268 × 10 | 26, 163 |
| Options: Regler Music | 94 × 8 | 132, 89 |
| Options: Regler Sounds | 94 × 8 | 132, 103 |
| Options: Regler-Schiene | 94 × 2 | mittig im Regler |
| Options: Regler-Griff | 4 × 8 | wandert auf der Schiene |
| Options: Back | 60 × 14 | 130, 120 |

## Schriften

| Schrift | Wo | Größe |
|---|---|---|
| Press Start 2P | Sprechername, Name der Stimme | 8 Pixel hoch, 8 Pixel pro Buchstabe |
| Press Start 2P | Titel, "DAY 1" | 24 Pixel hoch (3-fach) |
| Pixel Operator | Text, Hinweise, Content-Warnung, Regler-Namen | Zeile 8 Pixel hoch (halbe Spielpixel) |
| Pixel Operator | Antworten der Stimmen, Menü-Knöpfe, Namensfeld | Zeile 16 Pixel hoch (ganze Spielpixel) |

Textbox: höchstens 3 Zeilen. Antwort der Stimme: höchstens 3 Zeilen.
