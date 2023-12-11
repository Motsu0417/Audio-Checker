using System;
using UnityEngine;

public class NoteNameDetector
{
    private string[] noteNames = { "ド", "ド♯", "レ", "レ♯", "ミ", "ファ", "ファ♯", "ソ", "ソ♯", "ラ", "ラ♯", "シ" };

    public string GetNoteName(float freq)
    {
        // 周波数からMIDIノートナンバーを計算
        var noteNumber = calculateNoteNumberFromFrequency(freq);
        // 0:C - 11:B に収める
        var note = noteNumber % 12;
        // 0:C～11:Bに該当する音名を返す
        try
        {
            return noteNames[note];
        }
        catch(Exception e)
        {
            return e.ToString();   
        }
    }

    // See https://en.wikipedia.org/wiki/MIDI_tuning_standard
    private int calculateNoteNumberFromFrequency(float freq)
    {
        return Mathf.FloorToInt(69 + 12 * Mathf.Log(freq / 440, 2));
    }
}
