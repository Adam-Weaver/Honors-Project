using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Tilemaps;

public class Player_Cursor_Controls : MonoBehaviour
{
    public GameObject gameController;

    public float moveSpeed = 20f;
    public Transform moveTarget;

    public LayerMask mapEdge;
    public LayerMask selectable;
    public LayerMask enemies;

    public Tilemap validSpaceMap;

    public GameObject currentlySelectedUnit;

    public GameObject buttonCanvas;
    public List<Button> buttonList;
    public int selectedButtonIndex;

    public GameObject forecastCanvas;
    public List<Text> forecastLabelList;

    public GameObject victoryCanvas;
    public GameObject defeatCanvas;

    public GameObject playerTurnCanvas;
    public GameObject enemyTurnCanvas;

    public GameObject hoverCanvas;
    public List<Text> statLabelList;

    public GameObject levelUpCanvas;
    public GameObject levelUpInfoCanvas;
    public List<Text> levelUpInfoList;

    Rigidbody2D rb;
    BoxCollider2D col;

    bool isMovingUnit;
    bool isChoosingAttackTarget;
    Vector3 selectedUnitLastPos;

    public List<GameObject> attackableTargets;
    public int attackTargetIndex;

    int allyDmg;
    int allyHit;
    int allyCrit;
    int enemyDmg;
    int enemyHit;
    int enemyCrit;
    bool allyDoubles = false;
    bool enemyDoubles = false;

    bool didShowGrowths = false;

    // Start is called before the first frame update
    void Start()
    {
        currentlySelectedUnit = null;
        moveTarget.parent = null;
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        isMovingUnit = false;
        isChoosingAttackTarget = false;
        selectedButtonIndex = -1;
        attackTargetIndex = -1;
        attackableTargets = new List<GameObject>();


        WipeValidSpaceMap();
    }

