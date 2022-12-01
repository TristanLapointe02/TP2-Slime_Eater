using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ComportementJoueur : MonoBehaviour
{
    //VIE
    public float vieJoueur; //Vie du joueur
    public float vieMax; //Vie max du joueur
    public Slider sliderVie; //Slider de barre de vie
    public TextMeshProUGUI texteVie; //Ref au texte de vie

    //XP
    public int levelActuel; //Ref au niveau actuel du joueur
    public float xpActuel; //Xp actuel du joueur
    public float xpMax; //Xp max du niveau actuel
    public float xpMaxLvl1; //Xp max de d�part
    public Slider sliderXp; //Slider de barre de vie
    public TextMeshProUGUI texteXp; //Ref au texte de vie
    public TextMeshProUGUI texteLevelActuel; //Texte du level actuel du joueur

    //AUTRES
    public bool invulnerable; //Determine si le joueur est invuln�rable ou non
    public static bool mortJoueur; //Detecte si nous sommes mort ou non
    public GameObject menuFin; //Reference au menu de fin
    public AudioClip sonHit; //Son lorsque le joueur prend des degats
    public static float ennemisTues; //Nombre d'ennemis tues
    public static bool finJeu; //Indiquer que c'est la fin du jeu
    public AudioClip sonLevelUp; //Son lorsque le joueur level up

    void Start()
    {
        //Assigner quelques valeurs
        vieJoueur = vieMax;
        xpMax = xpMaxLvl1;
        xpActuel = 0;
        ennemisTues = 0;
        finJeu = false;
    }

    void Update()
    {
        //Mettre a jour la valeur du slider de vie
        float fillValueHp = vieJoueur / vieMax;
        sliderVie.value = fillValueHp;

        //Mettre a jour le texte de vie
        texteVie.text = Mathf.RoundToInt(vieJoueur).ToString();

        //Mettre a jour la valeuyr du slider d'xp
        float fillValueXp = xpActuel / xpMax;
        sliderXp.value = fillValueXp;

        //Mettre a jour le texte d'xp
        texteXp.text = Mathf.RoundToInt(xpActuel).ToString() + " / " + Mathf.RoundToInt(xpMax).ToString();
        texteLevelActuel.text = levelActuel.ToString();

        //TEST, PRENDRE DEGATS
        if (Input.GetKeyDown(KeyCode.K))
        {
            TakeDamage(10f);
        }

        //TEST, AUGMENTER XP
        if (Input.GetKeyDown(KeyCode.M))
        {
            AugmenterXp(5);
        }
    }

    //Fonction permettant au joueur de prendre des d�g�ts
    public void TakeDamage(float valeurDegat)
    {
        //Jouer un sound effect
        GetComponent<AudioSource>().PlayOneShot(sonHit);

        //Enlever de la vie au joueur
        if(vieJoueur > 0 && invulnerable == false)
        {
            vieJoueur -= valeurDegat;
        }

        //Si le joueur �tait pour mourir
        if(vieJoueur <= 0)
        {
            mortJoueur = true;

            //Appeler une fonction affichant le menu de fin
            FinJeu();
        }
    }

    //Fonction permettant de heal le joueur
    public void AugmenterVie(float valeurVie)
    {
        //Ajouter de la vie au joueur
        vieJoueur += valeurVie;

        //Si nous avons trop de vie
        if (vieJoueur > vieMax)
        {
            //La mettre a son maximum
            vieJoueur = vieMax;
        }
    }

    //Fonction permettant de grossir le joueur
    public void AugmenterGrosseur(float valeurGrosseur)
    {
        //Augmenter le scale du joueur
        if(transform.localScale.magnitude < 50)
        {
            transform.localScale += new Vector3(valeurGrosseur, valeurGrosseur, valeurGrosseur);
        }
    }

    //Fonction permettant d'augmenter l'xp du joueur
    public void AugmenterXp(float valeurXp)
    {
        //Augmenter l'xp actuel du joueur
        xpActuel += valeurXp;

        //Si jamais on d�passe l'xp max
        if(xpActuel >= xpMax)
        {
            //Trouver la difference si on depasse l'xp max
            float difference = xpActuel - xpMax;

            //Reset l'xp actuel
            xpActuel = 0;

            //Ajouter la difference
            if (difference > 0)
            {
                xpActuel += difference;
            }

            //Augmenter de niveau
            levelActuel++;

            //Augmenter l'xp max
            xpMax += xpMax / 3;

            //Jouer un sound effect
            GetComponent<AudioSource>().PlayOneShot(sonLevelUp);
        }
    }

    //Fonction permettant d'augmenter la vitesse du joueur
    public IEnumerator AugmenterVitesse(float valeur, float duree)
    {
        //Ajouter la vitesse
        GetComponent<ControleJoueur>().vitesse += valeur;

        //Attendre un certain delai
        yield return new WaitForSeconds(duree);

        //Enlever la vitesse
        GetComponent<ControleJoueur>().vitesse -= valeur;
    }

    //Fonction permettant d'augmenter les degats du joueur
    public IEnumerator AugmenterDegats(float valeur, float duree)
    {
        //Ajouter les degats
        GetComponent<ControleTir>().degatsJoueur += valeur;

        //Attendre un certain delai
        yield return new WaitForSeconds(duree);

        //Enlever les degats
        GetComponent<ControleTir>().degatsJoueur -= valeur;
    }

    //Fonction permettant d'augmenter la hauteur de saut du joueur
    public IEnumerator AugmenterSaut(float valeur, float duree)
    {
        //Ajouter un jump boost
        GetComponent<ControleJoueur>().forceSaut += valeur;

        //Attendre un certain delai
        yield return new WaitForSeconds(6);


        //Enlever la force de saut
        GetComponent<ControleJoueur>().forceSaut -= valeur;
    }

    //Fonction permettant d'augmenter la hauteur de saut du joueur
    public IEnumerator Invulnerabilite(float duree)
    {
        //Indiquer que le joueur est invulnerable
        GetComponent<ComportementJoueur>().invulnerable = true;

        //Attendre un certain delai
        yield return new WaitForSeconds(duree);

        //Enlever l'invulnerabilite
        GetComponent<ComportementJoueur>().invulnerable = false;
    }

    public void FinJeu()
    {
        //Faire apparaitre un menu
        menuFin.SetActive(true);

        //Indiquer que c'est la fin du jeu
        finJeu = true;
    }
}
