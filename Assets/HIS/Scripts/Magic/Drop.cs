using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drop : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void drop(Deck_Manage.MagicType magicType1, Deck_Manage.MagicType magicType2)
    {
        print("Drop" + magicType1.ToString() + magicType2.ToString());
    }
}
