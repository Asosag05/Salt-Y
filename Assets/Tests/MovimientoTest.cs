using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class MovimientoTest
{
    private GameObject player;
    private GameObject ground;
    private movimiento movimientoScript;
    private Rigidbody2D rb;
    private DistanceJoint2D distanceJoint;
    
    [SetUp]
    public void Setup()
    {
        //Creamos suelo
        ground = new GameObject("Ground");
        ground.transform.position = new Vector3(0, -2f, 0);
        BoxCollider2D groundCollider = ground.AddComponent<BoxCollider2D>();
        groundCollider.size = new Vector2(20f, 1f);

        //Creamos layer Ground 
        int groundLayerID = LayerMask.NameToLayer("Ground");
        if (groundLayerID == -1)
        {
            groundLayerID = 0;
        }

        ground.layer = groundLayerID;

        //Jugador
        player = new GameObject("Player");
        player.transform.position = new Vector3(0, 0, 0);
        rb= player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 6f;
        BoxCollider2D playerCollider = player.AddComponent<BoxCollider2D>();
        playerCollider.size = new Vector2(1f, 1f);

        distanceJoint = player.AddComponent<DistanceJoint2D>();
        distanceJoint.enabled = false;

        //Script de testeo
        movimientoScript = player.AddComponent<movimiento>();
        movimientoScript.groundLayer = LayerMask.GetMask(LayerMask.LayerToName(groundLayerID));
    }

    [TearDown]
    public void Teardown()
    {
        //Destruye los objetos para que cada test empiece de 0
        Object.Destroy(player);
        Object.Destroy(ground);
    }

    //QA01: Test para verificar que el jugador se desplaza al mantener presionado el botón de movimiento
    [UnityTest]
    public IEnumerator QA01_Movimiento_MantenerBoton_DesplazaObjeto()
    {
        Vector2 initialPosition = player.transform.position;
        movimientoScript.testHorizontalInput = 1f; // Simula mantener presionado el botón de movimiento hacia la derecha
        yield return new WaitForSeconds(1f); // Espera 1 segundo
        
        Assert.Greater(player.transform.position.x, initialPosition.x, "El jugador no se ha desplazado hacia la derecha al mantener presionado el botón de movimiento.");
    }

    //QA02: Test para verificar que el salto se corta al soltar el botón de salto
    [UnityTest]
    public IEnumerator QA02_Salto_CortarSalto_ReduceAlturaMaxima()
    {
        // Salto alto
        movimientoScript.testJumpInputDown = true;
        yield return new WaitForFixedUpdate(); // Frame 1: Recibe input
        movimientoScript.testJumpInputDown = false;
        
        yield return new WaitForSeconds(0.4f); //Esperamos a que llegue a la altura máxima
        float maxJump = player.transform.position.y; //Guardamos esa altura
        
        // Reseteamos el jugador al suelo para el siguiente salto
        player.transform.position = new Vector3(0,0,0);
        rb.linearVelocity = new Vector2(0,0);
        yield return new WaitForSeconds(0.2f); // Dejar que se estabilice en el suelo

        // Salto corto
        movimientoScript.testJumpInputDown = true; //Apretamos boton
        yield return new WaitForFixedUpdate();
        movimientoScript.testJumpInputDown = false;
        
        yield return new WaitForSeconds(0.1f);  //Esperamos poco tiempo
        movimientoScript.testJumpInputUp = true; //Soltamos boton
        yield return new WaitForFixedUpdate();
        movimientoScript.testJumpInputUp = false;

        // Esperamos el mismo tiempo total
        yield return new WaitForSeconds(0.3f);
        float notMaxJump = player.transform.position.y;

        // Assert: El salto donde soltamos el botón debe ser considerablemente más bajo
        Assert.Less(notMaxJump, maxJump, "El multiplicador de corte de salto no funcionó. El salto no fue más bajo al soltar el botón de salto antes de alcanzar la altura máxima.");
    }

    //QA04: Test para verificar que el jugador acumula inercia al balancearse en la telaraña
    [UnityTest]
    public IEnumerator QA04_Inercia_BalancearseEnTelarana_AceleraJugador()
    {
        // Elevamos al jugador y lo colgamos del techo
        player.transform.position = new Vector3(0, 0, 0);
        rb.linearVelocity = Vector2.zero;

        // Configuramos la cuerda
        distanceJoint.connectedAnchor = new Vector2(0, 5f);
        distanceJoint.distance = 5f;
        distanceJoint.enabled = true;

        // Simulamos input a la derecha mientras está colgado
        movimientoScript.testHorizontalInput = 1f;

        // Esperamos a que el jugador se balancee y acumule inercia
        yield return new WaitForSeconds(0.5f);

        // Assert: La velocidad horizontal debe ser mayor a 0, confirmando el impulso
        Assert.Greater(rb.linearVelocity.x, 0.5f, "El jugador no acumuló inercia al balancearse en la telaraña.");
    }

}