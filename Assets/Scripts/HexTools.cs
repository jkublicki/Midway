using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
//using UnityEditor.TerrainTools;
using UnityEngine;
using static UnitManager;

/*
Zalozenia: 
Wielkość heksa określa średnica opasającego go okręgu
Wspolrzedne heksow to q - kolumna i r - wiersz
Pomocnicza wspolrzedna s, dynamiczne wyliczana, gdzie s + q + r = 0
Uklad wspolrzednych hexow jest jak w axial w https://www.redblobgames.com/grids/hexagons/#coordinates
Wiersze numerujemy od 0 na środku rosnące w dół
Kolumny są przechylone na lewo, srodowkowa 0
*/

public static class HexTools
{

   
    public static Vector2 HexCoordsToCart(HexCoords hexCoords)
    {
        float x = Sett.Sq3 * Sett.HexR * (hexCoords.q + hexCoords.r / 2.0f);
        float y = -1.5f * hexCoords.r * Sett.HexR;
        return new Vector2(x, y);
    }

    public static HexCoords HexRound(float q, float r)
    {
        float s = -q - r;

        int rq = Mathf.RoundToInt(q);
        int rr = Mathf.RoundToInt(r);
        int rs = Mathf.RoundToInt(s);

        float dq = Mathf.Abs(rq - q);
        float dr = Mathf.Abs(rr - r);
        float ds = Mathf.Abs(rs - s);

        if (dq > dr && dq > ds)
            rq = -rr - rs;
        else if (dr > ds)
            rr = -rq - rs;
        // else: rs gets fixed implicitly - s nie korygujemy, bo nie jest zwracane

        return new HexCoords(rq, rr);
    }


    public static HexCoords CartToHexCoords(Vector2 point)
    {
        //pomysł: zaokraglamy punkt do najblizszych linii q i s, czyli y = -sqrt(3) * x + c oraz y = sqrt(3) * x + c

        float q = (point.y + Sett.Sq3 * point.x) / (3.0f * Sett.HexR);
        float s = (point.y - Sett.Sq3 * point.x) / (3.0f * Sett.HexR);

        float r = -q - s;


        //zwykłe zaokrąglenie czasem powoduje błędy
        return HexRound(q, r);
    }

    //funkcja do zwracania listy sąsiadów w obrębie range / z i bez samego siebie
    public static void HexNeighbors(HexCoords center, int range, bool excludeCenter, List<HexCoords> neighbors)
    {
        neighbors.Clear();

        //iteracja po delcie wierszy
        for (int dr = -range; dr <= range; dr++)            
        {
            //iteracja po delcie kolumn
            for (int dq = Mathf.Max(-range - dr, -range); dq <= Mathf.Min(range, range - dr); dq++)
            {
                if (excludeCenter && dq == 0 && dr == 0)
                {
                    continue;
                }

                int q = center.q + dq;
                int r = center.r + dr;

                neighbors.Add(new HexCoords(q, r));
            }
        }
    }


    // Claude: Dystans między dwoma hexami w axial coordinates
    // Gdy q i r maja rozne znaki: możemy isc ukosnie → max(dq, dr)
    // Gdy q i r mają te same znaki: musimy sumowac → dq + dr
    public static int HexDistance(HexCoords a, HexCoords b)
    {
        int dq = Mathf.Abs(a.q - b.q);
        int dr = Mathf.Abs(a.r - b.r);

        if (Mathf.Sign(a.q - b.q) == Mathf.Sign(a.r - b.r))
            return dq + dr;
        else
            return Mathf.Max(dq, dr);
    }


    public static float HexDirectionToRotation(HexDirection direction)
    {
        switch (direction)
        {
            case HexDirection.NorthWest:
                return 60.0f;
            case HexDirection.NorthEast:
                return 120.0f;
            case HexDirection.East:
                return 180.0f;
            case HexDirection.SouthEast:
                return 240.0f;
            case HexDirection.SouthWest:
                return 300.0f;
            case HexDirection.West:
                return 0.0f;
            default:
                return 0.0f;
        }
    }

    //https://www.redblobgames.com/grids/hexagons/#rotation
    public static HexCoords RotateVector(HexCoords vector, HexDirection originDirection, HexDirection destinationDirection)
    {
        int rot = Mathf.RoundToInt((HexDirectionToRotation(destinationDirection) - HexDirectionToRotation(originDirection)) / 60.0f);
        if (rot < 0)
        {
            rot = 6 + rot;
        }

        Vector3Int qrs = new Vector3Int(vector.q, vector.r, -vector.q - vector.r);

        for (int i = 0; i < rot; i++)
        {
            qrs = new Vector3Int(-qrs.y, -qrs.z, -qrs.x);
        }

        return new HexCoords(qrs.x, qrs.y);
    }

    public static HexCoords Neighbor(HexCoords originHexCoords, HexDirection originDirection)
    {
        int deltaQ; 
        int deltaR;

        switch (originDirection)
        {
            case HexDirection.NorthWest:
                deltaQ = 0;
                deltaR = -1;
                break;
            case HexDirection.NorthEast:
                deltaQ = 1;
                deltaR = -1;
                break;
            case HexDirection.East:
                deltaQ = 1;
                deltaR = 0;
                break;
            case HexDirection.SouthEast:
                deltaQ = 0;
                deltaR = 1;
                break;
            case HexDirection.SouthWest:
                deltaQ = -1;
                deltaR = 1;
                break;
            case HexDirection.West:
                deltaQ = -1;
                deltaR = 0;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(originDirection), originDirection, "Nieobsługiwany kierunek hexa");
        }

        return new HexCoords(originHexCoords.q + deltaQ, originHexCoords.r + deltaR);
    }



    public static HexDirection AdjacentDirection(HexDirection direction, HexDirectionChange change)
    {
        HexDirection afterLeft;
        HexDirection afterRight;
        HexDirection afterNa;

        switch (direction) 
        {
            case HexDirection.NorthWest:
                afterLeft = HexDirection.West;
                afterRight = HexDirection.NorthEast;
                afterNa = HexDirection.NorthWest;
                break;
            case HexDirection.NorthEast:
                afterLeft = HexDirection.NorthWest;
                afterRight = HexDirection.East;
                afterNa = HexDirection.NorthEast;
                break;
            case HexDirection.East:
                afterLeft = HexDirection.NorthEast;
                afterRight = HexDirection.SouthEast;
                afterNa = HexDirection.East;
                break;
            case HexDirection.SouthEast:
                afterLeft = HexDirection.East;
                afterRight = HexDirection.SouthWest;
                afterNa = HexDirection.SouthEast;
                break;
            case HexDirection.SouthWest:
                afterLeft = HexDirection.SouthEast;
                afterRight = HexDirection.West;
                afterNa = HexDirection.SouthWest;
                break;
            case HexDirection.West:
                afterLeft = HexDirection.SouthWest;
                afterRight = HexDirection.NorthWest;
                afterNa = HexDirection.West;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, "Nieobsługiwany kierunek hexa");
        }

        switch (change)
        {
            case HexDirectionChange.NA:
                return afterNa;
            case HexDirectionChange.ToLeft: 
                return afterLeft;
            case HexDirectionChange.ToRight:
                return afterRight;
            default:
                throw new ArgumentOutOfRangeException(nameof(change), change, "Nieobsługiwana zmiana kierunku hexa");
        }
    }

}
