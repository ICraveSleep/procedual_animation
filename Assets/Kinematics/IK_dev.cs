using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class IK_dev : MonoBehaviour
{
    public int chainLength = 2;
    public Transform target;
    
    [Header("Solver Parameters")]
    public int iterations = 10;
    
    //Error from taget point
    public float delta = 0.001f;

    [Range(0, 1)]
    public float snapBackStrength = 1.0f;

    // proteced
    protected Transform[] links;
    protected float[] linkLengths;
    protected float totalLength;
    protected Vector3[] linkPositions;
    

    // Start is called before the first frame update
    void Awake() {
        Init();
    }

    void Init() {
        links = new Transform[chainLength + 1];
        linkPositions = new Vector3[chainLength + 1];
        linkLengths = new float[chainLength];
        totalLength = 0;

        Debug.Log("Number of 'links': " + links.Length);

        //Init data
        var current = this.transform;    
        for (int i = links.Length-1; i>=0; i--){
            links[i] = current;

            // When the base link is reached, no length can be extracted past that.
            if (i == links.Length-1){
                // Do nothing
            }
            else{
                linkLengths[i] = (links[i+1].position - links[i].position).magnitude;
                Debug.Log("d links: " + links[i+1].name + " and " + links[i].name + " = " + linkLengths[i]);
                totalLength += linkLengths[i];
            }

            current = current.parent;
        }


    }

    private void LateUpdate() {
        ResolveIK();
    }

    private void ResolveIK(){
        if (target == null){
            return;
        }

        // To change the chainlengh during runtime
        if (linkLengths.Length != chainLength){
            Init();
        }

        //Get positions
        for(int i = 0; i < links.Length; i++){
            linkPositions[i] = links[i].position;
        }

        //Checking if it is possible to reach the target TCP
        // Taking square for a faster computational check
        var sqr_target_distance = (target.position - links[0].position).sqrMagnitude;
        var sqr_chain_distance = totalLength * totalLength;

        if(sqr_target_distance >= sqr_chain_distance){
            //Strech towards the target TCP

            // TODO make it support different lengths
            var direction = (target.position - linkPositions[0]).normalized;
            
            for(int i = 1; i < linkPositions.Length; i++){ // int i = 1, since the base_link has no parent link
                linkPositions[i] = linkPositions[i-1] + direction * linkLengths[i-1];
            }
        }
        else{
            for (int n = 0; n < iterations; n++){
                // Inverse Kinematic Solver
                for (int i = linkPositions.Length-1; i > 0; i--){
                    if (i == linkPositions.Length-1){
                        linkPositions[i] = target.position;
                    }
                    else{
                        linkPositions[i] = linkPositions[i+1] + (linkPositions[i] - linkPositions[i+1]).normalized * linkLengths[i];
                    }
                }
                
                // Forward Kinemtaic Solver
                for (int i = 1; i < linkPositions.Length; i++){
                    linkPositions[i] = linkPositions[i-1] + (linkPositions[i] - linkPositions[i-1]).normalized * linkLengths[i-1];
                }

                //If error tolerance is reached
                if ((linkPositions[linkPositions.Length -1] - target.position).sqrMagnitude <= delta * delta){
                    break;
                }
            }

        }

        //Set position
        for(int i = 0; i < links.Length; i++){
            links[i].position = linkPositions[i];
        }


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDrawGizmos() {
        var current = this.transform;
        for (int i = 0; i < chainLength && current != null && current.parent != null; i++){
            var scale = Vector3.Distance(current.position, current.parent.position) * 0.1f;
            Handles.matrix = Matrix4x4.TRS(current.position, Quaternion.FromToRotation(Vector3.up, current.parent.position - current.position), new Vector3(scale, Vector3.Distance(current.parent.position, current.position), scale));
            Handles.color = Color.green;
            Handles.DrawWireCube(Vector3.up * 0.5f, Vector3.one);
            current = current.parent;
        }

    }
}
