using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class NavController : MonoBehaviour
{
    public GameObject Indicator; //GameObject that contains the blue arrow
    public GameObject Objective;
    private Vector3 player;
    private Vector3 destination;
    public LineRenderer lineRenderer;
    private NavMeshPath path;
    
    void Start(){
        destination=Objective.transform.position;
    }
    void Update()
    {
        path = new NavMeshPath();
        player = Indicator.transform.position;
        RedrawPath();
    }

    void RedrawPath(){
        bool works = NavMesh.CalculatePath(player, destination, NavMesh.AllAreas, path); //Saves the path in the path variable.
        if(works){
        Vector3[] corners = path.corners;
        print(path.corners.Length);
        lineRenderer.positionCount = corners.Length;
        lineRenderer.SetPositions(corners);
        
        }
    }
}
