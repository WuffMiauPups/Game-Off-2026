// Scene 1 – first playable. Written for the Voices prototype.
// Conventions read by SessionController:
//   "Name: text"        -> speaker + line (playerthoughts / description = narration)
//   "* [Voice: text]"   -> an answer spoken by that voice (cycle with Shift)
//   # event: fade_in | doctors_enter
//   # input: name       -> player types a name (TODO in Unity)

VAR player_name = "Patient"

-> start

=== start ===
# scene: you wake up inside a padded cell...
# event: fade_in

playerthoughts: where am I? how did I get here?

description: there is a gray table in front of me with a bottle of pills... im strapped to my chair *rumbles chair* by these leather restraints. A door to my right. Behind the small gated window i can see 2 guards...
(door opens. 3 People walk in)
# event: doctors_enter

Doctor 1: Ah, you are awake. the sedative didnt work as long as we hoped for.

* [Paranoid: What? Why am I here? why am i getting sedated?]
    -> Answer1Scene1
* [Aggressive: Let me go! why am I here? i will rip your head off]
    -> Answer1Scene1
* [Childlike: please! this is a mistake! im not supposed to be here!]
    -> Answer1Scene1
* [Yes-Man: Yes right... a sedative for me. why again did i get that?]
    -> Answer1Scene1
* [Empathic: of course, people like me need sedatives!]
    -> Answer1Scene1
* [Asshole: injecting me with this should be a felony]
    -> Answer1Scene1

=== Answer1Scene1 ===

Doctor 1: Well you are still under its effects, so your current answer is not yet noted.
Doctor 2: It should be, (Doctor 1), the effects do not hide the truth.
Doctor 3: It can interfere with the areas of the brain that are used when telling the truth. I suggest its not brought into the report.

Doctor 1: Well, Patient, can you state your name?

# input: name

Doctor 2: And you confirm that is your real name?

-> Question2Scene1

=== Question2Scene1 ===

* [Paranoid: No. Why should i tell you my real name?]
    -> Answer2Scene1
* [Aggressive: Shut up.]
    -> Answer2Scene1
* [Childlike: Yes! im not lying! why should i lie?]
    -> Answer2Scene1
* [Yes-Man: Yes! Yes! That is my name!]
    -> Answer2Scene1
* [Empathic: why? is it important? don't you have my name already?]
    -> Answer2Scene1
* [Asshole: got your ears blocked by shit?]
    -> Answer2Scene1

=== Answer2Scene1 ===

Doctor 3: Well, EVERY Answer you state has an Impact on how we different Doctors view you personally. We all can then influence reports. This leads to different outcomes of this procedure here with you.
Doctor 2: And don't play smart on us! We are not allowed to show reactions (looks at Doctor 3) although some of us might struggle depending on the answer...

Doctor 1: It is crucial that you comply. We are here to help you. And not answering is the mostly the worst you can do.

Doctor 1: Well, i guess that's it for today. Thank you for coorporating. Tomorrow we will begin the Reporting, aswell as the Medication depending on your answers.

Doctor 2 (intervenes, and reads of the police report): We will find out why you listened to the "Voice" and what it told you.

Doctor 3: until then, the guards will bring you into your living cell. See ya.

-> END
