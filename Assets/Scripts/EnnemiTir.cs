using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class EnnemiTir : MonoBehaviour
{

    [Header("Balle")]
    public Balle balle; //Balle tir�e

    [Header("Valeurs de tir")]
    bool peutTirer = true; //Bool permettant de savoir si l'ennemi peut tirer/cooldown
    public float shootCooldown; //Cooldown de tir
    public float shootDelay; //D�lai entre les balles tir�es
    public int nombreBalles; //Nombre de balles a tirer
    public float forceTir; //Force du tir

    //VARIABLE DE TEST
    public bool forceStopTir; //Force le stop du tir

    [Header("Autres r�f�rences")]
    public AudioClip sonTir; //Son de tir
    public GameObject gun; //Position du "gun"

    void Update()
    {
        //Faire que le gun face le joueur
        gun.transform.LookAt(GetComponent<EnemyController>().joueur.transform);

        //S'il peut tirer et qu'il est in range
        if (peutTirer && ControleAmeliorations.pause == false && ControleMenu.pauseMenu == false && GetComponent<EnemyController>().InRangeJoueur() && forceStopTir == false)
        {
            //Commencer le cooldown apr�s avoir tirer
            peutTirer = false;
            StartCoroutine(delaiTir(shootCooldown));

            // Jouer un son de tir
            GetComponent<AudioSource>().PlayOneShot(sonTir);

            //Tirer une balle vers l'avant
            StartCoroutine(tirBalleEnnemie(nombreBalles, shootDelay, gun.transform.forward));
        }
    }

    //Fonction de cooldown du tir
    public IEnumerator delaiTir(float delai)
    {
        yield return new WaitForSeconds(delai);
        peutTirer = true;
    }

    public IEnumerator tirBalleEnnemie(int nbBalles, float delai, Vector3 direction)
    {
        //Pour le nombre de balles a tirer
        for (int i = 0; i < nombreBalles; i++)
        {
            //Jouer un son de tir
            GetComponent<AudioSource>().PlayOneShot(sonTir);

            //Instancier une balle
            Balle nouvelleBalle = Instantiate(balle, gun.transform.position, gun.transform.rotation);

            //Propulser la balle vers la direction du joueur
            nouvelleBalle.GetComponent<Rigidbody>().AddForce(gun.transform.forward * forceTir, ForceMode.Impulse);

            //Changer les degats de la balle selon ceux du joueur
            nouvelleBalle.GetComponent<Balle>().degats = GetComponent<EnemyController>().enemy.degats;

            //Delai pour la prochaine balle
            yield return new WaitForSeconds(delai);
        }
    }
}
