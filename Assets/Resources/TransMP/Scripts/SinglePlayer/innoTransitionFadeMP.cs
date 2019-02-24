using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class innoTransitionFadeMP : MonoBehaviour
{
    // Components
    public static innoTransitionFadeMP instance { get; private set; }
    private SpriteRenderer sr;

    // Settings
    [Header("Transition Settings")]
    public float wait_to_transition;
    public float transition_speed;

    [Header("Lobby Objects")]
    public GameObject[] lobby_objs;

    // Variables
    private bool canvas_gone;
    private float timer;
    private float alpha;

    // Start is called before the first frame update
    void Awake()
    {
        if (GameObject.Find("inno_transition_transmp") != null) {
            Destroy(gameObject);
        }
        else {
            gameObject.name = "inno_transition_transmp";
            instance = this;
        }

        canvas_gone = false;
        timer = wait_to_transition;
        alpha = 0f;
        sr = GetComponent<SpriteRenderer>();
        sr.color = new Color(1f, 1f, 1f, alpha);
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0) {
            if (!canvas_gone) {
                if (GameObject.Find("Canvas") != null) {
                    Destroy(GameObject.Find("Canvas"));
                }
                canvas_gone = true;
            }

            alpha = Mathf.Lerp(alpha, 1f, transition_speed * Time.deltaTime);

            if (alpha >= 0.995f) {
                transitionAction();
                Destroy(gameObject);
            }
        }

        sr.color = new Color(1f, 1f, 1f, alpha);
    }

    public void transitionAction() {
        Scene current_scene = SceneManager.GetActiveScene();
        Scene lobby_scene = SceneManager.CreateScene("MultiplayerLobby");

        for (int i = 0; i < lobby_objs.Length; i++) {
            string name = lobby_objs[i].name;
            Vector3 position = lobby_objs[i].transform.position;
            Vector3 scale = lobby_objs[i].transform.localScale;

            GameObject lobby_object = Instantiate(lobby_objs[i]);
            lobby_object.name = name;
            lobby_object.transform.position = position;
            lobby_object.transform.localScale = scale;
            SceneManager.MoveGameObjectToScene(lobby_object, lobby_scene);
        }

        SceneManager.UnloadSceneAsync(current_scene);
    }
}
