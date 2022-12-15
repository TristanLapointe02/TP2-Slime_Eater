using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControleJoueur : MonoBehaviour
{
    [Header("Valeurs")]
    public float vitesse; //Vitesse du joueur
    public float dashVitesse; //Vitesse du dash
    public float dashTimer; //Timer du dash
    public float dashCooldown; //Cooldown du dash
    public float forceSaut; //Force de saut du joueur
    int jumpCounter; //Counter du jump
    public int maxJump; //Nombre de sauts maximals du joueur
    public float degatsZone; //Degats de la zone de saut
    public float forceExplosionInitiale; //Force explosion initiale
    public float forceExplosion; //Force de l'explosion de zone
    public float multiplicateurForceExplosion; //Par combien multiplier la force d'explosion selon la taille du joueur
    public float tailleDash; //Taille a reg�n�rer pour l'amelioration de dash
    float xInput; //Inputs sur l'axe des x
    float zInput; //Inputs sur l'axe des x

    [Header("Sons")]
    public AudioClip sonJump; //Son lorsque le joueur saute
    public AudioClip sonAtterir; //Son lorsque le joueur atterit
    public AudioClip sonMangerSlime; //Son lorsque le joueur mange un slime

    [Header("References")]
    Rigidbody rb; //Rigidbody du joueur
    public ZoneDegats zone; //Reference a la zone de degats du joueur

    //AUTRES
    bool fixJump; //Bool permettant de fix le jump

    //MEILLEURE GESTION DU SAUT
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    void Start()
    {
        //Assigner les r�f�rences
        rb = GetComponent<Rigidbody>();

        //Reset des valeurs
        jumpCounter = maxJump;
        forceExplosion = forceExplosionInitiale;

        //Montrer la zone
        zone.gameObject.GetComponent<MeshRenderer>().enabled = true;
    }

    
    void Update()
    {
        //TEST AM�LIORATION JUMP
        if(rb.velocity.y < 0)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        //Capturer les inputs
        if(ComportementJoueur.finJeu == false && ControleAmeliorations.pause == false)
        {
            InputProcess();
        }

        //Gestion des inputs de menu
        // Si on appuie sur Tab
        if (Input.GetButtonDown("Open Stats"))
        {
            // Afficher le menu de statistiques
            gameObject.GetComponent<ControleMenu>().OuvrirStatistiques();
        }
        // Si on rel�ches Tab
        if (Input.GetButtonUp("Open Stats"))
        {
            // Fermer le menu de statistiques
            gameObject.GetComponent<ControleMenu>().FermerStatistiques();
        }

        //Si jamais le joueur appuie sur escape
        if (Input.GetButtonDown("Cancel"))
        {
            //Ouvrir/fermer menu pause
            gameObject.GetComponent<ControleMenu>().MenuPause();
        }
    }

    //Fonction permettant de recevoir les inputs
    private void InputProcess()
    {
        xInput = Input.GetAxis("Horizontal");
        zInput = Input.GetAxis("Vertical");

        //Si on appuie sur espace
        if (Input.GetButtonDown("Jump") && jumpCounter > 0)
        {
            //Faire sauter le joueur
            rb.AddForce(Vector3.up * forceSaut * 1000f);

            //Jouer le son de saut
            GetComponent<AudioSource>().PlayOneShot(sonJump);

            //Indiquer que nous venons de sauter
            fixJump = true;

            //Diminuer le nombre de jumps
            jumpCounter--;

            //Permettre au joueur de sauter apr�s un petit delai
            Invoke("ResetJump", 0.15f);

            //Montrer la zone
            zone.gameObject.GetComponent<MeshRenderer>().enabled = true;
        }

        // Si on appuie sur left shift
        if (Input.GetButtonDown("Fire3") && dashTimer >= dashCooldown)
        {
            dashTimer = 0;
            Move(dashVitesse, GetComponent<ControleTir>().gun.transform.forward);

            //Reg�n�rer de la masse s'il y a lieu
            if(tailleDash > 0)
            {
                GetComponent<ComportementJoueur>().AugmenterGrosseur(tailleDash);
            }
        }

        //Reset jump counter
        if (isGrounded() && fixJump == false)
        {
            jumpCounter = maxJump;
        }
    }

    //Pour le mouvement, c'est mieux d'utiliser fixedUpdate
    private void FixedUpdate()
    {
        // Incr�menter le timer pour le cooldown du dash
        if (dashTimer < dashCooldown)
        {
            dashTimer += Time.deltaTime;
        }

        //Appeler la fonction de mouvement
        Move(vitesse, new Vector3(xInput, 0, zInput));
    }

    //Fonction de collision
    private void OnCollisionEnter(Collision collision)
    {
        //Lorsque le joueur atterit sur le sol
        if(collision.gameObject.tag == "Sol" && zone.plusGrandeDistance - GetComponent<Collider>().bounds.extents.y >= 2f)
        {
            //Jouer un son
            GetComponent<AudioSource>().PlayOneShot(sonAtterir);

            //Appeler la fonction pour faire des d�g�ts aux ennemis
            Explosion(forceExplosion, degatsZone);

            //Reset l'explosion
            zone.plusGrandeDistance = 0;

            //Disable les visuels de la zone
            zone.gameObject.GetComponent<MeshRenderer>().enabled = false;
        }
    }


    //Fonction permettant de bouger le joueur
    private void Move(float vitesse, Vector3 direction)
    {
        //Ajouter de la force � la balle
        Vector3 deplacement = new Vector3(direction.x, 0f, direction.z);
        rb.AddForce(deplacement.normalized * vitesse);
    }

    //Fonction permettant de verifier si le joueur touche le sol
    bool isGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, GetComponent<Collider>().bounds.extents.y, 10);
    }

    //Fonction permettant de reset le jump
    public void ResetJump()
    {
        fixJump = false;
    }

    //Fonction permettant de faire l'explosion
    public void Explosion(float forceExplosion, float degatsZone)
    {
        Collider[] hitColliders = Physics.OverlapSphere(new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - GetComponent<Collider>().bounds.extents.y, gameObject.transform.position.z), zone.rayonActuel / 2);

        //Pour tous les collider touch�s
        foreach(var collider in hitColliders)
        {
            //Trouver les ennemis
            if(collider.gameObject.TryGetComponent(out EnemyController ennemy))
            {
                //Leur faire des degats
                ennemy.TakeDamage(degatsZone);

                //Faire une explosion
                ennemy.GetComponent<Rigidbody>().AddExplosionForce(forceExplosion, new Vector3(transform.position.x, transform.position.y - GetComponent<Collider>().bounds.extents.y, transform.position.z), zone.rayonActuel / 2);
            }
        }
    }


    //TEST POUR VOIR LA ZONE DE DEGATS
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        //Draw a cube where the OverlapBox is (positioned where your GameObject is as well as a size)
        Gizmos.DrawWireSphere(transform.position, zone.rayonActuel/2);
    }
}
