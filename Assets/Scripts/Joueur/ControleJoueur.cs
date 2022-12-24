using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Description : Gestion des inputs et actions du joueur
 * Fait par : Tristan Lapointe et Samuel S�guin
 */

public class ControleJoueur : MonoBehaviour
{
    [Header("Valeurs")]
    public float vitesse; //Vitesse du joueur
    public float dashVitesse; //Vitesse du dash
    public float dashTimer; //Timer du dash
    public float dashCooldown; //Cooldown du dash
    public float forceSaut; //Force de saut du joueur
    int jumpCounter; //Compteur du jump
    public int maxJump; //Nombre maximum de sauts du joueur
    public float degatsZone; //D�g�ts de la zone de saut
    public float forceExplosionInitiale; //Force d'explosion initiale
    public float forceExplosion; //Force de l'explosion de zone
    public float multiplicateurForceExplosion; //Par combien multiplier la force d'explosion selon la taille du joueur
    public float tailleDash; //Taille a reg�n�rer pour l'amelioration de dash
    float xInput; //Inputs sur l'axe des x
    float zInput; //Inputs sur l'axe des z

    [Header("Sons")]
    public AudioClip sonJump; //Son lorsque le joueur saute
    public AudioClip sonAtterir; //Son lorsque le joueur atterit
    public AudioClip sonMangerSlime; //Son lorsque le joueur mange un item slime

    [Header("References")]
    Rigidbody rb; //R�f�rence au Rigidbody du joueur
    public ZoneDegats zone; //R�f�rence � la zone de d�g�ts du joueur

    //AUTRES
    bool fixJump; //Bool permettant de fix le jump

    //MEILLEURE GESTION DU SAUT
    public float fallMultiplier; //Multiplicateur de la force de gravit�

    void Start()
    {
        //Assigner les r�f�rences
        rb = GetComponent<Rigidbody>();

        //Reset des valeurs
        jumpCounter = maxJump;
        forceExplosion = forceExplosionInitiale;

        //Montrer la zone d'explosion
        zone.gameObject.GetComponent<MeshRenderer>().enabled = true;
    }

    
    void Update()
    {
        //Am�lioration de gravit�
        if(rb.velocity.y < 0)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }

        //Capturer les inputs
        if(ComportementJoueur.finJeu == false && ControleAmeliorations.pause == false && ControleMenu.pauseMenu == false)
        {
            InputProcess();
        }

        //Si on est en pause ou fin du jeu, freeze le rigidbody
        if(ControleAmeliorations.pause || ControleMenu.pauseMenu || ComportementJoueur.finJeu)
        {
            //Enlever la v�locit� du joueur
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        }
        else
        {
            //Sinon, tout est beau
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        }

        //Gestion des inputs de menu (ne sont pas dans InputProcess parce qu'ils ne sont pas affect� par pause ou fin du jeu) : 
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

        //Si le joueur appuie sur escape
        if (Input.GetButtonDown("Cancel"))
        {
            //Ouvrir/fermer menu pause
            GetComponent<ControleMenu>().MenuPause();
        }
    }

    //Fonction permettant de recevoir les inputs
    private void InputProcess()
    {
        //Recevoir l'input de WASD ou des touches directionnelles 
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
            //R�initialiser le timer du dash
            dashTimer = 0;

            //Si on ne re�oit aucun input directionnel
            if (xInput == 0 && zInput == 0)
            {
                //Dash dans la direction que l'on vise avec la souris
                Move(dashVitesse, GetComponent<ControleTir>().gun.transform.forward);
            }
            //Si on re�oit un input directionnel
            else
            {
                //Dash dans la direction du input
                Move(dashVitesse, new Vector3(xInput, 0, zInput));
            }
            
            //Reg�n�rer de la masse s'il y a lieu
            if(tailleDash > 0)
            {
                GetComponent<ComportementJoueur>().AugmenterGrosseur(tailleDash);
            }
        }

        //R�initialiser le compteur de jump si on touche le sol
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

            //R�initialiser l'explosion
            zone.plusGrandeDistance = 0;

            //D�sactiver les visuels de la zone
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

    //Fonction permettant de verifier si le joueur touche au sol
    bool isGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, GetComponent<Collider>().bounds.extents.y, 10);
    }

    //Fonction permettant de r�initialiser le jump
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
                //Leur faire subir une explosion
                ennemy.SubirExplosion(forceExplosion, new Vector3(transform.position.x, transform.position.y - GetComponent<Collider>().bounds.extents.y, transform.position.z), zone.rayonActuel / 2, degatsZone);   
            }
        }
    }
}
