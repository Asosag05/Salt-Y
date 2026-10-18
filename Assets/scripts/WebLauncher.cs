using UnityEngine;

public class WebLauncher : MonoBehaviour
{
    public SpiderWeb telarana;
    public float distanciaMax = 15f;
    public LayerMask capasEnganchables;
    private SpringJoint2D springJoint;
    private Vector2 puntoDeAnclaje;

    void Awake()
    {
        springJoint = GetComponent<SpringJoint2D>();
        if (springJoint != null){
            springJoint.enabled = false;
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W)){
            IntentarEnganche();
        }
        else if (Input.GetKeyUp(KeyCode.W)){
            SoltarEnganche();
        }
    }

    private void IntentarEnganche()
    {
        Vector2 posicionRaton = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 posicionActual = transform.position;
        Vector2 direccion = (posicionRaton - posicionActual).normalized;

        RaycastHit2D hit = Physics2D.Raycast(posicionActual, direccion, distanciaMax, capasEnganchables);
        if (hit.collider != null){
            puntoDeAnclaje = hit.point;
            telarana.Desplegar(puntoDeAnclaje, posicionActual);
            if (springJoint != null){
                springJoint.connectedAnchor = puntoDeAnclaje;
                springJoint.distance = Vector2.Distance(posicionActual, puntoDeAnclaje) * 0.8f;
                springJoint.enabled = true;
            }
        }
    }

    private void SoltarEnganche()
    {
        telarana.Cortar();
        if (springJoint != null){
            springJoint.enabled = false;
        }
    }
}
