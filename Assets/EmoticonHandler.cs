using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmoticonHandler
{
    private readonly string _emoticonPrefix = "<sprite name=\""; // <sprite name="
    private readonly string _emoticonSuffix = "\">"; // ">
    private readonly List<string> _emoticonList = new List<string>()
    {
        ":(",
        ":D",
        ":DD",
        "=D",
        ";D",
        ";.D",
        ":)",
        "-_-",
        ";)",
        "<3",
        ":o",
        "=]",
        ":P"
    };

    public string ConvertString(string input)
    {
        var replacedEmoticons = input;

        foreach (var emoticon in _emoticonList)
        {
            // expected input :)
            // expected result
            // <sprite name="emoticonhere">
            // replace if found
            replacedEmoticons = replacedEmoticons.Replace(emoticon, _emoticonPrefix + emoticon + _emoticonSuffix);
        }

        return replacedEmoticons;
    }
}
