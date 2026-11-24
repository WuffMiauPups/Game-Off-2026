// Added for Unity: VAR player_name, "# event: fade_in", "# timer: off", "# input: name".
VAR player_name = "Patient"

-> start

=== start ===

# scene: you wake up inside a padded cell...
# event: fade_in
# timer: off

playerthoughts: where am I? how did I get here?

description: there is a gray table in front of me with a bottle of pills. im strapped to a chair by leather belts. 
description: A door to my right. Behind the small gated window i can see 2 guards...

Head Doctor: Ah, you are awake. the sedative didnt work as long as we hoped for. 

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

Head Doctor: Well you are still under its effects, so your current answer is not yet noted.
Old Doctor: It should be, (Doctor 1), the effects do not hide the truth.
Young Doctor: The medication can interfere with the prefrontal cortex. I suggest today is not brought into the report.

Head Doctor: Well, Patient, can you state your name? # input: name

Old Doctor: And you confirm that is your real name?

-> Question2Scene1

=== Question2Scene1 ===

(Choice)
* [Paranoid: No. Why should i tell you my real name?]
    -> Answer2Scene1
* [Narcissistic: got your ears blocked by shit?]
    -> Answer2Scene1
* [Anxious: Im not lying! really!]
    -> Answer2Scene1
* [Yes-Man: Yes! Yes! That is my name!]
    -> Answer2Scene1

=== Answer2Scene1 ===

Young Doctor: Well, EVERY Answer you state has an Impact on how we different Doctors view you personally. We all can then influence reports. This leads to different outcomes of this procedure here with you.
Old Doctor: And don't play smart on us! We are not allowed to show reactions, although some of us might struggle depending on the answer...

Head Doctor: It is crucial that you comply. We are here to help you. And not answering is the mostly the WORST you can do.

Head Doctor: Well, i guess that's it for today. Thank you for coorporating. Tomorrow we will begin the Reporting, aswell as the Medication depending on your answers. 

Old Doctor: (intervenes, and reads of the police report): We will find out why you listened to the "Voice" and what it told you.

Young Doctor: until then, the guards will bring you into your living cell. See ya.

-> END
