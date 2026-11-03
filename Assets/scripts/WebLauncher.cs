using UnityEngine;

public class WebLauncher : MonoBehaviour
{

    public SpiderWeb telarana;
    public float distanciaMax = 15f;
    public LayerMask capasEnganchables;


    public float radioAsistencia = 0.1f;


    public bool mostrarIndicador = true;
    public Material materialIndicador;
    public Color colorValido = new Color(0.2f, 1f, 0.4f, 0.9f);
    public Color colorInvalido = new Color(1f, 0.3f, 0.3f, 0.6f);
    public Color colorNoDisponible = new Color(0.7f, 0.7f, 0.7f, 0.35f);
    public float grosorLinea = 0.04f;
    public float radioMira = 0.25f;
    public int sortingOrder = 50;

    private SpringJoint2D springJoint;
    private Vector2 puntoDeAnclaje;

    private movimiento scriptMovimiento;
    private bool puedeDisparar = true;
    private bool enganchado = false;

    private Camera cam;
    private LineRenderer mira;
    private const int SegmentosMira = 24;

    void Awake()
    {
        springJoint = GetComponent<SpringJoint2D>();
        if (springJoint != null){
            springJoint.enabled = false;
        }
        scriptMovimiento = GetComponent<movimiento>();
        cam = Camera.main;
        CrearIndicadores();
    }

    
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

        ActualizarIndicador();
    }

    private Vector2 ObtenerPosicionRaton()
    {
        if (cam == null){
            cam = Camera.main;
        }
        return cam.ScreenToWorldPoint(Input.mousePosition);
    }

    private bool BuscarObjetivo(Vector2 origen, Vector2 posicionRaton, out Vector2 punto)
    {
        punto = Vector2.zero;

  
        if (radioAsistencia > 0f){
            Collider2D[] cercanos = Physics2D.OverlapCircleAll(posicionRaton, radioAsistencia, capasEnganchables);
            float mejorDistRaton = float.MaxValue;
            bool encontrado = false;

            foreach (Collider2D col in cercanos){
                Vector2 candidato = col.ClosestPoint(posicionRaton);
                Vector2 haciaCandidato = candidato - origen;
                float dist = haciaCandidato.magnitude;
                if (dist > distanciaMax || dist < 0.01f){
                    continue;
                }

                float distRaton = (candidato - posicionRaton).sqrMagnitude;
                if (distRaton >= mejorDistRaton){
                    continue;
                }

                // Comprobar línea de visión
                RaycastHit2D hit = Physics2D.Raycast(origen, haciaCandidato / dist, dist + 0.1f, capasEnganchables);
                if (hit.collider == col){
                    mejorDistRaton = distRaton;
                    punto = hit.point;
                    encontrado = true;
                }
            }

            if (encontrado){
                return true;
            }
        }


        Vector2 direccion = posicionRaton - origen;
        if (direccion.sqrMagnitude < 0.0001f){
            return false;
        }
        RaycastHit2D directo = Physics2D.Raycast(origen, direccion.normalized, distanciaMax, capasEnganchables);
        if (directo.collider != null){
            punto = directo.point;
            return true;
        }
        return false;
    }


    private void CrearIndicadores()
    {
        Material mat = materialIndicador;
        if (mat == null){
            mat = new Material(Shader.Find("Sprites/Default"));
        }

        mira = CrearLinea("IndicadorMira", SegmentosMira, true, mat);
        mira.enabled = false;
    }

    private LineRenderer CrearLinea(string nombre, int puntos, bool cerrada, Material mat)
    {
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(transform, false);

        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.loop = cerrada;
        lr.positionCount = puntos;
        lr.startWidth = grosorLinea;
        lr.endWidth = grosorLinea;
        lr.sharedMaterial = mat;
        lr.sortingOrder = sortingOrder;
        return lr;
    }

    private void ActualizarIndicador()
    {
        bool visible = mostrarIndicador && !enganchado;
        mira.enabled = visible;
        if (!visible){
            return;
        }

        Vector2 origen = transform.position;
        Vector2 posicionRaton = ObtenerPosicionRaton();

        bool hayObjetivo = BuscarObjetivo(origen, posicionRaton, out Vector2 punto);

        Vector2 destino = hayObjetivo
            ? punto
            : origen + Vector2.ClampMagnitude(posicionRaton - origen, distanciaMax);

        Color color = !puedeDisparar ? colorNoDisponible
                    : hayObjetivo ? colorValido
                    : colorInvalido;


        // Criculo impacto
        float radio = hayObjetivo ? radioMira : radioMira * 0.6f;
        mira.startColor = color;
        mira.endColor = color;
        for (int i = 0; i < SegmentosMira; i++){
            float angulo = (i / (float)SegmentosMira) * Mathf.PI * 2f;
            Vector2 p = destino + new Vector2(Mathf.Cos(angulo), Mathf.Sin(angulo)) * radio;
            mira.SetPosition(i, p);
        }
    }


    private void IntentarEnganche()
    {
        if (!puedeDisparar){
            return;
        }

        Vector2 posicionRaton = ObtenerPosicionRaton();
        Vector2 posicionActual = transform.position;

        if (BuscarObjetivo(posicionActual, posicionRaton, out Vector2 punto)){
            puntoDeAnclaje = punto;
            puedeDisparar = false;
            enganchado = true;
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
        enganchado = false;
        telarana.Cortar();
        if (springJoint != null){
            springJoint.enabled = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Muestra el radio de asistencia alrededor del ratón al ejecutar con el objeto seleccionado
        if (Application.isPlaying && cam != null){
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(ObtenerPosicionRaton(), radioAsistencia);
        }
    }
}