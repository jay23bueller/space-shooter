using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    #region Variables
    [SerializeField]
    protected float _speed = 8.0f;
    protected bool _canMove;
    protected bool _isEnemyWeapon;
    [SerializeField]
    private GameObject _weaponHitPrefab;
    [SerializeField]
    private AudioClip _disruptedShotClip;
    [SerializeField]
    private Color _originalHitColor;
    [SerializeField]
    private Color _disruptedHitColor;
    
    #endregion

    #region UnityMethods
    // Update is called once per frame
    void Update()
    {
        Move();
    }

    private void OnDestroy()
    {
        //TripleShot
        if (transform.parent != null)
            Destroy(transform.parent.gameObject);
 
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        CollisionCheck(ref collision);
    }
    #endregion

    #region Methods
    protected void CollisionCheck(ref Collider2D collision)
    {
        if (collision != null)
        {
            bool destroySelf = false;
            if (collision.CompareTag("Player") && _isEnemyWeapon)
            {
                collision.GetComponent<Player>().UpdateLives(-1);
                destroySelf = true;
            }

            if(collision.CompareTag("Enemy") && !_isEnemyWeapon)
            {
                collision.GetComponent<Enemy>().GetDestroyed(true);
                destroySelf = true;
            }

            if (collision.CompareTag("Asteroid"))
            {
                collision.GetComponent<Asteroid>().InitiateGame();
                destroySelf = true;
            }

            if(collision.CompareTag("Powerup") && _isEnemyWeapon)
            {
                collision.GetComponent<Powerup>().GetDestroyed();
                destroySelf = true;
            }

            if (destroySelf)
            {
                GetComponent<Collider2D>().enabled = false;
                OnHit();
            }
                

            
        }
    }

    private void OnHit()
    {
        Instantiate(_weaponHitPrefab, transform.position, transform.rotation);
        Destroy(gameObject);
    }


    // Move laser till it's out of the viewport
    // at the moment, it is assumed to be moving to the top
    protected virtual void Move()
    {
        if (_canMove)
        {

            if (transform.position.y < GameManager.ENVIRONMENT_TOP_BOUND && transform.position.y > GameManager.ENVIRONMENT_BOTTOM_BOUND - 1f)
                transform.Translate(transform.up * _speed * Time.deltaTime, Space.World);
            else
                Destroy(gameObject);
        }

    }

    public virtual void InitializeFiring(int owner, bool disrupted)
    {
        switch(owner)
        {
            case 0:
                _isEnemyWeapon = true;
                tag = "EnemyLaser";
                break;
            case 1:
                tag = "Laser";
                gameObject.layer = LayerMask.NameToLayer("Laser");
                break;
            default:
                Debug.LogError("Incorrect value for InitializingFire!");
                break;
        }

        
        ParticleSystem.MainModule main = _weaponHitPrefab.GetComponent<ParticleSystem>().main;
        main.startColor = disrupted ? _disruptedHitColor : _originalHitColor;

        if (disrupted)
        {
            StartCoroutine(SelfDestructRoutine());
        }
        GetComponent<Collider2D>().enabled = true;

        _canMove = true;
    }

    private IEnumerator SelfDestructRoutine()
    {
        GetComponent<SpriteRenderer>().color = Color.HSVToRGB(.48f,1f,1f);
        yield return new WaitForSeconds(Random.Range(.1f, 1f));
        if(GetComponent<Collider2D>().enabled)
        {
            AudioSource.PlayClipAtPoint(_disruptedShotClip,Camera.main.transform.position);
            OnHit();
        }
            
    }
    #endregion
}
