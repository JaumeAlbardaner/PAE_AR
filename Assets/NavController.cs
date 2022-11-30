using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class NavController : MonoBehaviour
{
    public GameObject Player; //GameObject that contains the blue arrow
    public GameObject Objective;
    private Vector3 _player;
    private Vector3 destination;
    public LineRenderer lineRenderer;
    private NavMeshPath path;
    
    void Start(){
        updateDest();
    }
    void Update()
    {
        if(destination != null){
            path = new NavMeshPath();
            _player = Player.transform.position;
            RedrawPath();
        }
        
    }

    public void updateDest(){
        if(Objective != null){
        destination=Objective.transform.position;
        }
    }

    void RedrawPath(){
        bool works = NavMesh.CalculatePath(_player, destination, NavMesh.AllAreas, path); //Saves the path in the path variable.
        if(works){
            for (var i=0;i<path.corners.Length;i++){
                path.corners[i].y += 0.1f;
            }
        Vector3[] corners = path.corners;
        lineRenderer.positionCount = corners.Length;
        lineRenderer.SetPositions(corners);
        
        }
    }
}
