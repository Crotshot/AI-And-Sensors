using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Helpers = Crotty.Helpers.StaticHelpers;

[ExecuteInEditMode]
public class AI_Vision : MonoBehaviour
{
    [Min(0.1f)]
    [SerializeField] public float range = 20, verticalStart = 0.1f, verticalEnd = 5f;
    [Range(0.1f, 180f)]
    [SerializeField] public float degrees = 90;
    [SerializeField] public float eyeHeight = 1.5f;
    [SerializeField] Color visionColor = Color.red;
    [SerializeField] int scanFrequency;
    [SerializeField] LayerMask layers, blockers;
    Mesh visionMesh;

    Collider[] scannedColliders;
    float scanTimer, scanInterval;
    List<Transform> objectsInSight = new List<Transform>();
    public UnityEvent<List<Transform>> objectsObserved;

    private void Start() {
        scanInterval = 1.0f / scanFrequency;
        if (objectsObserved == null)
            objectsObserved = new UnityEvent<List<Transform>>();
    }

#if UNITY_EDITOR
    private void OnValidate() {//When a change is made in the editor 
        visionMesh = CreateVisionMesh();
        scanInterval = 1.0f / scanFrequency;
        Scan();
    }
#endif
    private void Update() {
        scanTimer -= Time.deltaTime;
        if(scanTimer < 0) {//Timer created to scan less often as we do not need to scan every frame
            scanTimer += scanInterval;
            Scan();
        }
    }

    private void Scan() {
        scannedColliders = Physics.OverlapSphere(transform.position + Vector3.up * eyeHeight, range, layers, QueryTriggerInteraction.Collide);
        objectsInSight.Clear();

        for (int i = 0; i < scannedColliders.Length; ++i) {
            Transform obj = scannedColliders[i].transform;
            if (CanSeeObject(obj)) {
                objectsInSight.Add(obj);
            }
        }
        objectsObserved?.Invoke(objectsInSight);
    }

