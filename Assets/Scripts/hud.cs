using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class hud : MonoBehaviour {

    private float cdMouse;
    private float cdM;
    public float cdShift;
    public float cdS;
    public float cdE;
    public float cde;

    private Color opaque = new Color32(0, 0, 0, 150);
    private Color noOpaque = new Color32(0, 0, 0, 0);

    private void Start()
    {
        cdM = 1F - (1F * client.yourLevel / 100F);
        cdMouse = cdM;
        cdS = 3F - (3F * client.yourLevel / 100F);
        cdShift = cdS;
        cde = 3F - (3F * client.yourLevel / 100F);
        cdE = cde;
    }

    void Update () {

        cdM = 1F - (1F * client.yourLevel / 100F);
        cdS = 3F - (3F * client.yourLevel / 100F); 
        cde = 3F - (3F * client.yourLevel / 100F);
        gameObject.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = client.yourLevel.ToString();

        if (goblin.mouseActivated) {
            gameObject.transform.GetChild(0).GetChild(2).GetComponent<Image>().color = opaque;
            gameObject.transform.GetChild(0).GetChild(1).GetComponent<Image>().fillAmount = cdMouse / cdM;
            cdMouse -= Time.deltaTime;
            if (cdMouse <= 0F) {
                gameObject.transform.GetChild(0).GetChild(2).GetComponent<Image>().color = noOpaque;
                gameObject.transform.GetChild(0).GetChild(1).GetComponent<Image>().fillAmount = 0;
                cdMouse = cdM;
                goblin.mouseActivated = false;
            }
        }

        if (goblin.eActivated) { 
            gameObject.transform.GetChild(0).GetChild(4).GetComponent<Image>().color = opaque;
            gameObject.transform.GetChild(0).GetChild(3).GetComponent<Image>().fillAmount = cdE / cde;
            cdE -= Time.deltaTime;
            if (cdE <= 0F) {
                gameObject.transform.GetChild(0).GetChild(4).GetComponent<Image>().color = noOpaque;
                gameObject.transform.GetChild(0).GetChild(3).GetComponent<Image>().fillAmount = 0;
                cdE = cde;
                goblin.eActivated = false;
            }
        }

        if (goblin.shiftActivated) {
            gameObject.transform.GetChild(0).GetChild(6).GetComponent<Image>().color = opaque;
            gameObject.transform.GetChild(0).GetChild(5).GetComponent<Image>().fillAmount = cdShift / cdS;
            cdShift -= Time.deltaTime;
            if (cdShift <= 0F) {
                gameObject.transform.GetChild(0).GetChild(6).GetComponent<Image>().color = noOpaque;
                gameObject.transform.GetChild(0).GetChild(5).GetComponent<Image>().fillAmount = 0;
                cdShift = cdS;
                goblin.shiftActivated = false;
            }
        }
    }
}
