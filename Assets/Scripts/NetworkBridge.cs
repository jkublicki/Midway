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

        UnitManager.Unit unit = UnitManager.Instance.UnitList.FirstOrDefault(u => u.ID.Equals(unitId));
        UnitManager.Instance.MoveForward(unit);
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

        UnitManager.Unit unit = UnitManager.Instance.UnitList.FirstOrDefault(u => u.ID.Equals(unitId));
        UnitManager.Instance.MoveLeft(unit);
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

        UnitManager.Unit unit = UnitManager.Instance.UnitList.FirstOrDefault(u => u.ID.Equals(unitId));
        UnitManager.Instance.MoveRight(unit);
    }


    // >>>>>>> dokonczyc
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SubmitBeMovedServerRpc(string unitId, int destinationQ, int destinationR) //unitOrigin jako surgat ID
    {
        ServiceBeMovedClientRpc(unitId, destinationQ, destinationR);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ServiceBeMovedClientRpc(string unitId, int destinationQ, int destinationR)
    {
        UnitManager.Unit unit = UnitManager.Instance.UnitList.FirstOrDefault(u => u.ID.Equals(unitId));
        UnitManager.Instance.BeMoved(unit, new HexCoords(destinationQ, destinationR));
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
