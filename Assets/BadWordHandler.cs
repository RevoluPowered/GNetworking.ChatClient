// LICENSE
// GNetworking, SimpleUnityClient and GameServer are property of Gordon Alexander MacPherson
// No warantee is provided with this code, and no liability shall be granted under any circumstances.
// All rights reserved GORDONITE LTD 2018 � Gordon Alexander MacPherson.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets
{
    public class BadWordHandler
    {
        public BadWordHandler()
        {

        }

        /// <summary>
        /// List of bad words.
        /// </summary>
        private readonly List<string> _badWords = new List<string>()
        {
            "anal",
            "anus",
            "arse",
            "ass",
            "ballsack",
            "balls",
            "bastard",
            "bitch",
            "biatch",
            "bloody",
            "blowjob",
            "blow job",
            "bollock",
            "bollok",
            "boner",
            "boob",
            "bugger",
            "bum",
            "butt",
            "buttplug",
            "clitoris",
            "cock",
            "coon",
            "crap",
            "cunt",
            "damn",
            "dick",
            "dildo",
            "dyke",
            "fag",
            "feck",
            "fellate",
            "fellatio",
            "felching",
            "fuck",
            "f u c k",
            "fudgepacker",
            "fudge packer",
            "flange",
            "Goddamn",
            "God damn",
            "jerk",
            "jizz",
            "knobend",
            "knob end",
            "labia",
            "lmao",
            "lmfao",
            "muff",
            "nigger",
            "nigga",
            "omg",
            "penis",
            "piss",
            "poop",
            "prick",
            "pube",
            "pussy",
            "queer",
            "scrotum",
            "sex",
            "shit",
            "s hit",
            "sh1t",
            "slut",
            "smegma",
            "spunk",
            "tit",
            "tosser",
            "turd",
            "twat",
            "vagina",
            "wank",
            "whore",
            "wtf"
        };

        /// <summary>
        /// Filter string of any bad words we have in our list
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string Filter(string input)
        {
            var output = input;
            foreach (string word in _badWords)
            {
                output = output.Replace(word, "*censored*");
            }

            return output;
        }

    }

}