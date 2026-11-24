// Zum Testen in dieser Datei. Später nur EINMAL im ganzen Spiel (z. B. in IntroScene).
VAR player_name = "Patient"
VAR report_head = 0
VAR report_old = 0
VAR report_young = 0
VAR clues = 0

-> Day1

=== Day1 ===

description: Same room as yesterday.
playerthoughts: Same room, but what is that smell? Something's off... 
playerthoughts: I need to know why I even am here. I don't know anything!

Head Doctor: Welcome {player_name}. This is the first consultation. 

-> QuestionDay1_1

=== QuestionDay1_1 ===

Head Doctor: Lets begin. do you recall anything from october 1995?

# sfx_timer: pentap

(Choice)
* [Paranoid: No, I dont know anything.]
    ~ report_head += 0
    ~ report_old += 0
    ~ report_young += 0
    -> AnswerDay1_1
* [Narcissistic: What makes you think I would tell you?]
    ~ report_head += 0
    ~ report_old -= 1
    OldDoctor: don't play smart on us.
    ~ report_young += 1
    -> AnswerDay1_1
* [Anxious: No... please i don't know anything!]
    ~ report_head -= 1
    ~ report_old += 1
    ~ report_young += 0
    -> AnswerDay1_1
* [Yes-Man: Yes! I... uh know... Sorry, I think I forgot.]
    ~ report_head += 1
    ~ report_old -= 1
    ~ report_young += 1
    Young Doctor: try to focus on your last memories...
    -> AnswerDay1_1

=== AnswerDay1_1 ===

# sfx: writing

-> QuestionDay1_2

=== QuestionDay1_2 ===

OldDoctor: On that fateful day, there was a party from one of your classmates, was that a friend of yours?

# sfx_timer: chair

(Choice)
* [Paranoid: No. Not a friend at all.]
    ~ report_head -= 1
    ~ report_old -= 1
    ~ report_young -= 1
    HeadDoctor: Well, because in your testimony, you called Thomas your "friend".
    -> AnswerDay1_2
* [Narcissistic: are you sniffing around in my private life?]
    ~ report_head += 0
    ~ report_old -= 1
    ~ report_young += 1
    HeadDoctor: Well, because in your testimony, you called Thomas your "friend".
    -> AnswerDay1_2
* [Anxious: I don't know whats going on. you need to clarify!]
    ~ report_head += 0
    ~ report_old += 0
    ~ report_young += 1
    HeadDoctor: Well, because in your testimony, you called Thomas your "friend".
    -> AnswerDay1_2
* [Yes-Man: Yes! I was at my friend's house]
    ~ report_head += 1
    ~ report_old += 1
    ~ report_young += 1
    -> AnswerDay1_2

=== AnswerDay1_2 ===

OldDoctor: And you went to his parents house. According to your testimony it was a 2 story light blue house.

-> QuestionDay1_3

=== QuestionDay1_3 ===

YoungDoctor: In your testimony, you called Chloe your date for the night. Is that correct?

(Choice)
* [Paranoid: Nope, just a classmate. Not a date]
    ~ report_head += 1
    ~ report_old += 0
    ~ report_young += 1
    -> AnswerDay1_3
* [Narcissistic: Why? jealous? we had something going on]
    ~ report_head += 0
    ~ report_old -= 1
    ~ report_young += 2
    -> AnswerDay1_3
* [Anxious: I... I don't know! i guess?]
    ~ report_head += 0
    ~ report_old -= 1
    ~ report_young += 1
    -> AnswerDay1_3
* [Yes-Man: Yes! my Date! Chloe was my Date that night.]
    ~ report_head -= 1
    ~ report_old += 1
    ~ report_young -= 1
    -> AnswerDay1_3

=== AnswerDay1_3 ===

-> EndDay1

=== EndDay1 ===

HeadDoctor: Sample goodbye for today.
description: Sample description, guards bring me back to my cell.

// Average of the three reports. Below -2: confined for all time.
{ (report_head + report_old + report_young) / 3.0 < -2: -> BadBranchDayX }

-> END

=== BadBranchDayX ===

# game_over
HeadDoctor: Sample line for a bad report.

-> END