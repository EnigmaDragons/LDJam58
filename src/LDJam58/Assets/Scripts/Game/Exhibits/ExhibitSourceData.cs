using System;
using UnityEngine;

[Serializable]
public class ExhibitSourceData
{
    [SerializeField] private string _name = "";
    [SerializeField] private string _theme = "";
    [SerializeField] private string _size = "";
    [SerializeField] private string _rarity = "";
    [SerializeField] private int _enjoyment;
    [SerializeField] private int _popularity;
    [SerializeField] private string _artistNotes = "";
    [SerializeField] private int _artistEffort;
    [SerializeField] private int _vfxEffort;

    public string Name => _name;
    public string Theme => _theme;
    public string Size => _size;
    public string Rarity => _rarity;
    public int Enjoyment => _enjoyment;
    public int Popularity => _popularity;
    public string ArtistNotes => _artistNotes;
    public int ArtistEffort => _artistEffort;
    public int VfxEffort => _vfxEffort;

    public void SetName(string value) => _name = value;
    public void SetTheme(string value) => _theme = value;
    public void SetSize(string value) => _size = value;
    public void SetRarity(string value) => _rarity = value;
    public void SetEnjoyment(int value) => _enjoyment = value;
    public void SetPopularity(int value) => _popularity = value;
    public void SetArtistNotes(string value) => _artistNotes = value;
    public void SetArtistEffort(int value) => _artistEffort = value;
    public void SetVfxEffort(int value) => _vfxEffort = value;
}
