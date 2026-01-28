using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class NetworkBridge : NetworkBehaviour
{
    public static NetworkBridge Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
    }


    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SubmitMoveForwardServerRpc(string unitId) //unitOrigin jako surgat ID
    {
        ServiceMoveForwardClientRpc(unitId);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ServiceMoveForwardClientRpc(string unitId)
    {
        Debug.Log("ServiceMoveForwardClientRpc dla jednostki " + unitId.ToString());

        UnitManager.Unit unit = SceneManagerReferences.Instance.Unit.UnitList.FirstOrDefault(u => u.ID.Equals(unitId));
        SceneManagerReferences.Instance.Unit.MoveForward(unit);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SubmitMoveLeftServerRpc(string unitId) //unitOrigin jako surgat ID
    {
        ServiceMoveLeftClientRpc(unitId);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ServiceMoveLeftClientRpc(string unitId)
    {
        Debug.Log("ServiceMoveForwardClientRpc dla jednostki " + unitId.ToString());

        UnitManager.Unit unit = SceneManagerReferences.Instance.Unit.UnitList.FirstOrDefault(u => u.ID.Equals(unitId));
        SceneManagerReferences.Instance.Unit.MoveLeft(unit);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SubmitMoveRightServerRpc(string unitId) //unitOrigin jako surgat ID
    {
        ServiceMoveRightClientRpc(unitId);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ServiceMoveRightClientRpc(string unitId)
    {
        Debug.Log("ServiceMoveForwardClientRpc dla jednostki " + unitId.ToString());

        UnitManager.Unit unit = SceneManagerReferences.Instance.Unit.UnitList.FirstOrDefault(u => u.ID.Equals(unitId));
        SceneManagerReferences.Instance.Unit.MoveRight(unit);
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
