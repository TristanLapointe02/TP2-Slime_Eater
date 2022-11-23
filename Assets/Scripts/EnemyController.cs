using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class EnemyController : MonoBehaviour
{
    public StatsEnemy enemy; // type d'ennemi
    public GameObject joueur; // Référence au joueur
    private Vector3 directionJoueur; // distance et direction entre ennemi et joueur
    public float vie; //Vie de l'ennemi
    public GameObject slimeLoot; //Reference au loot de slime
    public AudioClip sonSuction; //Son de suction lorsque l'ennemi touche le joueur

    private void Start()
    {
        //Assigner les valeurs de maniere dynamique selon les stats
        //Vie
        vie = enemy.vieMax;

        //Taille
        gameObject.transform.localScale =  new Vector3(enemy.tailleEnnemi, enemy.tailleEnnemi, enemy.tailleEnnemi);

        //Couleur
        AppliquerMat();


        //Trouver le joueur lorsqu'on est spawn
        joueur = GameObject.FindGameObjectWithTag("Player");

    }

    private void FixedUpdate()
    {
        //Obtenir la distance et direction avec joueur
        directionJoueur = joueur.transform.position - transform.position; 

        //Si l'ennemi n'est pas ranged
        if (enemy.ranged == false)
        {
            //Appeler la fonction pour bouger normalement
            Move();
        }

        //Si l'ennemi est ranged
        if (enemy.ranged == true)
        {
            // Si le joueur est assez proche, l'attaquer
            if (directionJoueur.magnitude <= 25f + joueur.GetComponent<Collider>().bounds.size.x)
            {
                //Attack();
            }
            // Sinon bouger vers lui
            else
            {
                Move();
            }
        }
    }

    //Fonction permettant de bouger vers le joueur
    private void Move()
    {
        //Modifier la position de l'ennemi selon la vitesse
        transform.position += directionJoueur.normalized * enemy.vitesse * Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Lorsqu'on collide avec le joueur
        if (collision.gameObject.tag == "Player")
        {
            // Faire perdre de la vie au joueur
            collision.gameObject.GetComponent<ComportementJoueur>().TakeDamage(enemy.degats);

            //Ajouter animation de mort éventuellement

            //Faire jouer un son
            collision.gameObject.GetComponent<AudioSource>().PlayOneShot(sonSuction);

            //Mourir
            MortEnnemi();
        }
    }

    //Fonction qui fait perdre de la vie a l'ennemi
    public void TakeDamage(float damage)
    {
        //Diminuer la vie
        vie -= damage;

        //Si la vie tombe a 0
        if (vie <= 0)
        {
            //Mourir
            MortEnnemi();
        }

        //Changer le matériel pendant 0.15 secondes
        GetComponent<MeshRenderer>().material.color = Color.red;
        Invoke("AppliquerMat", 0.15f);
    }

    //Fonction de mort de l'ennemi
    public void MortEnnemi()
    {
        //Spawn du loot selon le nombre a spawn
        for (int i = 0; i < enemy.nombreLootSpawn; i++)
        {
            //Spawn le loot
            GameObject loot = Instantiate(slimeLoot, transform.position, Quaternion.identity);

            //Changer la valeur du loot selon celle max qui faut donner
            loot.GetComponent<SlimeLoot>().sizeValue = Random.Range(0, enemy.valeurLoot);
        }

        //Indiquer qu'on est mort
        ComportementJoueur.ennemisTues++;

        //Se detruire
        Destroy(gameObject);
    }

    //Fonction permettant de remettre a la normale le matériel de l'ennemi
    public void AppliquerMat()
    {
        gameObject.GetComponent<MeshRenderer>().material.color = enemy.couleur;
    }
}
