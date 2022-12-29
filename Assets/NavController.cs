using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR.ARFoundation;

public class NavController : MonoBehaviour
{
    public GameObject Player; //GameObject that contains the blue arrow
    public GameObject Objective;
    public GameObject destmark;
    private GameObject clone;

    private GameObject _handicapped;
    private bool swapped = false;
    private Vector3 _player;
    private Vector3 destination;
    public LineRenderer lineRenderer;
    private NavMeshPath path;

    public GameObject MiniMap_GUI;
    private TMP_Text textViewer;


    //Optimization of the distance calculation
    private int pathcorners;
    private float prevdist = 0.0f;

    //Ending text
    public GameObject ending;
    
    void Start(){
        updateDest();
        _handicapped = GameObject.Find("Handicapped");
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        clone = Instantiate(destmark,Objective.transform);

        textViewer = MiniMap_GUI.GetComponentInChildren<TMP_Text>();

        pathcorners = 0;
        prevdist = 0.0f;
    }
    void Update()
    {
        if(destination != null){
            path = new NavMeshPath();
            _player = Player.transform.position;
            RedrawPath();
        }
        
    }

    public void swap(){
        swapped = !swapped;
        if (swapped){
            destination=_handicapped.transform.position;
        }
        else{
            destination = Objective.transform.position;
        }
        Destroy(clone);
        clone = Instantiate(destmark,Objective.transform);
    }

    public void updateDest(){
        if(Objective != null){
        destination=Objective.transform.position;
        }
        Destroy(clone);
        clone = Instantiate(destmark,Objective.transform);
    }

    void RedrawPath(){
        bool works = NavMesh.CalculatePath(_player, destination, NavMesh.AllAreas, path); //Saves the path in the path variable.
        float dist = 0.0f;
        if(works){
            if(pathcorners != path.corners.Length){
                pathcorners = path.corners.Length;
                prevdist = 0.0f;
                for (var i=1;i<path.corners.Length;i++){
                    prevdist += Vector3.Distance(path.corners[i],path.corners[i+1]);

                }

            }
            for (var i=0;i<path.corners.Length;i++){
                dist = prevdist + Vector3.Distance(path.corners[0],path.corners[1]);
                path.corners[i].y += 0.1f;
            }
        textViewer.SetText("Distance: " + (int)dist +"m");
        Vector3[] corners = path.corners;
        lineRenderer.positionCount = corners.Length;
        lineRenderer.SetPositions(corners);

        //Start ending coroutine
        if (dist<1){
            StartCoroutine(EndGame());
        }
        
        }
        IEnumerator EndGame(){
            ending.SetActive(true);
            MiniMap_GUI.SetActive(false);      
            yield return new WaitForSeconds(5); //wait 5 secconds
            Application.Quit();
        }

    }
}
