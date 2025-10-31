using UnityEngine;

public class GameLayers : MonoBehaviour
{
    public static GameLayers i;
    public LayerMask GroundLayer;
    public float GroundCheckDistance = 0.3f;
    public LayerMask PlayerLayer;
    public int PlayerLayerInt;
    public LayerMask EnemyLayer;
    public int EnemyLayerInt;



    private void Awake()
    {
        if (i == null)
        {
            i = this;
        }
        else
        { 
            Destroy(this);
        }
    }

    private void Start()
    {
        PlayerLayerInt = (int)Mathf.Log(PlayerLayer.value, 2);
        EnemyLayerInt = (int)Mathf.Log(EnemyLayer.value, 2);
    }
}
