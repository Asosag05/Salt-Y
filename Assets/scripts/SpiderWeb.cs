using System.Collections.Generic;
using UnityEngine;

public class SpiderWeb : MonoBehaviour
{
    public class WebNode
    {
        public Vector2 currentPos;
        public Vector2 oldPos;

        public WebNode (Vector2 pos)
        {
            currentPos = pos;
            oldPos = pos;
        }
    }

    public Transform playerTransform;
    public float nodeDistance = 0.2f;
    public Vector2 gravity = new Vector2(0f, -9.8f);
    public int constraintIterations = 50;

    private LineRenderer lineRenderer;
    private List<WebNode> nodes = new List<WebNode>();

    private Vector2 puntoDeAnclaje;
    private bool estaDesplegada = false;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (estaDesplegada){
            DrawWeb();
        }
    }

    private void FixedUpdate()
    {
        if (!estaDesplegada){
            return;
        }
        Simulate();
        ApplyConstraints();
    }

    public void Desplegar(Vector2 anclaje, Vector2 posicionArana)
    {
        puntoDeAnclaje = anclaje;
        nodes.Clear();
        Vector2 direccion = posicionArana - anclaje;
        float distancia = direccion.magnitude;
        int cantidadNodos = Mathf.CeilToInt(distancia / nodeDistance);
        cantidadNodos = Mathf.Max(3, cantidadNodos);
        lineRenderer.positionCount = cantidadNodos;
        Vector2 direccionNormalizada = direccion.normalized;
        for (int i = 0; i < cantidadNodos; i++){
            Vector2 pos = anclaje + (direccionNormalizada * (i * nodeDistance));
            nodes.Add(new WebNode(pos));
        }
        estaDesplegada = true;
    }

    public void Cortar()
    {
        estaDesplegada = false;
        lineRenderer.positionCount = 0;
        nodes.Clear();
    }

    private void Simulate()
    {
        for (int i = 0; i < nodes.Count; i++){
            WebNode node = nodes[i];
            Vector2 velocity = node.currentPos - node.oldPos;
            node.oldPos = node.currentPos;
            node.currentPos += velocity;
            node.currentPos += gravity * Time.fixedDeltaTime * Time.fixedDeltaTime;
            nodes[i] = node;
        }
    }

    private void ApplyConstraints()
    {
        for (int iteration = 0; iteration < constraintIterations; iteration++){
            nodes[0].currentPos = puntoDeAnclaje;
            if (playerTransform != null){
                nodes[nodes.Count - 1].currentPos = playerTransform.position;
            }
            for (int i = 0; i < nodes.Count - 1; i++){
                WebNode nodeA = nodes[i];
                WebNode nodeB = nodes[i + 1];
                Vector2 direction = nodeB.currentPos - nodeA.currentPos;
                float currentDistance = direction.magnitude;
                float error = currentDistance - nodeDistance;
                Vector2 correctionDir = direction.normalized;
                Vector2 correctionVector = correctionDir * error;
                if (i != 0){
                    nodeA.currentPos += correctionVector * 0.5f;
                    nodeB.currentPos -= correctionVector * 0.5f;
                }
                else{
                    nodeB.currentPos -= correctionVector;
                }
            }
        }
    }

    private void DrawWeb()
    {
        for (int i = 0; i < nodes.Count; i++){
            lineRenderer.SetPosition(i, nodes[i].currentPos);
        }
    }
}
