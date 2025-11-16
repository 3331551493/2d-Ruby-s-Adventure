using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demageable : MonoBehaviour
{
    public float damageCooldown = 0.2f; // 伤害冷却时间
    private float lastDamageTime;       // 上次造成伤害的时间

    void OnTriggerStay2D(Collider2D other)
    {
        RubyController controller = other.GetComponent<RubyController>();

        if (controller != null)
        {
            // 检查是否已经过了冷却时间
            if (Time.time >= lastDamageTime + damageCooldown)
            {
                controller.ChangeHealth(-1);
                lastDamageTime = Time.time; // 更新上次伤害时间
            }
        }
    }
}
