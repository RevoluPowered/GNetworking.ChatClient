﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmoticonHandler
{
    private readonly string _emoticonPrefix = "<sprite name=\""; // <sprite name="
    private readonly string _emoticonSuffix = "\">"; // ">

    private readonly Dictionary<string, string> _emoticonList = new Dictionary<string,string>()
    {
        {":(", "sadface"},
        {":D", "openhappy"},
        {";)", "winky"},
        {"<3", "love"},
        {":)", "happy"},
        {":p", "tongue" },
        {":P", "tongue" }
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
            replacedEmoticons = replacedEmoticons.Replace(emoticon.Key, _emoticonPrefix + emoticon.Value + _emoticonSuffix);
        }

        return replacedEmoticons;
    }
}
