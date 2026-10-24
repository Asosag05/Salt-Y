using UnityEngine;

public class WebLauncher : MonoBehaviour
{
    public SpiderWeb telarana;
    public float distanciaMax = 15f;
    public LayerMask capasEnganchables;

    private SpringJoint2D springJoint;
    private Vector2 puntoDeAnclaje;

    private movimiento scriptMovimiento;
    private bool puedeDisparar = true;

    void Awake()
    {
        springJoint = GetComponent<SpringJoint2D>();
        if (springJoint != null){
            springJoint.enabled = false;
        }
        scriptMovimiento = GetComponent<movimiento>();
    }

    // Update is called once per frame
    void Update()
    {
        if (scriptMovimiento != null && scriptMovimiento.IsGrounded()){
            puedeDisparar = true;
        }
        if (springJoint != null && springJoint.enabled){
            Vector2 posicionActual = transform.position;
            Vector2 direccionAlAnclaje = puntoDeAnclaje - posicionActual;
            float distancia = direccionAlAnclaje.magnitude;
            RaycastHit2D hitObstaculo = Physics2D.Raycast(posicionActual, direccionAlAnclaje.normalized, distancia - 0.2f, capasEnganchables);
            if (hitObstaculo.collider != null){
                SoltarEnganche();
            }
        }
        if (Input.GetMouseButtonDown(0)){
            IntentarEnganche();
        }
        else if (Input.GetMouseButtonUp(0)){
            SoltarEnganche();
        }
    }

    private void IntentarEnganche()
    {
        if (!puedeDisparar){
            return;
        }

        Vector2 posicionRaton = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 posicionActual = transform.position;
        Vector2 direccion = (posicionRaton - posicionActual).normalized;

        RaycastHit2D hit = Physics2D.Raycast(posicionActual, direccion, distanciaMax, capasEnganchables);
        if (hit.collider != null){
            puntoDeAnclaje = hit.point;
            puedeDisparar = false;
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