    /// <summary>
    /// Check to see if an object is within the AI sensors vision
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    private bool CanSeeObject(Transform obj) {
        Vector3 origin = transform.position + Vector3.up * eyeHeight,
        dest = obj.position,
        dir = dest - origin;
        dir.y = 0;
        //Checking y angle of object is within sensor degrees
        float deltaAngle = Vector3.Angle(dir, transform.forward);
        if (deltaAngle > degrees) {
            return false;
        }

        float bOrigin_Y = origin.y - (verticalStart / 2),
        tOrigin_Y = bOrigin_Y + verticalStart,
        bEnd_Y = origin.y - (verticalEnd / 2f),
        tEnd_Y = bEnd_Y + verticalEnd,
        distance = Helpers.Vector3PlaneDistance(origin, dest, Helpers.Plane.XZ);
        //Calculate if the object is between the top and bottom lines of the mesh;
        //Get the distance between the objects and calculate whethter it is above/below the lines
        if (Helpers.Y_Line_Check_Below(bOrigin_Y, bEnd_Y, range, distance, dest.y) || !Helpers.Y_Line_Check_Below(tOrigin_Y, tEnd_Y, range, distance, dest.y)) {
            return false;
        }
        Debug.DrawLine(origin, dest + Vector3.up * 0.05f);
        if(Physics.Linecast(origin, dest + Vector3.up * 0.05f, out RaycastHit hit, blockers, QueryTriggerInteraction.Ignore)) {
            //Debug.Log(hit.collider.name);
            return false;
        }
#if UNITY_EDITOR
        testColor = Color.white;
#endif
        return true;
    }


#if UNITY_EDITOR
    /// <summary>
    /// Creates a segmented conical-like mesh used for sensing vision on NPC's 
    /// </summary>
    /// <returns></returns>
    private Mesh CreateVisionMesh() {//Using a mesh a to visualize the area of vision the AI sensor has
        Mesh mesh = new Mesh();
        int segments = (int)degrees / 5 + ((int)degrees % 5 == 0 ? 0 : 1); //Segments of view cone
        int numTriangles = (segments * 4) + 2 + 2;
        int numVertices = numTriangles * 3;

        Vector3[] verts = new Vector3[numVertices];
        int[] tris = new int[numVertices];

        Vector3 bCenter = new Vector3(0, eyeHeight - verticalStart/2f,0); //Eyes Bottom
        Vector3 tCenter = bCenter + Vector3.up * verticalStart;//Eyes Top

        Vector3 bLeft = Quaternion.Euler(0, -degrees, 0) * Vector3.forward * range + (Vector3.up * verticalEnd * -0.5f + Vector3.up * eyeHeight);//End Left bottom
        Vector3 bRight = Quaternion.Euler(0, degrees, 0) * Vector3.forward * range + (Vector3.up * verticalEnd * -0.5f + Vector3.up * eyeHeight);//End Right bottom
        Vector3 tRight = bRight + Vector3.up * verticalEnd; // End Right Top
        Vector3 tLeft = bLeft + Vector3.up * verticalEnd;//End Left Top

        int vert = 0;
        //ADD verts to the array of verts that will be used to calculate are mesh
        //LEFT
        verts[vert++] = bCenter;
        verts[vert++] = bLeft;  
        verts[vert++] = tLeft;
        verts[vert++] = tLeft;
        verts[vert++] = tCenter;
        verts[vert++] = bCenter;

        //RIGHT
        verts[vert++] = bCenter;
        verts[vert++] = bRight;
        verts[vert++] = tRight;
        verts[vert++] = tRight;
        verts[vert++] = tCenter;
        verts[vert++] = bCenter;

        float currentAngle = -degrees, deltaAngle = (degrees * 2)/segments;
        for(int i = 0;  i < segments; ++i) {//Calculate vert positions for each segment
            bLeft = Quaternion.Euler(0, currentAngle, 0) * Vector3.forward * range + (Vector3.up * verticalEnd * -0.5f + Vector3.up * eyeHeight);
            bRight = Quaternion.Euler(0, currentAngle + deltaAngle, 0) * Vector3.forward * range + (Vector3.up * verticalEnd * -0.5f + Vector3.up * eyeHeight);

            tRight = bRight + Vector3.up * verticalEnd;
            tLeft = bLeft + Vector3.up * verticalEnd;
            //Add verts for each segment
            //OpposingSide
            verts[vert++] = bLeft;
            verts[vert++] = bRight;
            verts[vert++] = tRight;

            verts[vert++] = tRight;
            verts[vert++] = tLeft;
            verts[vert++] = bLeft;

            //Top
            verts[vert++] = tCenter;
            verts[vert++] = tLeft;
            verts[vert++] = tRight;

            //Bottom
            verts[vert++] = bCenter;
            verts[vert++] = bLeft;
            verts[vert++] = bRight;

            currentAngle += deltaAngle;//Increment and go to next segment
        }

        for(int i = 0; i < numVertices; ++i) {
            tris[i] = i;
        }

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        return mesh;
    }

    Color testColor;
    private void OnDrawGizmos() {//Draw the mesh in edit mode for easier editing
        if (visionMesh) {
            Gizmos.color = visionColor;
            Gizmos.DrawMesh(visionMesh, transform.position, transform.rotation);

            Gizmos.DrawWireSphere(transform.position + Vector3.up * eyeHeight, range);
            for(int i = 0; i < scannedColliders.Length; ++i) {
                if (scannedColliders[i] != null) {
                    Gizmos.DrawSphere(scannedColliders[i].transform.position, 0.1f);
                }
            }

            Gizmos.color = testColor;
            foreach(Transform obj in objectsInSight) {
                if (obj != null) {
                    Gizmos.DrawSphere(obj.position, 0.2f);
                }
            }
        }
    }
#endif
}