    // Update is called once per frame
    void Update()
    {
        if (levelUpCanvas.active)
        {
            hoverCanvas.active = false;
            
            if (Input.GetKeyDown(KeyCode.Space))
            {
                CharacterStats currStats = currentlySelectedUnit.GetComponent<CharacterStats>();

                levelUpInfoList[0].GetComponent<Text>().text = "" + currStats.characterName;
                levelUpInfoList[9].GetComponent<Text>().text = "" + currStats.level;

                levelUpInfoList[1].GetComponent<Text>().text = "+" + currStats.statGains[0];
                levelUpInfoList[2].GetComponent<Text>().text = "+" + currStats.statGains[1];
                levelUpInfoList[3].GetComponent<Text>().text = "+" + currStats.statGains[2];
                levelUpInfoList[4].GetComponent<Text>().text = "+" + currStats.statGains[3];
                levelUpInfoList[5].GetComponent<Text>().text = "+" + currStats.statGains[4];
                levelUpInfoList[6].GetComponent<Text>().text = "+" + currStats.statGains[5];
                levelUpInfoList[7].GetComponent<Text>().text = "+" + currStats.statGains[6];
                levelUpInfoList[8].GetComponent<Text>().text = "+" + currStats.statGains[7];

                levelUpInfoCanvas.active = true;
                levelUpCanvas.active = false;
            }
            return;
        }
        if (levelUpInfoCanvas.active)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                
                if (!didShowGrowths)
                {
                    CharacterStats currStats = currentlySelectedUnit.GetComponent<CharacterStats>();

                    levelUpInfoList[1].GetComponent<Text>().text = "" + currStats.maxHp;
                    levelUpInfoList[2].GetComponent<Text>().text = "" + currStats.Str;
                    levelUpInfoList[3].GetComponent<Text>().text = "" + currStats.Mag;
                    levelUpInfoList[4].GetComponent<Text>().text = "" + currStats.Skl;
                    levelUpInfoList[5].GetComponent<Text>().text = "" + currStats.Spd;
                    levelUpInfoList[6].GetComponent<Text>().text = "" + currStats.Luck;
                    levelUpInfoList[7].GetComponent<Text>().text = "" + currStats.Def;
                    levelUpInfoList[8].GetComponent<Text>().text = "" + currStats.Res;

                    didShowGrowths = true;
                    return;
                }
                else
                {
                    levelUpInfoCanvas.active = false;
                    currentlySelectedUnit = null;
                    didShowGrowths = false;
                }
            }
            return;
        }

        if (enemyTurnCanvas.active)
        {
            hoverCanvas.active = false;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                enemyTurnCanvas.active = false;
                transform.position = gameController.GetComponent<GameController>().enemyUnitList[0].transform.position;
                moveTarget.position = transform.position;
            }
            return;
        }

        var gameControllerScript = gameController.GetComponent<GameController>();
        if (!gameControllerScript.isPlayerTurn)
        {
            hoverCanvas.active = false;
            return;
        }

        // Move the cursor towards the move target
        transform.position = Vector3.MoveTowards(transform.position, moveTarget.position, moveSpeed * Time.deltaTime);


        if (victoryCanvas.active)
        {
            hoverCanvas.active = false;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // TODO: Implement transition to next scene after victory
                Scene scene = SceneManager.GetActiveScene();
                SceneManager.LoadScene(scene.name);
                return;
            }

            return;
        }

        if (defeatCanvas.active)
        {
            hoverCanvas.active = false;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Scene scene = SceneManager.GetActiveScene();
                SceneManager.LoadScene(scene.name);
                return;
            }
            return;
        }


        if (playerTurnCanvas.active)
        {
            hoverCanvas.active = false;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                playerTurnCanvas.active = false;
                transform.position = gameController.GetComponent<GameController>().playerUnitList[0].transform.position;
                moveTarget.position = transform.position;
            }
            return;
        }

        // Populate the unit info box with information on the current unit the player is hovering over, if one exists.
        PopulateHoverStats();
                

        // Handle the possible controls of the player: menus, moving the cursor, selecting units, attacking, etc.
        if (buttonCanvas.active || forecastCanvas.active)
        {
            if (selectedButtonIndex == -1)
            {
                selectedButtonIndex = 0;
            }
            int currIndex = 0;
            foreach (Button button in buttonList)
            {
                var image = button.GetComponent<Image>();
                if (currIndex != selectedButtonIndex)
                {
                    image.color = Color.black;
                }
                else
                {
                    image.color = Color.blue;
                }
                currIndex += 1;
            }

            if (isChoosingAttackTarget && forecastCanvas.active)
            {

                // TODO: Implement switching between available attack targets.
                if (Input.GetKeyDown(KeyCode.C))
                {
                    isChoosingAttackTarget = false;
                    ResetCombatNums();
                    buttonCanvas.active = true;
                    forecastCanvas.active = false;
                }
                else if (Input.GetKeyDown(KeyCode.Space))
                {
                    Attack();

                    if (attackableTargets[attackTargetIndex].GetComponent<CharacterStats>().currentHp <= 0)
                    {
                        currentlySelectedUnit.GetComponent<CharacterStats>().exp += 100;
                        levelUpCanvas.active = true;

                        ResetCombatNums();
                        isChoosingAttackTarget = false;
                        buttonCanvas.active = false;
                        forecastCanvas.active = false;
                        currentlySelectedUnit.GetComponent<CharacterStats>().hasMoved = 1;
                        return;
                    }
                    else
                    {
                        int currUnitLevel = currentlySelectedUnit.GetComponent<CharacterStats>().level;
                        int enemyUnitLevel = attackableTargets[attackTargetIndex].GetComponent<CharacterStats>().level;
                        
                        if (currUnitLevel <= enemyUnitLevel)
                        {
                            currentlySelectedUnit.GetComponent<CharacterStats>().exp += 30;
                        }
                        else if (currUnitLevel - enemyUnitLevel <= 2)
                        {
                            currentlySelectedUnit.GetComponent<CharacterStats>().exp += 15;
                        }
                        else
                        {
                            currentlySelectedUnit.GetComponent<CharacterStats>().exp += 5;
                        }
                        if (currentlySelectedUnit.GetComponent<CharacterStats>().exp >= 100)
                        {
                            levelUpCanvas.active = true;

                            ResetCombatNums();
                            isChoosingAttackTarget = false;
                            buttonCanvas.active = false;
                            forecastCanvas.active = false;
                            currentlySelectedUnit.GetComponent<CharacterStats>().hasMoved = 1;
                            return;
                        }   
                    }
                    currentlySelectedUnit.GetComponent<CharacterStats>().hasMoved = 1;
                    currentlySelectedUnit = null;
                    ResetCombatNums();

                    isChoosingAttackTarget = false;
                    buttonCanvas.active = false;
                    forecastCanvas.active = false;
                    
                }
                else if (Mathf.Abs(Input.GetAxisRaw("Vertical")) == 1f)
                {
                    if (Input.GetAxisRaw("Vertical") > 0)
                    {
                        if (attackTargetIndex > 0)
                        {
                            attackTargetIndex -= 1;
                            ForecastAttack();
                        }
                    }
                    else if (Input.GetAxisRaw("Vertical") < 0)
                    {
                        if (attackTargetIndex + 1 < attackableTargets.Count)
                        {
                            attackTargetIndex += 1;
                            ForecastAttack();
                        }
                    }
                }
                return;
            }
            if (!isChoosingAttackTarget)
            {
                bool moveVer = Mathf.Abs(Input.GetAxisRaw("Vertical")) == 1f;
                if (moveVer)
                {
                    if (Input.GetAxisRaw("Vertical") == 1f)
                    {
                        if (selectedButtonIndex >= 1)
                        {
                            selectedButtonIndex -= 1;
                        }
                    }
                    else if (Input.GetAxisRaw("Vertical") == -1f)
                    {
                        if (selectedButtonIndex < buttonList.Count - 1)
                        {
                            selectedButtonIndex += 1;
                        }
                    }
                }

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    switch (selectedButtonIndex)
                    {
                        case 0:
                            if (!CanAttack(currentlySelectedUnit.GetComponent<CharacterStats>().weaponRange))
                            {
                                selectedButtonIndex = 1;
                            }
                            else
                            {
                                buttonCanvas.active = false;
                                FindAttackableTargets(currentlySelectedUnit.GetComponent<CharacterStats>().weaponRange);
                                attackTargetIndex = 0;
                                ForecastAttack();
                                isChoosingAttackTarget = true;
                            }
                            break;
                        case 1:
                            currentlySelectedUnit.GetComponent<CharacterStats>().hasMoved = 1;
                            currentlySelectedUnit = null;
                            selectedButtonIndex = -1;
                            buttonCanvas.active = false;
                            break;
                        default:
                            break;
                    }
                }
                else if (Input.GetKeyDown(KeyCode.C))
                {
                    currentlySelectedUnit.transform.position = selectedUnitLastPos;
                    buttonCanvas.active = false;
                    selectedButtonIndex = -1;
                    ActivateValidSpaceMap();
                }
            }
            else
            {

            }
            
        }
        else
        {
            if (!isMovingUnit && Vector3.Distance(transform.position, moveTarget.position) <= 0.02f)
            {
                // Store whether the player is fully pressing the arrow keys to simplify later code
                bool moveHor = Mathf.Abs(Input.GetAxisRaw("Horizontal")) == 1f;
                bool moveVer = Mathf.Abs(Input.GetAxisRaw("Vertical")) == 1f;


                // If the player is currently pressing any of the arrow keys fully, prepare to move the cursor based on what is pressed.
                if (moveHor || moveVer)
                {
                    Vector3 targetPosition = moveTarget.position;
                    if (moveHor && moveVer)
                    {
                        targetPosition = moveTarget.position + new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0);
                    }
                    else
                    {
                        if (moveVer)
                        {
                            targetPosition = moveTarget.position + new Vector3(0, Input.GetAxisRaw("Vertical"), 0);
                        }
                        else
                        {
                            targetPosition = moveTarget.position + new Vector3(Input.GetAxisRaw("Horizontal"), 0, 0);
                        }
                    }
                    if (!Physics2D.OverlapCircle(targetPosition, 0.1f, mapEdge) && currentlySelectedUnit == null)
                    {
                        moveTarget.position = targetPosition;
                    }
                    /*
                     * TODO: Implement pathing.
                     * 1) Track each position the cursor has gone to since selecting the unit (can add each target position
                     *      to a list of size s [where s is the move stat of the selected unit]). Keep an int to track the number of
                     *      spaces that have been used up.
                     * 2) When attempting to make a move, check it against map edge, then check the spaces int. If spaces int >= s,
                     *      check whether the distance target and currUnitPos (count x change and count y change) is <= s.
                     *          a) If distance <= s, erase ALL entries in the current list and form a new set of list entries by creating positions
                     *              by changing currUnitPos's x the needed number of times, then change the result's y the needed number of times.
                     *              WHEN DOING SO, RECALCULATE THE SPACES INT.
                     *          b) If distance > s, block the movement attempt.
                     * 3) After otherwise confirming the validity of a target position change, go through the path list and check if the
                     *      new target position is in the list. If so, set all entries after that index to null and adjust the spaces int accordingly.
                     * 4) If the move is valid per steps 2 and 3, check if the move would enter a forest tile. If so, check to ensure that
                     *      spaces int + 2 <= s.
                     *          a) If spaces int + 2 <= s, allow the move.
                     *          b) If spaces int + 2 > s, block the movement attempt.
                     * 5) When a cursor movement attempt is permitted, add it to the path list in the empty spot with the lowest index. If the
                     *      movement attempt would enter a forest, add it a second time.
                     * 4) When space is pressed after a unit is selected and the cursor is not on the same spot, have the unit move along the 
                     *      entries in the path list one by one, until the target is reached.
                     */
                    else if (!Physics2D.OverlapCircle(targetPosition, 0.1f, mapEdge) && (!Physics2D.OverlapCircle(targetPosition, 0.1f, enemies)))
                    {
                        float distFromUnit = 0f;

                        distFromUnit += Mathf.Abs(targetPosition.x - selectedUnitLastPos.x);
                        distFromUnit += Mathf.Abs(targetPosition.y - selectedUnitLastPos.y);

                        if (distFromUnit <= currentlySelectedUnit.GetComponent<CharacterStats>().Mov)
                        {
                            moveTarget.position = targetPosition;
                        }
                    }
                }
                else if (Input.GetKeyDown(KeyCode.Space))
                {
                    if (currentlySelectedUnit == null)
                    {
                        this.SelectUnit();

                    }
                    else
                    {
                        foreach (GameObject unit in gameController.GetComponent<GameController>().playerUnitList)
                        {
                            if (unit == currentlySelectedUnit)
                            {
                                continue;
                            }
                            if (Physics2D.IsTouching(col, unit.GetComponent<BoxCollider2D>()))
                            {
                                return;
                            }
                        }
                        foreach (GameObject unit in gameController.GetComponent<GameController>().enemyUnitList)
                        {
                            if (unit == currentlySelectedUnit)
                            {
                                continue;
                            }
                            if (Physics2D.IsTouching(col, unit.GetComponent<BoxCollider2D>()))
                            {
                                return;
                            }
                        }
                        currentlySelectedUnit.transform.position = Vector3.MoveTowards(currentlySelectedUnit.transform.position, transform.position, moveSpeed * Time.deltaTime);
                        WipeValidSpaceMap();
                        isMovingUnit = true;
                    }
                }
                else if (Input.GetKeyDown(KeyCode.C))
                {
                    if (currentlySelectedUnit != null)
                    {
                        currentlySelectedUnit.transform.position = selectedUnitLastPos;
                        currentlySelectedUnit = null;
                        WipeValidSpaceMap();
                    }
                }
            }
            else if (isMovingUnit)
            {
                currentlySelectedUnit.transform.position = Vector3.MoveTowards(currentlySelectedUnit.transform.position, transform.position, moveSpeed * Time.deltaTime);
                if (Vector3.Distance(currentlySelectedUnit.transform.position, transform.position) <= 0.02f)
                {
                    isMovingUnit = false;
                    buttonCanvas.SetActive(true);
                }
            }

        }
    }    

    private void WipeValidSpaceMap()
    {
        Color hidden = new Color(1.0f, 1.0f, 1.0f, 0.0f);

        BoundsInt bounds = validSpaceMap.cellBounds;

        for (int x = bounds.min.x; x < bounds.max.x; x++)
        {
            for (int y = bounds.min.y; y < bounds.max.y; y++)
            {
                for (int z = bounds.min.z; z < bounds.max.z; z++)
                {

                    Vector3Int currTile = new Vector3Int(x, y, z);
                    validSpaceMap.SetTileFlags(currTile, TileFlags.None);
                    validSpaceMap.SetColor(currTile, hidden);
                }
            }

        }
    }

    private void ActivateValidSpaceMap()
    {
        WipeValidSpaceMap();

        Vector3Int sourceTile = validSpaceMap.WorldToCell(selectedUnitLastPos);

        int moveRange = currentlySelectedUnit.GetComponent<CharacterStats>().Mov;

        Color hidden = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        Color visible = new Color(1.0f, 1.0f, 1.0f, 0.5f);

        for (int x = -moveRange; x <= moveRange; x++)
        {
            for (int y = -moveRange; y <= moveRange; y++)
            {
                Vector3Int currCell = sourceTile + new Vector3Int(x, y, 0);

                validSpaceMap.SetTileFlags(currCell, TileFlags.None);

                if (Math.Abs(x) + Math.Abs(y) > moveRange)
                {
                    validSpaceMap.SetColor(currCell, hidden);
                    continue;
                }
                validSpaceMap.SetColor(currCell, visible);
            }
        }

        foreach (GameObject unit in gameController.GetComponent<GameController>().enemyUnitList)
        {
            if (unit != null && unit.active)
            {
                Vector3Int unitPos = validSpaceMap.WorldToCell(unit.transform.position);
                validSpaceMap.SetColor(unitPos, hidden);
                
            }
        }
        
        foreach (GameObject unit in gameController.GetComponent<GameController>().playerUnitList)
        {
            if (unit != null && unit.active)
            {
                Vector3Int unitPos = validSpaceMap.WorldToCell(unit.transform.position);
                if (unit != currentlySelectedUnit)
                {
                    validSpaceMap.SetColor(unitPos, hidden);
                }
            }
        }
    }

    private void SelectUnit()
    {
        foreach (GameObject unit in gameController.GetComponent<GameController>().playerUnitList)
        {
            try
            {
                CharacterStats unitStats = unit.GetComponent<CharacterStats>();
                if (unitStats.hasMoved == 0 && Physics2D.IsTouching(col, unit.GetComponent<BoxCollider2D>()))
                {
                    currentlySelectedUnit = unit;
                    selectedUnitLastPos = new Vector3(currentlySelectedUnit.transform.position.x,
                            currentlySelectedUnit.transform.position.y,
                            currentlySelectedUnit.transform.position.z);
                    ActivateValidSpaceMap();
                    return;
                }
            }
            catch (Exception e)
            {
                continue;
            }
        }
    }
    private bool CanAttack(int attackRange)
    {
        if (attackRange == 1)
        {
            var up = moveTarget.position + new Vector3(0f, 1f, 0f);
            var down = moveTarget.position + new Vector3(0f, -1f, 0f);
            var left = moveTarget.position + new Vector3(-1f, 0f, 0f);
            var right = moveTarget.position + new Vector3(1f, 0f, 0f);
            return (Physics2D.OverlapCircle(up, 0.1f, enemies)
                || Physics2D.OverlapCircle(down, 0.1f, enemies)
                || Physics2D.OverlapCircle(left, 0.1f, enemies)
                || Physics2D.OverlapCircle(right, 0.1f, enemies));
        }
        else
        {
            // TODO: Implement range detection for range = 2
            return false;
        }
    }

    private void FindAttackableTargets(int weaponRange)
    {
        attackableTargets.Clear();
        foreach (GameObject unit in gameController.GetComponent<GameController>().enemyUnitList)
        {
            try
            {
                CharacterStats unitStats = unit.GetComponent<CharacterStats>();
                if (Vector3.Distance(transform.position, unit.transform.position) <= weaponRange + 0.2f)
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

    private void ForecastAttack()
    {
        if (attackableTargets.Count >= 1)
        {
            var currAllyStats = currentlySelectedUnit.GetComponent<CharacterStats>();
            var currEnemyStats = attackableTargets[attackTargetIndex].GetComponent<CharacterStats>();

            transform.position = attackableTargets[attackTargetIndex].transform.position;
            moveTarget.position = transform.position;

            allyDmg = currAllyStats.weaponMt;
            switch(currAllyStats.isMagic)
            {
                case 0:
                    allyDmg += currAllyStats.Str - currEnemyStats.Def;
                    break;
                case 1:
                    allyDmg += currAllyStats.Mag - currEnemyStats.Res;
                    break;
            }

            if (allyDmg < 0)
            {
                allyDmg = 0;
            }

            allyHit = currAllyStats.hit + currAllyStats.Skl - currEnemyStats.Skl;
            if (allyHit > 100)
            {
                allyHit = 100;
            }
            else if (allyHit < 0)
            {
                allyHit = 0;
            }

            allyCrit = currAllyStats.crit + currAllyStats.Luck - currEnemyStats.Luck;
            if (allyCrit > 100)
            {
                allyCrit = 100;
            }
            else if (allyCrit < 0)
            {
                allyCrit = 0;
            }

            enemyDmg = currEnemyStats.weaponMt;
            switch (currEnemyStats.isMagic)
            {
                case 0:
                    enemyDmg += currEnemyStats.Str - currAllyStats.Def;
                    break;
                case 1:
                    enemyDmg += currEnemyStats.Mag - currAllyStats.Res;
                    break;
            }

            if (enemyDmg < 0)
            {
                enemyDmg = 0;
            }

            enemyHit = currEnemyStats.hit + currEnemyStats.Skl - currAllyStats.Skl;
            if (enemyHit > 100)
            {
                enemyHit = 100;
            }
            else if (enemyHit < 0)
            {
                enemyHit = 0;
            }

            enemyCrit = currEnemyStats.crit + currEnemyStats.Luck - currAllyStats.Luck;
            if (enemyCrit > 100)
            {
                enemyCrit = 100;
            }
            else if (enemyCrit < 0)
            {
                enemyCrit = 0;
            }

            forecastCanvas.active = true;
            forecastLabelList[0].GetComponent<Text>().text = "" + currAllyStats.currentHp;
            forecastLabelList[1].GetComponent<Text>().text = "" + allyDmg;
            forecastLabelList[2].GetComponent<Text>().text = "" + allyHit;
            forecastLabelList[3].GetComponent<Text>().text = "" + allyCrit;
            forecastLabelList[4].GetComponent<Text>().text = "" + currEnemyStats.currentHp;
            forecastLabelList[5].GetComponent<Text>().text = "" + enemyDmg;
            forecastLabelList[6].GetComponent<Text>().text = "" + enemyHit;
            forecastLabelList[7].GetComponent<Text>().text = "" + enemyCrit;

            if (currAllyStats.Spd - 5 >= currEnemyStats.Spd)
            {
                forecastLabelList[1].GetComponent<Text>().text += " x2";
                allyDoubles = true;
            }
            else
            {
                allyDoubles = false;
            }

            if (currEnemyStats.Spd - 5 >= currAllyStats.Spd)
            {
                forecastLabelList[5].GetComponent<Text>().text += " x2";
                enemyDoubles = true;
            }
            else
            {
                enemyDoubles = false;
            }
        }
    }

    private void Attack()
    {
        try
        {
            if (attackableTargets.Count >= 1)
            {
                if (allyDmg == -1 || allyHit == -1 || allyCrit == -1
                    || enemyDmg == -1 || enemyHit == -1 || enemyCrit == -1)
                {
                    return;
                }
                int hitr1 = UnityEngine.Random.Range(0, 100);
                int hitr2 = UnityEngine.Random.Range(0, 100);
                int hitValue = (hitr1 + hitr2) / 2;
                if (hitValue <= allyHit)
                {
                    if (allyDmg >= attackableTargets[attackTargetIndex].GetComponent<CharacterStats>().currentHp)
                    {
                        gameController.GetComponent<GameController>().enemyUnitList.Remove(attackableTargets[attackTargetIndex]);
                    }
                    attackableTargets[attackTargetIndex].GetComponent<CharacterStats>().currentHp -= allyDmg;

                    int crit = UnityEngine.Random.Range(0, 100);
                    if (crit <= allyCrit)
                    {
                        if ((2 * allyDmg) >= attackableTargets[attackTargetIndex].GetComponent<CharacterStats>().currentHp)
                        {
                            gameController.GetComponent<GameController>().enemyUnitList.Remove(attackableTargets[attackTargetIndex]);
                            
                        }
                        attackableTargets[attackTargetIndex].GetComponent<CharacterStats>().currentHp -= (2 * allyDmg);
                    }
                }
                


                hitr1 = UnityEngine.Random.Range(0, 100);
                hitr2 = UnityEngine.Random.Range(0, 100);
                hitValue = (hitr1 + hitr2) / 2;
                if (hitValue <= enemyHit)
                {
                    if (enemyDmg >= currentlySelectedUnit.GetComponent<CharacterStats>().currentHp)
                    {
                        gameController.GetComponent<GameController>().playerUnitList.Remove(currentlySelectedUnit);
                    }
                    currentlySelectedUnit.GetComponent<CharacterStats>().currentHp -= enemyDmg;
                    int crit = UnityEngine.Random.Range(0, 100);
                    if (crit <= enemyCrit)
                    {
                        if ((2 * enemyDmg) >= currentlySelectedUnit.GetComponent<CharacterStats>().currentHp)
                        {
                            gameController.GetComponent<GameController>().playerUnitList.Remove(currentlySelectedUnit);
                        }
                        currentlySelectedUnit.GetComponent<CharacterStats>().currentHp -= (2 * enemyDmg);
                    }
                }


                if (allyDoubles && currentlySelectedUnit.GetComponent<CharacterStats>().currentHp > 0)
                {
                    hitr1 = UnityEngine.Random.Range(0, 100);
                    hitr2 = UnityEngine.Random.Range(0, 100);
                    hitValue = (hitr1 + hitr2) / 2;
                    if (hitValue <= allyHit)
                    {
                        if (allyDmg >= attackableTargets[attackTargetIndex].GetComponent<CharacterStats>().currentHp)
                        {
                            gameController.GetComponent<GameController>().enemyUnitList.Remove(attackableTargets[attackTargetIndex]);
                        }
                        attackableTargets[attackTargetIndex].GetComponent<CharacterStats>().currentHp -= allyDmg;

                        int crit = UnityEngine.Random.Range(0, 100);
                        if (crit <= allyCrit)
                        {
                            if ((2 * allyDmg) >= attackableTargets[attackTargetIndex].GetComponent<CharacterStats>().currentHp)
                            {
                                gameController.GetComponent<GameController>().enemyUnitList.Remove(attackableTargets[attackTargetIndex]);
                            }
                            attackableTargets[attackTargetIndex].GetComponent<CharacterStats>().currentHp -= (2 * allyDmg);
                        }
                    }
                    if (attackableTargets[attackTargetIndex].GetComponent<CharacterStats>().currentHp < 0)
                    {
                        return;
                    }
                }
                else if (enemyDoubles && attackableTargets[attackTargetIndex].GetComponent<CharacterStats>().currentHp > 0)
                {
                    hitr1 = UnityEngine.Random.Range(0, 100);
                    hitr2 = UnityEngine.Random.Range(0, 100);
                    hitValue = (hitr1 + hitr2) / 2;
                    if (hitValue <= enemyHit)
                    {
                        if (enemyDmg >= currentlySelectedUnit.GetComponent<CharacterStats>().currentHp)
                        {
                            gameController.GetComponent<GameController>().playerUnitList.Remove(currentlySelectedUnit);
                        }
                        currentlySelectedUnit.GetComponent<CharacterStats>().currentHp -= enemyDmg;
                        int crit = UnityEngine.Random.Range(0, 100);
                        if (crit <= enemyCrit)
                        {
                            if ((2 * enemyDmg) >= currentlySelectedUnit.GetComponent<CharacterStats>().currentHp)
                            {
                                gameController.GetComponent<GameController>().playerUnitList.Remove(currentlySelectedUnit);
                            }
                            currentlySelectedUnit.GetComponent<CharacterStats>().currentHp -= (2 * enemyDmg);
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

    private void ResetCombatNums()
    {
        allyDmg = -1;
        allyHit = -1;
        allyCrit = -1;
        allyDoubles = false;
        enemyDmg = -1;
        enemyHit = -1;
        enemyCrit = -1;
        enemyDoubles = false;
    }

    private void PopulateHoverStats()
    {
        GameObject hoverUnit = GetUnitAtCursor();
        CharacterStats hoverStats = null;

        if (hoverUnit == null && currentlySelectedUnit == null)
        {
            hoverCanvas.active = false;
            return;
        }
        else if (currentlySelectedUnit != null)
        {
            hoverStats = currentlySelectedUnit.GetComponent<CharacterStats>();
        }
        else
        {
            hoverStats = hoverUnit.GetComponent<CharacterStats>();
        }
        
        hoverCanvas.active = true;

        statLabelList[0].GetComponent<Text>().text = "" + hoverStats.characterName;
        statLabelList[1].GetComponent<Text>().text = "" + hoverStats.currentHp;
        statLabelList[2].GetComponent<Text>().text = "" + hoverStats.maxHp;
        statLabelList[3].GetComponent<Text>().text = "" + hoverStats.Str;
        statLabelList[4].GetComponent<Text>().text = "" + hoverStats.Mag;
        statLabelList[5].GetComponent<Text>().text = "" + hoverStats.Skl;
        statLabelList[6].GetComponent<Text>().text = "" + hoverStats.Spd;
        statLabelList[7].GetComponent<Text>().text = "" + hoverStats.Luck;
        statLabelList[8].GetComponent<Text>().text = "" + hoverStats.Def;
        statLabelList[9].GetComponent<Text>().text = "" + hoverStats.Res;
        statLabelList[10].GetComponent<Text>().text = "" + hoverStats.weaponMt;
        statLabelList[11].GetComponent<Text>().text = "" + hoverStats.hit;
        statLabelList[12].GetComponent<Text>().text = "" + hoverStats.crit;
        statLabelList[13].GetComponent<Text>().text = "" + hoverStats.Mov;

        if (hoverStats.isMagic == 0)
        {
            statLabelList[14].GetComponent<Text>().text = "Physical";
        }
        else
        {
            statLabelList[14].GetComponent<Text>().text = "Magical";
        }

        if (hoverStats.weaponRange == 1)
        {
            statLabelList[15].GetComponent<Text>().text = "1";
        }
        else
        {
            statLabelList[15].GetComponent<Text>().text = "1-2";
        }

        statLabelList[16].GetComponent<Text>().text = "" + hoverStats.exp;
        statLabelList[17].GetComponent<Text>().text = "" + hoverStats.level;
    }

    private GameObject GetUnitAtCursor()
    {
        foreach (GameObject unit in gameController.GetComponent<GameController>().playerUnitList)
        {
            if (unit != null && Physics2D.IsTouching(col, unit.GetComponent<BoxCollider2D>()))
            {
                return unit;
            }
        }

        foreach (GameObject unit in gameController.GetComponent<GameController>().enemyUnitList)
        {
            if (unit != null && Physics2D.IsTouching(col, unit.GetComponent<BoxCollider2D>()))
            {
                return unit;
            }
        }

        return null;
    }
}
