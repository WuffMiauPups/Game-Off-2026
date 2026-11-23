// Intro day. Text taken from IntroScene.ink in Ink Unterwegs (20.11.).
// Added for Unity: VAR player_name, "# event: fade_in", "# timer: off", "# input: name".
// Conventions read by SessionController:
//   "Name: text"        -> speaker + line (playerthoughts / description = narration)
//   "* [Voice: text]"   -> an answer spoken by that voice (cycle with Shift)
//   "(Choice)"          -> author's marker, not shown in the game
//   # input: name       -> after this line the player types a name, use it as {player_name}

VAR player_name = "Patient"

-> start

=== start ===

# scene: you wake up inside a padded cell...
# event: fade_in
# timer: off

playerthoughts: where am I? how did I get here?

description: there is a gray table in front of me with a bottle of pills. im strapped to a chair by leather belts.
description: A door to my right. Behind the small gated window i can see 2 guards...

HeadDoctor: Ah, you are awake. the sedative didnt work as long as we hoped for.

(Choice)
* [Paranoid: What? Why am I here? why am i getting sedated?]
    -> Answer1Scene1
* [Narcissistic: injecting me with this should be a felony]
    -> Answer1Scene1
* [Anxious: please! this is a mistake! im not supposed to be here!]
    -> Answer1Scene1
* [Yes-Man: Yes right... a sedative for me. why again did i get that?]
    -> Answer1Scene1

=== Answer1Scene1 ===

HeadDoctor: Well you are still under its effects, so your current answer is not yet noted.
OldDoctor: It should be, (Doctor 1), the effects do not hide the truth.
YoungDoctor: It can interfere with the areas of the brain that are used when telling the truth. I suggest its not brought into the report.

HeadDoctor: Well, Patient, can you state your name? # input: name

OldDoctor: And you confirm that is your real name?

-> Question2Scene1

=== Question2Scene1 ===

(Choice)
* [Paranoid: No. Why should i tell you my real name?]
    -> Answer2Scene1
* [Narcissistic: got your ears blocked by shit?]
    -> Answer2Scene1
* [Anxious: Yes! im not lying! why should i lie?]
    -> Answer2Scene1
* [Yes-Man: Yes! Yes! That is my name!]
    -> Answer2Scene1

=== Answer2Scene1 ===

YoungDoctor: Well, EVERY Answer you state has an Impact on how we different Doctors view you personally. We all can then influence reports. This leads to different outcomes of this procedure here with you.
OldDoctor: And don't play smart on us! We are not allowed to show reactions (looks at Doctor3) although some of us might struggle depending on the answer...

HeadDoctor: It is crucial that you comply. We are here to help you. And not answering is the mostly the worst you can do.

HeadDoctor: Well, i guess that's it for today. Thank you for coorporating. Tomorrow we will begin the Reporting, aswell as the Medication depending on your answers.

OldDoctor: (intervenes, and reads of the police report): We will find out why you listened to the "Voice" and what it told you.

YoungDoctor: until then, the guards will bring you into your living cell. See ya.

-> END
