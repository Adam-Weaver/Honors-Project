using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    public string characterName;

    public int maxHp;
    public int currentHp;
    public int Str;
    public int Mag;
    public int Skl;
    public int Spd;
    public int Luck;
    public int Def;
    public int Res;
    public int Mov;

    public int hasMoved;
    public int isEnemy;

    public List<int> growths = new List<int>();
    
    public int level;
    public int exp;

    public int weaponRange;
    public int weaponMt;
    public int hit;
    public int crit;
    public int isMagic;

    public List<int> statGains;

    public GameObject self;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (currentHp <= 0)
        {
            Destroy(self);
        }
        if (exp >= 100)
        {
            level += 1;
            exp -= 100;

            int i = 0;
            foreach (int growth in growths)
            {
                int didGrow = Random.Range(0, 100);
                if (didGrow < growth)
                {
                    statGains[i] = 1;

                    switch (i)
                    {
                        case 0:
                            maxHp += 1;
                            currentHp += 1;
                            break;
                        case 1:
                            Str += 1;
                            break;
                        case 2:
                            Mag += 1;
                            break;
                        case 3:
                            Skl += 1;
                            break;
                        case 4:
                            Spd += 1;
                            break;
                        case 5:
                            Luck += 1;
                            break;
                        case 6:
                            Def += 1;
                            break;
                        case 7:
                            Res += 1;
                            break;
                    }
                }
                else
                {
                    statGains[i] = 0;
                }
                i++;
            }   
        }
    }
}
