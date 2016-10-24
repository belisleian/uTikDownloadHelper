using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Runtime.InteropServices;
public class IniFile
{
    [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileStringA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
    // API functions
    private static extern int GetPrivateProfileString(string lpApplicationName, string lpKeyName, string lpDefault, System.Text.StringBuilder lpReturnedString, int nSize, string lpFileName);
    [DllImport("kernel32.dll", EntryPoint = "WritePrivateProfileStringA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
    private static extern int WritePrivateProfileString(string lpApplicationName, string lpKeyName, string lpString, string lpFileName);
    [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileIntA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
    private static extern int GetPrivateProfileInt(string lpApplicationName, string lpKeyName, int nDefault, string lpFileName);
    [DllImport("kernel32.dll", EntryPoint = "WritePrivateProfileStringA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
    private static extern int FlushPrivateProfileString(int lpApplicationName, int lpKeyName, int lpString, string lpFileName);

    string strFilename;
    // Constructor, accepting a filename
    public IniFile(string Filename)
    {
        strFilename = Filename;
    }

    // Read-only filename property
    public string FileName
    {
        get { return strFilename; }
    }

    public string GetString(string Section, string Key, string Default)
    {
        string functionReturnValue = null;
        // Returns a string from your INI file
        int intCharCount = 0;
        System.Text.StringBuilder objResult = new System.Text.StringBuilder(256);
        intCharCount = GetPrivateProfileString(Section, Key, Default, objResult, objResult.Capacity, strFilename);
        if (intCharCount > 0)
            functionReturnValue = Strings.Left(objResult.ToString(), intCharCount);
        return functionReturnValue;
    }

    public int GetInteger(string Section, string Key, int Default)
    {
        // Returns an integer from your INI file
        return GetPrivateProfileInt(Section, Key, Default, strFilename);
    }

    public bool GetBoolean(string Section, string Key, bool Default)
    {
        // Returns a boolean from your INI file
        return (GetPrivateProfileInt(Section, Key, Convert.ToInt32(Default), strFilename) == 1);
    }

    public void WriteString(string Section, string Key, string Value)
    {
        // Writes a string to your INI file
        WritePrivateProfileString(Section, Key, Value, strFilename);
        Flush();
    }

    public void WriteInteger(string Section, string Key, int Value)
    {
        // Writes an integer to your INI file
        WriteString(Section, Key, Convert.ToString(Value));
        Flush();
    }

    public void WriteBoolean(string Section, string Key, bool Value)
    {
        // Writes a boolean to your INI file
        WriteString(Section, Key, Convert.ToString(Convert.ToInt32(Value)));
        Flush();
    }

    private void Flush()
    {
        // Stores all the cached changes to your INI file
        FlushPrivateProfileString(0, 0, 0, strFilename);
    }

}
