using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerBanditController : MonoBehaviour
{
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private LayerMask explosionMask;
    [SerializeField] private LayerMask finish;
    [SerializeField] private GameObject bomb;
    [SerializeField] private GameObject explosion;
    [SerializeField] private int bombTimer = 3;
    [SerializeField] private int explodeTime = 1;
    private bool isMovement;

    private void Update()
    {
        if (isMovement)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            MovePlayerTo(Vector2.left);
        }
        
        if (Input.GetKeyDown(KeyCode.D))
        {
            MovePlayerTo(Vector2.right);
        }
        
        if (Input.GetKeyDown(KeyCode.W))
        {
            MovePlayerTo(Vector2.up);
        }
        
        if (Input.GetKeyDown(KeyCode.S))
        {
            MovePlayerTo(Vector2.down);
        }
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var createdBomb = Instantiate(bomb, gameObject.transform.position, Quaternion.identity);
            StartCoroutine(CountdownForBomb(bombTimer, createdBomb));
        }
    }

    private void MovePlayerTo(Vector2 dir)
    {
        if (Raycast(dir))
        {
            return;
        }

        if (RaycastFinish(dir))
        {
            SceneManager.LoadScene(0);
        }

        isMovement = true;

        var pos = (Vector2) transform.position + dir;
        transform.DOMove(pos, 0.5f).OnComplete(() => isMovement = false);
    }

    private bool Raycast(Vector2 dir)
    {
        var hit = Physics2D.Raycast(transform.position, dir, 1f, _layerMask);
        return hit.collider != null;
    }

    private bool RaycastFinish(Vector2 dir)
    {
        var hit = Physics2D.Raycast(transform.position, dir, 1f, finish);
        return hit.collider != null;
    }
    
    private bool RaycastFromObject(Vector2 dir, GameObject obj)
    {
        var hit = Physics2D.Raycast(obj.transform.position, dir, 1f, _layerMask);
        return hit.collider != null;
    }

    private void ExplodeBomb(GameObject createdBomb)
    {
        Destroy(createdBomb);
                
        var colliders = Physics2D.OverlapCircleAll(createdBomb.transform.position, 1f, explosionMask);

        foreach (var cldr in colliders)
        {
            Destroy(cldr.gameObject);
            if (cldr.gameObject.name.Equals("Player"))
            {
                SceneManager.LoadScene(0);
            }
        }

        Explosion(createdBomb);
    }

    private void Explosion(GameObject createdBomb)
    {
        List<GameObject> explosions = new List<GameObject>();
        Vector2 pos = createdBomb.transform.position;
        explosions.Add(Instantiate(explosion, pos, Quaternion.identity));
        
        if (!RaycastFromObject(Vector2.down, createdBomb))
        {
            explosions.Add(Instantiate(explosion, pos + Vector2.down , Quaternion.identity));
        }
        
        if (!RaycastFromObject(Vector2.up, createdBomb))
        {
            explosions.Add(Instantiate(explosion, pos + Vector2.up , Quaternion.identity));
        }
        
        if (!RaycastFromObject(Vector2.left, createdBomb))
        {
            explosions.Add(Instantiate(explosion, pos + Vector2.left , Quaternion.identity));
        }
        
        if (!RaycastFromObject(Vector2.right, createdBomb))
        {
            explosions.Add(Instantiate(explosion, pos + Vector2.right , Quaternion.identity));
        }

        foreach (var ex in explosions)
        {
            StartCoroutine(CountdownForExplosion(explodeTime, ex));
        }
    }
    
    IEnumerator CountdownForBomb(int seconds, GameObject createdBomb) {
        int counter = seconds;
        while (counter > 0) {
            yield return new WaitForSeconds (1);
            counter--;
        }
        ExplodeBomb(createdBomb);
    }
    
    IEnumerator CountdownForExplosion(int seconds, GameObject explosionToDestroy) {
        int counter = seconds;
        while (counter > 0) {
            yield return new WaitForSeconds (1);
            counter--;
        }
        Destroy(explosionToDestroy);
    }
}
