using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class WebLauncherTest
{
    private GameObject player;
    private WebLauncher launcherScript;
    private SpiderWeb spiderWebScript;
    private DistanceJoint2D distanceJoint;

    [SetUp]
    public void Setup()
    {
        //Jugador
        player= new GameObject("Player");
        player.transform.position = new Vector3(0, 0, 0);

        player.AddComponent<Rigidbody2D>();
        player.AddComponent<BoxCollider2D>();
        player.AddComponent<movimiento>();

        distanceJoint = player.AddComponent<DistanceJoint2D>();
        distanceJoint.enabled = false;

        //Telaraña
        player.AddComponent<LineRenderer>();
        spiderWebScript = player.AddComponent<SpiderWeb>();
        spiderWebScript.playerTransform = player.transform;

        //Disparador de telaraña
        launcherScript = player.AddComponent<WebLauncher>();
        launcherScript.telarana = spiderWebScript;
        
        int adheribleLayerID = LayerMask.NameToLayer("Enganchable");
        if (adheribleLayerID == -1)
        {
            adheribleLayerID = 0;
        }
        launcherScript.capasEnganchables = LayerMask.GetMask(LayerMask.LayerToName(adheribleLayerID));
        launcherScript.testMode = true;

    }

    [TearDown]
    public void Teardown()
    {
        Object.Destroy(player);
    }

    //QA03: Verificar que al disparar la telaraña hacia una superficie adherible, se genera un péndulo. 
    [UnityTest]
    public IEnumerator QA03_Telarana_SuperficieAdherible_GeneraPendulo()
    {
        //Pared con capa ADHERIBLE
        GameObject adherible = new GameObject("Adherible");
        adherible.transform.position = new Vector3(5f, 5f, 0);
        BoxCollider2D adheribleCollider = adherible.AddComponent<BoxCollider2D>();
        adheribleCollider.size = new Vector2(2f, 2f);
        adherible.layer = LayerMask.NameToLayer("Enganchable");

        //Apuntar hacia la pared y disparar la telaraña
        launcherScript.testMousePosition = new Vector2(5f, 5f);
        launcherScript.IntentarEnganche();

        yield return new WaitForSeconds(0.5f); // Esperar a que la telaraña se conecte

        Assert.IsTrue(distanceJoint.enabled, "La telaraña NO se ha enganchado a una superficie que sí era adherible.");

        Object.Destroy(adherible);
    }

    //QA05: Lanzar la telaraña hacia una superficie no adherible y verificar que no se genera un péndulo.
    [UnityTest]
    public IEnumerator QA05_Telarana_SuperficieNoAdherible_FallaEnganche()
    {
        //Pared con capa DEFAULT (no adherible)
        GameObject noAdherible = new GameObject("NoAdherible");
        noAdherible.transform.position = new Vector3(5f, 5f, 0);
        BoxCollider2D noAdheribleCollider = noAdherible.AddComponent<BoxCollider2D>();
        noAdheribleCollider.size = new Vector2(2f, 2f);
        noAdherible.layer = LayerMask.NameToLayer("Default");

        //Apuntar hacia la pared y disparar la telaraña
        launcherScript.testMousePosition = new Vector2(5f, 5f);
        launcherScript.IntentarEnganche();

        yield return new WaitForSeconds(0.5f); // Esperar a que la telaraña intente engancharse

        Assert.IsFalse(distanceJoint.enabled, "La telaraña se ha enganchado a una superficie que NO era adherible.");

        Object.Destroy(noAdherible);
    }
}