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

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)){
            IntentarEnganche();
        }
        else if (Input.GetMouseButtonUp(0)){
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
            telarana.DispararAnimacion(puntoDeAnclaje, posicionActual, ConectarFisicas);
        }
    }

    private void ConectarFisicas()
    {
        if (springJoint != null){
                springJoint.connectedAnchor = puntoDeAnclaje;
                springJoint.distance = Vector2.Distance(transform.position, puntoDeAnclaje) * 1.0f;
                springJoint.enabled = true;
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
