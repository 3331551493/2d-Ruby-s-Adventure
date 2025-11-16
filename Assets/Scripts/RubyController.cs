using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RubyController : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource walkAudioSource;
    [SerializeField] private AudioSource effectAudioSource;

    public AudioClip lunchSound;
    public AudioClip hitSound;

    public AudioClip walkSound;

    public int maxHealth = 5;
    public int health { get { return currentHealth; } }
    int currentHealth;
    public float timeInvincible = 2.0f;

    public float speed = 3.0f;

    bool isInvincible;
    float invincibleTimer;

    Rigidbody2D rigidbody2d;
    Animator animator;
    Vector2 lookDirection = new Vector2(1, 0);
    float horizontal;
    float vertical;

    public GameObject projectilePrefab;
    public ParticleSystem buffEffect;

    private bool isMoving = false;

    // 在第一次帧更新之前调用 Start
    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        currentHealth = maxHealth;

        walkAudioSource.clip = walkSound;


        //effectAudioSource = GetComponent<AudioSource>();
        //isInvincible = false;
    }

    // 每帧调用一次 Update
    void Update()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        Vector2 move = new Vector2(horizontal, vertical);

        // 检测是否在移动
        bool wasMoving = isMoving;
        isMoving = !Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y, 0.0f);

        // 控制走路声音
        HandleWalkSound(wasMoving, isMoving);

        //Mathf.Approximately()：比较浮点数，相近时为真
        if (!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y, 0.0f))
        {
            lookDirection.Set(move.x, move.y);
            lookDirection.Normalize();
        }

        animator.SetFloat("Look X", lookDirection.x);
        animator.SetFloat("Look Y", lookDirection.y);

        /**
         * magnitude	向量的实际长度	√(x² + y²)
         * sqrMagnitude	向量长度的平方	x² + y²
         * normalized	单位向量（方向相同，长度为1）	vector / magnitude
         */
        animator.SetFloat("Speed", move.magnitude);//获取二维向量的长度作为速度

        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if(invincibleTimer < 0)
            {
                isInvincible = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            Launch();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            RaycastHit2D hit = Physics2D.Raycast(rigidbody2d.position + Vector2.up * 0.2f, lookDirection, 1.5f, LayerMask.GetMask("NPC"));
            if (hit.collider != null)
            {
                NonPlayerCharacter character = hit.collider.GetComponent<NonPlayerCharacter>();
                if (character != null)
                {
                    character.DisplayDialog();
                }
            }
        }
    }

    void FixedUpdate()
    {
        Vector2 position = rigidbody2d.position;
        position.x = position.x + speed * horizontal * Time.deltaTime;
        position.y = position.y + speed * vertical * Time.deltaTime;

        rigidbody2d.MovePosition(position);
    }

    public void ChangeHealth(int amount)
    {
        if(amount < 0)
        {
            animator.SetTrigger("Hit");

            if (isInvincible)
                return;
            isInvincible = true;
            invincibleTimer = timeInvincible;

            // 停止走路声音
            if (walkAudioSource != null && walkAudioSource.isPlaying)
            {
                walkAudioSource.Stop();
            }

            PlaySound(hitSound);
        }
        else
        {
            buffEffect.Play();
        }

        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        UIHealthBar.instance.SetValue(currentHealth / (float)maxHealth);
    }

    void Launch()
    {
        if (effectAudioSource != null && lunchSound != null)
        {
            // 停止走路声音
            if (walkAudioSource != null && walkAudioSource.isPlaying)
            {
                walkAudioSource.Stop();
            }

        }

        //在刚体位置偏上创建预制体副本，并且无旋转（ Quaternion.identity）
        GameObject projectileObject = Instantiate(projectilePrefab, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);

        Projectile projectile = projectileObject.GetComponent<Projectile>();
        projectile.Launch(lookDirection, 300);

        animator.SetTrigger("Launch");

        // 播放发射音效
        PlaySound(lunchSound);
    }

    public void PlaySound(AudioClip clip)
    {
        effectAudioSource.PlayOneShot(clip);
    }

    // 新增方法：控制走路声音
    private void HandleWalkSound(bool wasMoving, bool isMoving)
    {
        if (walkAudioSource == null) return;

        // 如果从静止变为移动，开始播放走路声音
        if (!wasMoving && isMoving)
        {
            walkAudioSource.Play();
        }
        // 如果从移动变为静止，停止播放走路声音
        else if (wasMoving && !isMoving)
        {
            walkAudioSource.Stop();
        }
    }
}
        