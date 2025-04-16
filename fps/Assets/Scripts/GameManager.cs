using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Transform[] Points;
    void Start()
    {
        Instance = this;
    }

    public Vector3 GetPoints()
    {
        return Points[Random.Range(0, Points.Length)].position;
    }

}
