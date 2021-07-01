﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatSystem : MonoBehaviour
{
    public CombatAttacks[] Attacks;
    public CombatAttacks currentAttack;

    float attackTime;
    float bonusSpeed;
    Rigidbody2D rb;
    public float attackRadius;
    BasePlayer playerChar;
    public Transform attackPoint;
    bool isAttacking;
    public LayerMask enemyLayer;

    void Start()
    {
        currentAttack = Attacks[0];
        rb = GetComponent<Rigidbody2D>();
        playerChar = GetComponent<BasePlayer>();

        setUpAttackLinks();
    }

    // Update is called once per frame
    void Update()
    {
        if (GamePause.gamePaused)
        {
            return;
        }
        checkForAttackInput();

        if(attackTime > 0)
        {
            attackTime -= Time.deltaTime;
        }
        
        if(attackTime <= 0)
        {
            currentAttack = Attacks[0];
            
            playerChar.setPlayerExtraSpeed(0f);
        }
    }

    public void removeEndOfString(string attackName)
    {
        for(int i = 0; i < Attacks.Length; i++)
        {
            if(Attacks[i].AttackName == attackName)
            {
                Attacks[i].endOfAttackString = false;
                break;
            }
        }
    }

    public void setUpAttackLinks()
    {
        for(int i = Attacks.Length - 1; i >= 0; i--)
        {
            Debug.Log(i);
            string nameOfAttack = Attacks[i].AttackName;

            for(int j = Attacks.Length - 1; j >= 0; j--)
            {
                if(nameOfAttack == Attacks[j].nextLightAttack || nameOfAttack == Attacks[j].nextHeavyAttack)
                {
                    Attacks[i].previousAttackName = Attacks[j].AttackName;
                }
            }
        }
    }

    //checked throughout the update method, detects when the player uses any of the attack buttons
    //Each branch will execute the corresponding attack check for the button (Light, Heavy, downAttack, etc if there are any more)
    public void checkForAttackInput()
    {
        if (Input.GetButtonDown("LightAtt"))
        {
            checkLightAttack();
        } else if (Input.GetButtonDown("HeavyAtt"))
        {
            checkHeavyAttack();
        } else if(Input.GetButtonDown("LightAtt") && Input.GetButton("Down"))
        {
            checkDownAttack();
        }
    }

    public void checkLightAttack()
    {
        foreach (CombatAttacks attack in Attacks)
        {
            if(currentAttack.nextLightAttack == attack.AttackName)
            {
                if (attack.isUnlocked)
                {
                    isAttacking = true;
                    currentAttack = attack;
                    attackTime = currentAttack.attackDur;

                    attackEnemy();

                    checkMovement();

                    

                    if (currentAttack.endOfAttackString)
                    {
                        currentAttack = Attacks[0];
                        return;
                    }
                }
            }
        }
    }

    public void checkHeavyAttack()
    {
        foreach(CombatAttacks attack in Attacks)
        {
            if(currentAttack.nextHeavyAttack == attack.AttackName)
            {
                if (attack.isUnlocked)
                {
                    Debug.Log(attack.AttackName);
                    currentAttack = attack;
                    attackTime = currentAttack.attackDur;
                    attackEnemy();
                    checkMovement();

                    if (currentAttack.endOfAttackString)
                    {
                        currentAttack = Attacks[0];
                    }
                    break;
                }
            }
        }
    }

    public void checkDownAttack()
    {
        foreach (CombatAttacks attack in Attacks)
        {
            if (currentAttack.nextDownAttack == attack.AttackName)
            {
                if (attack.isUnlocked)
                {
                    currentAttack = attack;
                    attackTime = currentAttack.attackDur;
                    attackEnemy();
                    checkMovement();

                    if (currentAttack.endOfAttackString)
                    {
                        currentAttack = Attacks[0];
                    }
                }
            }
        }
    }

    public void checkMovement()
    {
        if (currentAttack.willMoveHor)
        {
            bonusSpeed = currentAttack.movementChange.x;
            playerChar.setPlayerExtraSpeed(bonusSpeed);
        }

        if (currentAttack.willMoveVert)
        {
            rb.velocity = Vector2.up * currentAttack.movementChange.y;
        }
    }

    public void attackEnemy()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayer);

        foreach(Collider2D detectedEnemy in hitEnemies)
        {
            HurtEnemy damagedEnemy = detectedEnemy.GetComponent<HurtEnemy>();
            damagedEnemy.hurtEnemyFunc(currentAttack.knockback.willKnockback, currentAttack.damage, .01f, currentAttack.knockback.knockbackForce, rb.transform);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }


}
