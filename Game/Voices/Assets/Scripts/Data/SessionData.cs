using System;

namespace Voices.Data
{
    [Serializable]
    public class SessionData
    {
        public string id;
        public Step[] steps;
    }

    // JsonUtility has no polymorphism, so every step type shares one flat class.
    // type: "line" | "question" | "event"
    [Serializable]
    public class Step
    {
        public string type;
        public string speaker;
        public string text;
        public string eventName;
        public float time;
        public Answer[] answers;
        public string timeoutReaction;
    }

    [Serializable]
    public class Answer
    {
        public string voice;
        public string text;
        public string reaction;
    }

    [Serializable]
    public class VoiceList
    {
        public VoiceDef[] voices;
    }

    [Serializable]
    public class VoiceDef
    {
        public string id;
        public string name;
    }
}
