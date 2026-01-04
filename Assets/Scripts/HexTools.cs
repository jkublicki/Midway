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
            /*
            String s = "";
            foreach (HexCoords c in result)
            {
                s += "(" + c.q.ToString() + ", " + c.r.ToString() + "), ";

            }
            Debug.Log(s);
            */

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

    public enum HexDirectionPT //point top
    {
        NORTH_WEST,
        NORTH_EAST,
        EAST,
        SOUTH_EAST,
        SOUTH_WEST,
        WEST
    }

    public static float HexDirectionToRotation(HexDirectionPT direction)
    {
        switch (direction)
        {
            case HexDirectionPT.NORTH_WEST:
                return 60.0f;
            case HexDirectionPT.NORTH_EAST:
                return 120.0f;
            case HexDirectionPT.EAST:
                return 180.0f;
            case HexDirectionPT.SOUTH_EAST:
                return 240.0f;
            case HexDirectionPT.SOUTH_WEST:
                return 300.0f;
            case HexDirectionPT.WEST:
                return 0.0f;
            default:
                return 0.0f;
        }
    }

    //https://www.redblobgames.com/grids/hexagons/#rotation
    public static HexCoords RotateVector(HexCoords vector, HexDirectionPT originDirection, HexDirectionPT destinationDirection)
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

    public static HexCoords Neighbor(HexCoords originHexCoords, HexDirectionPT originDirection)
    {
        int deltaQ; 
        int deltaR;

        switch (originDirection)
        {
            case HexDirectionPT.NORTH_WEST:
                deltaQ = 0;
                deltaR = -1;
                break;
            case HexDirectionPT.NORTH_EAST:
                deltaQ = 1;
                deltaR = -1;
                break;
            case HexDirectionPT.EAST:
                deltaQ = 1;
                deltaR = 0;
                break;
            case HexDirectionPT.SOUTH_EAST:
                deltaQ = 0;
                deltaR = 1;
                break;
            case HexDirectionPT.SOUTH_WEST:
                deltaQ = -1;
                deltaR = 1;
                break;
            case HexDirectionPT.WEST:
                deltaQ = -1;
                deltaR = 0;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(originDirection), originDirection, "Nieobsługiwany kierunek hexa");
        }

        return new HexCoords(originHexCoords.q + deltaQ, originHexCoords.r + deltaR);
    }

    public enum HexDirectionChangeE
    {
        NA,
        TO_LEFT,
        TO_RIGHT,
    }

    public static HexDirectionPT AdjacentDirection(HexDirectionPT direction, HexDirectionChangeE change)
    {
        HexDirectionPT afterLeft;
        HexDirectionPT afterRight;
        HexDirectionPT afterNa;

        switch (direction) 
        {
            case HexDirectionPT.NORTH_WEST:
                afterLeft = HexDirectionPT.WEST;
                afterRight = HexDirectionPT.NORTH_EAST;
                afterNa = HexDirectionPT.NORTH_WEST;
                break;
            case HexDirectionPT.NORTH_EAST:
                afterLeft = HexDirectionPT.NORTH_WEST;
                afterRight = HexDirectionPT.EAST;
                afterNa = HexDirectionPT.NORTH_EAST;
                break;
            case HexDirectionPT.EAST:
                afterLeft = HexDirectionPT.NORTH_EAST;
                afterRight = HexDirectionPT.SOUTH_EAST;
                afterNa = HexDirectionPT.EAST;
                break;
            case HexDirectionPT.SOUTH_EAST:
                afterLeft = HexDirectionPT.EAST;
                afterRight = HexDirectionPT.SOUTH_WEST;
                afterNa = HexDirectionPT.SOUTH_EAST;
                break;
            case HexDirectionPT.SOUTH_WEST:
                afterLeft = HexDirectionPT.SOUTH_EAST;
                afterRight = HexDirectionPT.WEST;
                afterNa = HexDirectionPT.SOUTH_WEST;
                break;
            case HexDirectionPT.WEST:
                afterLeft = HexDirectionPT.SOUTH_WEST;
                afterRight = HexDirectionPT.NORTH_WEST;
                afterNa = HexDirectionPT.WEST;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, "Nieobsługiwany kierunek hexa");
        }

        switch (change)
        {
            case HexDirectionChangeE.NA:
                return afterNa;
            case HexDirectionChangeE.TO_LEFT: 
                return afterLeft;
            case HexDirectionChangeE.TO_RIGHT:
                return afterRight;
            default:
                throw new ArgumentOutOfRangeException(nameof(change), change, "Nieobsługiwana zmiana kierunku hexa");
        }
    }

}
