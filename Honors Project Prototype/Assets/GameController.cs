using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;


public class GameController : MonoBehaviour
{
    public List<GameObject> playerUnitList;
    public List<GameObject> enemyUnitList;

    public List<GameObject> fallenPlayerUnitList;
    public List<GameObject> fallenEnemyUnitList;

    public GameObject mapBoss;

    public GameObject victoryCanvas;
    public GameObject defeatCanvas;
    public GameObject enemyTurnCanvas;
    public GameObject playerTurnCanvas;
    public GameObject levelUpCanvas;
    public GameObject levelUpInfoCanvas;
    public GameObject weaponUpgradeCanvas;

    public GameObject cursor;

    public bool didHaveABoss;
    public bool isPlayerTurn;

    bool canUpdate = true;

    bool isMovingEnemy;
    int indexOfMovingEnemy;
    int dmgCalcedIndex;
    int dist;

    Vector3 targetMovePosition;
    GameObject attackTarget;

    private static GameController instance;

    // Start is called before the first frame update
    void Start()
    {
        if (instance != null && instance != this)
        {
            DestroySelf();
            return;
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(transform.gameObject);

        fallenPlayerUnitList.Clear();
        fallenEnemyUnitList.Clear();

        didHaveABoss = false;
        isPlayerTurn = true;
        isMovingEnemy = false;
        indexOfMovingEnemy = -1;
        dmgCalcedIndex = -1;
        dist = 0;
        attackTarget = null;

        SceneManager.activeSceneChanged += SceneHasChanged;
    }

    void DestroySelf()
    {
        for (int i = playerUnitList.Count - 1; i >= 0; i--)
        {
            Destroy(playerUnitList[i]);
        }

        for (int i = enemyUnitList.Count - 1; i >= 0; i--)
        {
            Destroy(enemyUnitList[i]);
        }

        Destroy(transform.gameObject);
    }

    void SceneHasChanged(Scene current, Scene next)
    {
        // If we are on a dialogue scene, handled by a "Dialogue Controller" object, don't try to reset.
        GameObject dialogueController = GameObject.Find("Dialogue Controller");
        if (dialogueController != null)
        {
            return;
        }
        

        enemyTurnCanvas.active = false;
        playerTurnCanvas.active = true;

        // Reposition player units into the available slots of the new level.
        int currSlotID = 1;

        foreach (GameObject unit in playerUnitList)
        {
            GameObject slot = GameObject.Find("Slot " + currSlotID);
            if (slot != null)
            {
                unit.transform.position = slot.transform.position;

                var unitStats = unit.GetComponent<CharacterStats>();
                unitStats.spawnPoint = unit.transform.position;
                unitStats.currentHp = unitStats.maxHp;
                unitStats.hasMoved = 0;
                if (currSlotID == 1)
                {
                    cursor.transform.position = slot.transform.position;
                    cursor.GetComponent<Player_Cursor_Controls>().moveTarget.position = cursor.transform.position;
                }

                currSlotID += 1;
                continue;
            }
            break;
        }

        // Add the new player units to the player units list
        GameObject newPlayerUnits = GameObject.Find("newPlayerUnits");
        if (newPlayerUnits == null)
        {
            return;
        }

        foreach (Transform child in newPlayerUnits.transform)
        {
            GameObject newUnit = child.gameObject;
            playerUnitList.Add(newUnit);
        }

        // Add the new enemies to the enemy units list
        GameObject newEnemies = GameObject.Find("enemies");
        if (newEnemies == null)
        {
            return;
        }

        foreach (Transform child in newEnemies.transform)
        {
            GameObject newUnit = child.gameObject;
            enemyUnitList.Add(newUnit);
        }

        // Find the boss of the map, if there is one.
        GameObject bossHolder = GameObject.Find("Boss Holder");
        if (bossHolder != null)
        {
            Boss bossScript = bossHolder.GetComponent<Boss>();
            GameObject bossUnit = bossScript.mapBoss;

            if (bossUnit != null)
            {
                mapBoss = bossUnit;
            }
        }


        fallenPlayerUnitList.Clear();
        fallenEnemyUnitList.Clear();

        canUpdate = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (instance != null && instance != this)
        {
            DestroySelf();
            return;
        }

        if (!canUpdate)
        {
            return;
        }

        if (levelUpCanvas.active || levelUpInfoCanvas.active || weaponUpgradeCanvas.active)
        {
            return;
        }

        if (mapBoss != null)
        {
            didHaveABoss = true;
        }


        // Handle dead units
        for (int i = playerUnitList.Count - 1; i >= 0; i--)
        {
            var unit = playerUnitList[i];
            var unitStats = unit.GetComponent<CharacterStats>();

            if (unitStats.currentHp <= 0)
            {
                fallenPlayerUnitList.Add(unit);
                playerUnitList.Remove(unit);
            }
        }


        // Victory and Defeat Conditions

        // If the map had a boss, and the boss is nonexistent (has been killed), you win!
        if (didHaveABoss && mapBoss == null)
        {
            victoryCanvas.active = true;
            return;
        }



        // if the player units are all gone, you lose...
        if (playerUnitList.Count == 0)
        {
            defeatCanvas.active = true;
            return;
        }

        // If the enemy units are all gone, you win!
        if (enemyUnitList.Count == 0)
        {
            playerTurnCanvas.active = false;
            enemyTurnCanvas.active = false;
            defeatCanvas.active = false;
            victoryCanvas.active = true;
            canUpdate = false;
            return;
        }
        else
        {
            bool enemiesAllDead = true;
            for (int i = 0; i < enemyUnitList.Count; i++)
            {
                if (enemyUnitList[i] != null && enemyUnitList[i].active)
                {
                    enemiesAllDead = false;
                    break;
                }
            }
            if (enemiesAllDead)
            {
                victoryCanvas.active = true;
                playerTurnCanvas.active = false;
                enemyTurnCanvas.active = false;
                defeatCanvas.active = false;

                canUpdate = false;
            }
            else
            {
                victoryCanvas.active = false;
            }
        }
        
        if (enemyTurnCanvas.active)
        {
            return;
        }

        /**
        if (defeatCanvas.active || victoryCanvas.active || enemyTurnCanvas.active || playerTurnCanvas.active)
        {
            return;
        }
         **/
        if (isPlayerTurn)
        {
            if (indexOfMovingEnemy == -1)
            {
                playerTurnCanvas.active = true;
                indexOfMovingEnemy -= 1;
                return;
            }
             
            try
            {
                for (int i = enemyUnitList.Count - 1; i >= 0; i--) 
                {
                    var unit = enemyUnitList[i];
                    var unitStats = unit.GetComponent<CharacterStats>();

                    if (unitStats.currentHp <= 0)
                    {
                        fallenEnemyUnitList.Add(unit);
                        enemyUnitList.Remove(unit);
                    }
                }
            }
            catch (Exception e)
            {

            }


            // Check if it is still the player's turn.
            isPlayerTurn = false;
            for (int i = 0; i < playerUnitList.Count; i++)
            {
                if (playerUnitList[i] == null)
                {
                    continue;
                }
                var unitStats = playerUnitList[i].GetComponent<CharacterStats>();
                if (!playerUnitList[i].active)
                {
                    continue;
                }
                if (unitStats.hasMoved == 0)
                {
                    isPlayerTurn = true;
                    break;
                }
            }

            // If the player's turn has just ended, do some prep work: reset player unit has moved values, and tell self that it's enemy turn
            if (!isPlayerTurn)
            {
                for (int i = 0; i < playerUnitList.Count; i++)
                {
                    var unitStats = playerUnitList[i].GetComponent<CharacterStats>();
                    unitStats.hasMoved = 0;
                }
                indexOfMovingEnemy = 0;
                // enemyUnitList = enemyUnitList.Where(x => x != null).ToList();
                foreach (GameObject unit in enemyUnitList)
                {
                    var unitStats = unit.GetComponent<CharacterStats>();

                    if (unitStats.currentHp <= 0)
                    {
                        fallenEnemyUnitList.Add(unit);
                        enemyUnitList.Remove(unit);
                    }
                }

                enemyTurnCanvas.active = true;
            }
        }
        else
        {
            // If index of moving enemy is outside of enemy list length, enemy turn has ended. Perform clean-up and pass back to player.
            if (indexOfMovingEnemy >= enemyUnitList.Count)
            {
                for (int i = 0; i < enemyUnitList.Count; i++)
                {
                    if (enemyUnitList[i] != null)
                    {
                        var unitStats = enemyUnitList[i].GetComponent<CharacterStats>();
                        unitStats.hasMoved = 0;
                    }

                }

                try
                {
                    for (int i = enemyUnitList.Count - 1; i >= 0; i--)
                    {
                        var unit = enemyUnitList[i];
                        var unitStats = unit.GetComponent<CharacterStats>();

                        if (unitStats.currentHp <= 0)
                        {
                            fallenEnemyUnitList.Add(unit);
                            enemyUnitList.Remove(unit);
                        }
                    }
                }
                catch (Exception e)
                {

                }

                indexOfMovingEnemy = -1;
                isPlayerTurn = true;
                return;
            }

            GameObject currEnemy = enemyUnitList[indexOfMovingEnemy];

            if (currEnemy == null || !currEnemy.active)
            {
                indexOfMovingEnemy += 1;
                return;
            }
            var currEnemyStats = currEnemy.GetComponent<CharacterStats>();

            // If it's not the player's turn, take the enemy's turn. 
            // This part of the method handles the current index enemy before moving to the next
            if (!isMovingEnemy)
            {
                var currEnemyX = currEnemy.transform.position.x;
                var currEnemyY = currEnemy.transform.position.y;

                List<GameObject> attackablePlayerUnits = new List<GameObject>();

                FindAttackableTargets(currEnemyStats.weaponRange, currEnemyStats.Mov, currEnemyX, currEnemyY, attackablePlayerUnits);

                if (attackablePlayerUnits.Count <= 0)
                {
                    currEnemyStats.hasMoved = 1;
                    indexOfMovingEnemy += 1;
                }
                else
                {
                    attackTarget = null;
                    int highestDamage = -1;
                    for (int i = 0; i < attackablePlayerUnits.Count; i++)
                    {
                        int currDamage = ForecastDamage(currEnemy, attackablePlayerUnits[i]);
                        if (currDamage > highestDamage)
                        {
                            highestDamage = currDamage;
                            attackTarget = attackablePlayerUnits[i];
                        }
                    }
                        

                    if (attackTarget != null)
                    {
                        List<Vector3> possibleEndPoints = new List<Vector3>();
                        Vector3 basePosition = attackTarget.transform.position;
                        Vector3 currEnemyPos = currEnemy.transform.position;

                        if (currEnemyStats.weaponRange == 2)
                        {
                            possibleEndPoints.Add(new Vector3(basePosition.x + 1, basePosition.y + 1, 0f));
                            possibleEndPoints.Add(new Vector3(basePosition.x + 1, basePosition.y - 1, 0f));
                            possibleEndPoints.Add(new Vector3(basePosition.x - 1, basePosition.y + 1, 0f));
                            possibleEndPoints.Add(new Vector3(basePosition.x - 1, basePosition.y - 1, 0f));
                        }

                        possibleEndPoints.Add(new Vector3(basePosition.x + 1, basePosition.y, 0f));
                        possibleEndPoints.Add(new Vector3(basePosition.x - 1, basePosition.y, 0f));
                        possibleEndPoints.Add(new Vector3(basePosition.x, basePosition.y + 1, 0f));
                        possibleEndPoints.Add(new Vector3(basePosition.x, basePosition.y - 1, 0f));

                        possibleEndPoints = possibleEndPoints.Where(x => ((Mathf.Abs(x.x - currEnemyPos.x)) + (Mathf.Abs(x.y - currEnemyPos.y))) <= currEnemyStats.Mov).ToList();

                        foreach (GameObject unit in playerUnitList)
                        {
                            if (unit != null)
                            {
                                possibleEndPoints = possibleEndPoints.Where(x => x != unit.transform.position).ToList();
                            }                       
                        }
                        foreach (GameObject unit in enemyUnitList)
                        {
                            if (unit != null && unit != currEnemy)
                            {
                                possibleEndPoints = possibleEndPoints.Where(x => x != unit.transform.position).ToList();
                            }
                        }

                        if (possibleEndPoints.Contains(currEnemyPos))
                        {
                            targetMovePosition = currEnemyPos;
                            isMovingEnemy = true;
                        }

                        switch(possibleEndPoints.Count)
                        {
                            case 0:
                                currEnemyStats.hasMoved = 1;
                                indexOfMovingEnemy += 1;
                                dist = 0;
                                break;
                            case 1:
                                targetMovePosition = possibleEndPoints[0];
                                currEnemyPos = Vector3.MoveTowards(currEnemy.transform.position, targetMovePosition, 10 * Time.deltaTime);
                                isMovingEnemy = true;
                                dist += (int) Math.Abs(basePosition.x - targetMovePosition.x);
                                dist += (int) Math.Abs(basePosition.y - targetMovePosition.y);
                                break;
                            default:
                                targetMovePosition = possibleEndPoints[0];
                                currEnemyPos = Vector3.MoveTowards(currEnemy.transform.position, targetMovePosition, 10 * Time.deltaTime);
                                isMovingEnemy = true;
                                dist += (int) Math.Abs(basePosition.x - targetMovePosition.x);
                                dist += (int) Math.Abs(basePosition.y - targetMovePosition.y);
                                break;
                        }                        
                    }
                    
                }
            }
            else
            {
                currEnemy.transform.position = Vector3.MoveTowards(currEnemy.transform.position, targetMovePosition, 10 * Time.deltaTime);
                
                if (Vector3.Distance(currEnemy.transform.position, targetMovePosition) <= 0.02f)
                {
                    CharacterStats currPlayerStats = attackTarget.GetComponent<CharacterStats>();
                    var currEnemyLevel = currEnemyStats.level;
                    var currPlayerLevel = currPlayerStats.level;
                    Attack(currEnemy, attackTarget, dist);
                    dist = 0;
                    try
                    {
                        currEnemyStats.hasMoved = 1;
                        if (currPlayerStats.currentHp > 0)
                        {
                            if (currEnemyStats.currentHp <= 0)
                            {
                                currPlayerStats.exp += 100;
                                levelUpCanvas.active = true;

                            }
                            else if (currPlayerLevel <= currEnemyLevel)
                            {
                                if (currPlayerStats.exp >= 70)
                                {
                                    levelUpCanvas.active = true;
                                }
                                currPlayerStats.exp += 30;
                            }
                            else if (currPlayerLevel - currEnemyLevel <= 2)
                            {
                                if (currPlayerStats.exp >= 85)
                                {
                                    levelUpCanvas.active = true;
                                }
                                currPlayerStats.exp += 15;
                            }
                            else
                            {
                                if (currPlayerStats.exp >= 95)
                                {
                                    levelUpCanvas.active = true;
                                }
                                currPlayerStats.exp += 5;
                            }
                        }
                    }
                    catch (Exception e)
                    {

                    }
                    if (levelUpCanvas.active)
                    {
                        Player_Cursor_Controls cursorScript = cursor.GetComponent<Player_Cursor_Controls>();
                        cursorScript.currentlySelectedUnit = attackTarget;
                        cursor.transform.position = cursorScript.currentlySelectedUnit.transform.position;
                        cursorScript.moveTarget.position = cursor.transform.position;
                    }
                    isMovingEnemy = false;
                    indexOfMovingEnemy += 1;
                }
            }

        }
        
    }

    private void FindAttackableTargets(int weaponRange, int unitMv, float sourceUnitX, float sourceUnitY, List<GameObject> attackableTargets)
    {
        attackableTargets.Clear();
        foreach (GameObject unit in playerUnitList)
        {
            try
            {
                float distFromUnit = 0f;

                distFromUnit += Mathf.Abs(unit.transform.position.x - sourceUnitX);
                distFromUnit += Mathf.Abs(unit.transform.position.y - sourceUnitY);

                if (distFromUnit <= weaponRange + unitMv + 0.2f)
                {
                    attackableTargets.Add(unit);
                }
            }
            catch (Exception e)
            {
                continue;
            }
        }
    }

    private int ForecastDamage(GameObject enemyUnit, GameObject playerUnit)
    {
        var currPlayerStats = playerUnit.GetComponent<CharacterStats>();
        var currEnemyStats = enemyUnit.GetComponent<CharacterStats>();

        int enemyDmg = currEnemyStats.weaponMt;
        switch (currEnemyStats.isMagic)
        {
            case 0:
                enemyDmg += currEnemyStats.Str - currPlayerStats.Def;
                break;
            case 1:
                enemyDmg += currEnemyStats.Mag - currPlayerStats.Res;
                break;
        }

        if (enemyDmg < 0)
        {
            enemyDmg = 0;
        }

        if (currEnemyStats.Spd - 5 >= currPlayerStats.Spd)
        {
            return enemyDmg * 2;
        }
        return enemyDmg;
    }

    private void Attack(GameObject enemyUnit, GameObject playerUnit, int dist)
    {
        var enemyStats = enemyUnit.GetComponent<CharacterStats>();
        var playerStats = playerUnit.GetComponent<CharacterStats>();
        
        int enemyDmg = enemyStats.weaponMt;
        switch (enemyStats.isMagic)
        {
            case 0:
                enemyDmg += enemyStats.Str - playerStats.Def;
                break;
            case 1:
                enemyDmg += enemyStats.Mag - playerStats.Res;
                break;
        }

        if (enemyDmg < 0)
        {
            enemyDmg = 0;
        }

        int playerDmg = playerStats.weaponMt;
        switch (playerStats.isMagic)
        {
            case 0:
                playerDmg += playerStats.Str - enemyStats.Def;
                break;
            case 1:
                playerDmg += playerStats.Mag - enemyStats.Res;
                break;
        }
        if (playerDmg < 0)
        {
            playerDmg = 0;
        }

        int enemyHit = enemyStats.hit + enemyStats.Skl - playerStats.Skl;
        if (enemyHit > 100)
        {
            enemyHit = 100;
        }
        else if (enemyHit < 0)
        {
            enemyHit = 0;
        }

        int enemyCrit = enemyStats.crit + enemyStats.Luck - playerStats.Luck;
        if (enemyCrit > 100)
        {
            enemyCrit = 100;
        }
        else if (enemyCrit < 0)
        {
            enemyCrit = 0;
        }

        int playerHit = playerStats.hit + playerStats.Skl - enemyStats.Skl;
        if (playerHit > 100)
        {
            playerHit = 100;
        }
        else if (playerHit < 0)
        {
            playerHit = 0;
        }

        int playerCrit = playerStats.crit + playerStats.Luck - enemyStats.Luck;
        if (playerCrit > 100)
        {
            playerCrit = 100;
        }
        else if (playerCrit < 0)
        {
            playerCrit = 0;
        }

        if (enemyStats.weaponRange > playerStats.weaponRange && dist == 2)
        {
            playerHit = -1;
        }

        try
        {
            System.Random rng = new System.Random();

            int hit1 = rng.Next(0, 101);
            int hit2 = rng.Next(0, 101);
            float hitValue = (hit1 + hit2) / 2.0f;

            int crit = -1;

            if (hitValue <= enemyHit)
            {
                playerStats.currentHp -= enemyDmg;

                crit = rng.Next(0, 101);
                if (crit <= enemyCrit)
                {
                    playerStats.currentHp -= (2 * enemyDmg);
                }
            }

            if (playerStats.currentHp <= 0)
            {
                fallenPlayerUnitList.Add(playerUnit);
                playerUnitList.Remove(playerUnit);
                return; 
            }

            hit1 = rng.Next(0, 101);
            hit2 = rng.Next(0, 101);
            hitValue = (hit1 + hit2) / 2f;

            crit = -1;
            if (hitValue <= playerHit)
            {
                enemyStats.currentHp -= playerDmg;

                crit = rng.Next(0, 101);
                if (crit <= playerCrit)
                {
                    enemyStats.currentHp -= (2 * playerDmg);
                }
            }

            if (enemyStats.currentHp > 0 && playerStats.currentHp > 0)
            {
                if (enemyStats.Spd - 5 >= playerStats.Spd)
                {
                    hit1 = rng.Next(0, 101);
                    hit2 = rng.Next(0, 101);
                    hitValue = (hit1 + hit2) / 2.0f;

                    crit = -1;

                    if (hitValue <= enemyHit)
                    {
                        playerStats.currentHp -= enemyDmg;

                        crit = rng.Next(0, 101);
                        if (crit <= enemyCrit)
                        {
                            playerStats.currentHp -= (2 * enemyDmg);
                        }
                    }
                }
                else if (playerStats.Spd - 5 >= enemyStats.Spd)
                {
                    hit1 = rng.Next(0, 101);
                    hit2 = rng.Next(0, 101);
                    hitValue = (hit1 + hit2) / 2f;

                    crit = -1;
                    if (hitValue <= playerHit)
                    {
                        enemyStats.currentHp -= playerDmg;

                        crit = rng.Next(0, 101);
                        if (crit <= playerCrit)
                        {
                            enemyStats.currentHp -= (2 * playerDmg);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            return;
        }
    }

}
