using UnityEngine;

public struct HexCoords
{
    public HexCoords(int _q, int _r)
    {
        q = _q;
        r = _r;
    }

    public int q;
    public int r;

    public override bool Equals(object obj) //opcjonalne, ale szybsze
    {
        return obj is HexCoords other && q == other.q && r == other.r;
    }

    public override int GetHashCode() //do najszybszego sposobu przeszukiwania listy
    {
        return (q, r).GetHashCode(); 
    }

    public override string ToString()
    {
        return $"({q}, {r})";
    }


}

//struct dziala tak, ze porownywane sa wartosci, a nie referencje; w calss trzeba by nadpisywac equals i hash https://chatgpt.com/c/6950d5ef-40a0-832a-8a64-3251dcc44b44